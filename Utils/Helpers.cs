using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FieldDescription = ArcGIS.Core.Data.DDL.FieldDescription;

namespace ULDKClient.Utils
{
    public class Helpers
    {




        private static readonly ILogger log = Log.ForContext<Helpers>();


        /// <summary>
        /// Checks if the feature class exists in a File geoodatabase
        /// </summary>
        /// <param name="gdbPath"></param>
        /// <param name="featureClassName"></param>
        /// <returns></returns>
        public async static Task<bool> FeatureClassExistsAsync(string gdbPath, string featureClassName)
        {

            bool result = false;
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                try
                {

                    Geodatabase fGDB = new Geodatabase(new FileGeodatabaseConnectionPath(new System.Uri(gdbPath)));
                    FeatureClassDefinition featureClassDefinition = fGDB.GetDefinition<FeatureClassDefinition>(featureClassName);

                    result = true;

                }
                catch
                {
                    // GetDefinition throws an exception if the definition doesn't exist

                }

                });

            return result;
           
           
        }

        /// <summary>
        /// Creates a new feature class in a File geodatabase
        /// </summary>
        /// <param name="gdbPath"></param>
        /// <param name="featureClassName"></param>
        /// <returns></returns>
        public async static Task<bool> CreateResultsFeatureClassAsync(string gdbPath, string featureClassName)
        {

            bool result = false;
            try
            {

                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    Geodatabase fGDB = new Geodatabase(new FileGeodatabaseConnectionPath(new System.Uri(gdbPath)));

                    SchemaBuilder schemaBuilder = new SchemaBuilder(fGDB);
                    FeatureClassDescription featureClassDescription = new FeatureClassDescription(featureClassName,
                        new List<FieldDescription>()
                        {
                      new FieldDescription("OID", FieldType.OID),
                      new FieldDescription("Date", FieldType.Date),
                      new FieldDescription("Parcel_id_long", FieldType.String),
                      new FieldDescription("Parcel_id_short", FieldType.String),
                      new FieldDescription("Voivodeship", FieldType.String),
                      new FieldDescription("County", FieldType.String),
                      new FieldDescription("Commune", FieldType.String),
                      new FieldDescription("Region", FieldType.String)

                        },
                    new ShapeDescription(GeometryType.Polygon, new SpatialReferenceBuilder(2180).ToSpatialReference()));

                    FeatureClassToken featureClassToken = schemaBuilder.Create(featureClassDescription);

                    // Build status
                    result = schemaBuilder.Build();

                    if (!result)
                    {
                        log.Fatal(string.Join(",", schemaBuilder.ErrorMessages.ToList()));

                    }

                });
        
            }
            catch (Exception ex)
            {
                log.Fatal(ex.StackTrace.ToString());
                
            }

            return result;

        }

        public static GraphicsLayer GetGraphicsLayer(Map map)
        {
            if (map == null) return null;
            var graphicsLyr = map.GetLayersAsFlattenedList().OfType<GraphicsLayer>().FirstOrDefault();
            if (graphicsLyr == null)
            {
                var graphicsLayerCreationParams = new GraphicsLayerCreationParams { Name = "ULDK", IsVisible = true };
                graphicsLyr = LayerFactory.Instance.CreateLayer<GraphicsLayer>(graphicsLayerCreationParams, map);
            }
            return graphicsLyr;
        }

        public static async Task<bool> AddGeometrytoGraphicLayerAsync(GraphicsLayer graphicsLayer, ArcGIS.Core.Geometry.Polygon geom, string parcelId)
        {

            bool result = false;
            try
            {
                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {



                    CIMStroke outlineStroke = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(255, 0, 0), 1.1, SimpleLineStyle.Solid);
                    CIMPolygonSymbol polySymbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(255, 0, 0), SimpleFillStyle.Null, outlineStroke);
                    CIMTextSymbol pointSymbol = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.RedRGB, 24, "Comic Sans MS", "Regular");

                    var polyGraphic = new CIMPolygonGraphic
                    {
                        Polygon = geom,
                        Symbol = polySymbol.MakeSymbolReference()
                    };

                    //add text symbol


                    var pointGraphic = new CIMTextGraphic
                    {
                        Shape = geom.Extent.Center,
                        Symbol = pointSymbol.MakeSymbolReference(),
                        Text = parcelId,
                        ReferenceScale = 100.1,
                        Placement = Anchor.CenterPoint

                    };

                    graphicsLayer.AddElement(polyGraphic);
                    graphicsLayer.AddElement(pointGraphic);

                    graphicsLayer.ClearSelection();

                    result = true;

                });
            }
            catch (Exception ex)
            {
                log.Information("Cannot add a graphic to the Graphics Layer.");
                log.Fatal(ex.StackTrace.ToString());

            }
            
            return result;
        }

        public static async Task<bool> AddParceltoFeatureClassAsync(Parcel parcel, string projectParentFolder)
        {
            string message = "";
            bool result = false;
            try
            {
                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {

                    Uri fgbPath = new System.Uri(System.IO.Path.Combine(projectParentFolder, "GUGIK", "GUGIK.gdb"));


                    using (Geodatabase fgdb = new Geodatabase(new FileGeodatabaseConnectionPath(fgbPath)))
                    using (FeatureClass fc = fgdb.OpenDataset<FeatureClass>("ULDK"))

                    {
                                                        
                        EditOperation editOperation = new EditOperation();
                        editOperation.Callback(context =>
                        {
                        FeatureClassDefinition parcelSiteDefinition = fc.GetDefinition();
                            using (RowBuffer rowBuffer = fc.CreateRowBuffer())
                            {
                                rowBuffer["Date"] = parcel.RequestDate;
                                rowBuffer["Parcel_id_long"] = parcel.IdLong;
                                rowBuffer["Parcel_id_short"] = parcel.Id;
                                rowBuffer["Voivodeship"] = parcel.Voivodeship;
                                rowBuffer["County"] = parcel.County;
                                rowBuffer["Commune"] = parcel.Commune;
                                rowBuffer["Region"] = parcel.Region;
                                rowBuffer[parcelSiteDefinition.GetShapeField()] = parcel.Geom;

                                Feature feature = fc.CreateRow(rowBuffer);
                                context.Invalidate(feature);

                                Project.Current.SaveEditsAsync();
                            }
                        }, fc);
                    
                        try
                        {
                            result = editOperation.Execute();
                            if (!result) {
                                message = editOperation.ErrorMessage;
                                Log.Fatal("Cannot save a parcel in a feature class.");
                                Log.Fatal(message);
                            }
                            

                        }
                        catch (GeodatabaseException exObj)
                        {
                            message = exObj.Message;

                            Log.Fatal("Cannot save a parcel in a feature class. GDB exception.");
                            Log.Fatal(message);
                        }
                        
                    }

                });
            }
            catch (Exception ex)
            {
                log.Information("Cannot add a feature to the feature class.");
                log.Fatal(ex.StackTrace.ToString());

            }

            return result;
        }
    }
}
