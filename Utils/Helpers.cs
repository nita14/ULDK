using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using FieldDescription = ArcGIS.Core.Data.DDL.FieldDescription;
using Geometry = ArcGIS.Core.Geometry.Geometry;

namespace ULDKClient.Utils
{
	public class Helpers
	{

		private static readonly ILogger log = Log.ForContext<Helpers>();



		/// <summary>
		/// Checks if the file gdb exists, if not creates it with feature class
		/// </summary>
		/// <param name="projectParentFolder"></param>
		/// <returns></returns>
		public static Task<bool> CheckOrCreateLocalGDBAsync(string projectParentFolder)
		{
			//queued task on MCT
			return QueuedTask.Run(() =>
			{
				return CheckOrCreateLocalGDBAsync(projectParentFolder);
			});
		}

		//prefer synchronous api methods
		public static bool CheckOrCreateLocalGDB(string projectParentFolder)
		{
			string gdbPath = Path.Combine(projectParentFolder, Constants.PROJECT_SUBFOLDER, Constants.GDB_NAME_WITH_EXT);
			Geodatabase fdgb;


			//check if gdb exists
			if (Directory.Exists(gdbPath))
			{
				Log.Information("File GDB already exists.");
				fdgb = new Geodatabase(new FileGeodatabaseConnectionPath(new System.Uri(gdbPath)));

				//checking if the fc is there
				bool fcExists = CheckLocalFC(projectParentFolder);
				//if not create a fc
				if (!fcExists) { return CreateResultsFeatureClass(gdbPath, Constants.FC_RESULTS_NAME); }


			}

			//does not exist...creating one
			else
			{

				Log.Information("File GDB does not exists. Creating...");
				FileGeodatabaseConnectionPath fileGdbConnectionPath =
									new FileGeodatabaseConnectionPath(new System.Uri(gdbPath));

				try
				{
					//creating gdb
					using (fdgb = SchemaBuilder.CreateGeodatabase(fileGdbConnectionPath))
					{
						//create fc
						return CreateResultsFeatureClass(gdbPath, Constants.FC_RESULTS_NAME);

					}

				}
				catch (GeodatabaseWorkspaceException)
				{
					Log.Information("File gdb already exists. Path: " + fileGdbConnectionPath.ToString());
					return true;
				}
				catch (GeodatabaseException gex)
				{
					Log.Fatal("A geodatabase-related exception has occurred.");
					Log.Fatal(gex.Message.ToLower());
					return false;
				}
				catch (Exception aex)
				{
					Log.Fatal("Another fatal error occurred.");
					Log.Fatal(aex.GetType().FullName);
					Log.Fatal(aex.Message.ToLower());
					return false;
				}

			}

			return true;
		}


		/// <summary>
		/// Check if the feature class exists in a File geoodatabase
		/// </summary>
		/// <param name="projectParentFolder"></param>
		/// <returns></returns>
		public static bool CheckLocalFC(string projectParentFolder)
		{

			Log.Information("Checking if the results feature class already exists...");
			string gdbPath = Path.Combine(projectParentFolder, Constants.PROJECT_SUBFOLDER, Constants.GDB_NAME_WITH_EXT);

			Geodatabase fGDB = new Geodatabase(new FileGeodatabaseConnectionPath(new System.Uri(gdbPath)));
			FeatureClassDefinition featureClassDefinition = fGDB.GetDefinition<FeatureClassDefinition>(Constants.FC_RESULTS_NAME);

			if (featureClassDefinition == null)
			{
				return false;

			}
			else
			{
				return true;
			}

		}

		/// <summary>
		/// Creates a new feature class in a File geodatabase
		/// </summary>
		/// <param name="gdbPath"></param>
		/// <param name="featureClassName"></param>
		/// <returns></returns>
		public static bool CreateResultsFeatureClass(string gdbPath, string featureClassName)
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
			bool buildStatus = schemaBuilder.Build();
			return buildStatus;

		}

		public static GraphicsLayer GetGraphicsLayer(Map map)
		{
			if (map == null) return null;
			var graphicsLyr = map.GetLayersAsFlattenedList().OfType<GraphicsLayer>().FirstOrDefault();
			if (graphicsLyr == null)
			{
				var graphicsLayerCreationParams = new GraphicsLayerCreationParams { Name = Constants.GRAPHICS_LAYER_NAME, IsVisible = true };
				graphicsLyr = LayerFactory.Instance.CreateLayer<GraphicsLayer>(graphicsLayerCreationParams, map);
			}
			return graphicsLyr;
		}

		public static Task<bool> AddGeometrytoGraphicLayerAsync(GraphicsLayer graphicsLayer, Polygon geom, string parcelId)
		{

			return QueuedTask.Run(() =>
			{

				CIMStroke outlineStroke = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(255, 0, 0), 1.1, SimpleLineStyle.Solid);
				CIMPolygonSymbol polySymbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(255, 0, 0), SimpleFillStyle.Null, outlineStroke);
				CIMTextSymbol pointSymbol = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.RedRGB, 14, "Comic Sans MS", "Regular");

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

				return true;

			});


		}

		public static Task<bool> AddParceltoFeatureClassAsync(Parcel parcel, string projectParentFolder)
		{
			return QueuedTask.Run(() =>
					{

						Uri fgbPath = new System.Uri(System.IO.Path.Combine(projectParentFolder, Constants.PROJECT_SUBFOLDER, Constants.GDB_NAME_WITH_EXT));


						using (Geodatabase fgdb = new Geodatabase(new FileGeodatabaseConnectionPath(fgbPath)))
						using (FeatureClass fc = fgdb.OpenDataset<FeatureClass>(Constants.FC_RESULTS_NAME))

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

							bool result = editOperation.Execute();
							if (!result)
							{
								string message = editOperation.ErrorMessage;
								Log.Fatal("Cannot save a parcel in a feature class.");
								Log.Fatal(message);
							}
							return result;


						}


					});

		}

		public static Task<bool> ProcessPolylineFromSketchAsync(Polyline polyline)
		{

			return QueuedTask.Run(async () =>
			{
				// 1. Densify the line
				Polyline polyDensified = GeometryEngine.Instance.DensifyByLength(polyline, 1) as Polyline;

				// 2. Get first vertex and fetch parcel and iterate
				while (polyDensified.Points.Count > 0)
				{
					MapPoint mp = polyDensified.Points[0];
					Parcel parcel = await GetRemoteData.GetInstance().GetParcelByPointAsync(mp);
					// 3. Add pacel to map, gl and fc
					bool isGraphicadded = await Helpers.AddGeometrytoGraphicLayerAsync(ULDKDockpaneViewModel._graphicsLayer, parcel.Geom, parcel.Id);
					bool isFeatureAdded = await Helpers.AddParceltoFeatureClassAsync(parcel, ULDKDockpaneViewModel._projectParentFolder);

					// 4. Bufer the parcel geom by 50cm
					Polygon parcelGeomBuffer = GeometryEngine.Instance.Buffer(parcel.Geom, 0.50) as Polygon;
					// 5. Perform difference densyfied line and parcel geom
					Polyline polyDensifiedNew = GeometryEngine.Instance.Difference(polyDensified, parcelGeomBuffer) as Polyline;
					polyDensified = polyDensifiedNew;
				}
				return true;

			});
		}


		public static Task<bool> ProcessPolygonFromSketchAsync(Polygon polygon)
		{

			return QueuedTask.Run(async () =>
			{
				//1. Geometry, get extent

				Envelope polyExtent = polygon.Extent;
				double polyExtentXMin = polyExtent.XMin;
				double polyExtentYMin = polyExtent.YMin;
				double polyExtentXMax = polyExtent.XMax;
				double polyExtentYMax = polyExtent.YMax;

				//2. Get all points in the extent (every 1 meter) as Multipoint
				Multipoint fishnetMp = await CreateFishnetMultiPointFromExtent(polyExtentXMin, polyExtentYMin, polyExtentXMax, polyExtentYMax);

				//3. Intesection (GE) Geometry Sketch and All Points as Multipoint 
				Multipoint fishnetCropped = GeometryEngine.Instance.Intersection(fishnetMp, polygon) as Multipoint;

				//4. Take first point in the fishnet and process the extent
				while (fishnetCropped.PointCount > 0)
				{
					Parcel parcel = await GetRemoteData.GetInstance().GetParcelByPointAsync(fishnetCropped.Points[0]);
					// 3. Add pacel to map, gl and fc
					bool isGraphicadded = await Helpers.AddGeometrytoGraphicLayerAsync(ULDKDockpaneViewModel._graphicsLayer, parcel.Geom, parcel.Id);
					bool isFeatureAdded = await Helpers.AddParceltoFeatureClassAsync(parcel, ULDKDockpaneViewModel._projectParentFolder);

					// 4. Bufer the parcel geom by 50cm
					Polygon parcelGeomBuffer = GeometryEngine.Instance.Buffer(parcel.Geom, 0.50) as Polygon;
					// 5. Perform difference densyfied line and parcel geom
					Multipoint fishnetCroppedNew = GeometryEngine.Instance.Difference(fishnetCropped, parcelGeomBuffer) as Multipoint;
					fishnetCropped = fishnetCroppedNew;

				}

				return true;
			});

		}



		public static async Task<Multipoint> CreateFishnetMultiPointFromExtent(double polyExtentXMin, double polyExtentYMin, double polyExtentXMax, double polyExtentYMax)
		{

			Multipoint fishnetMp;
			List<MapPoint> points = new List<MapPoint>();

			double horizontalSpan = (polyExtentXMax - polyExtentXMin) + 2;
			double verticalSpan = (polyExtentYMax - polyExtentYMin) + 2;

			double x = polyExtentXMin;
			double y = polyExtentYMin;

			for (int i = 0; i < horizontalSpan * verticalSpan; i++)
			{


				var point = MapPointBuilderEx.CreateMapPoint(x, y);
				points.Add(point);

				// break, if already outside of envelope
				if (x >= polyExtentXMax && y >= polyExtentYMax)
				{
					break;
				}

				if (y <= polyExtentYMax)
				{
					y = y + 1;
				}
				else

				{
					x = x + 1;
					y = polyExtentYMin;
				}



			}

			fishnetMp = MultipointBuilderEx.CreateMultipoint(points, ULDKDockpaneViewModel._sp2180);


			return fishnetMp;


		}
		/// <summary>
		/// Add a sketch geomtry to the map
		/// </summary>
		/// <param name="graphicsLayer"></param>
		/// <param name="geom"></param>
		/// <returns></returns>
		public static Task<bool> AddSketchToGraphicLayerAsync(Geometry geom)
		{
			return QueuedTask.Run(() =>
			{

				bool res = false;

				if (geom.GeometryType == GeometryType.Point)
				{
					var circlePtSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.BlackRGB, 6, SimpleMarkerStyle.Cross);
					var pointGraphic = new CIMPointGraphic
					{
						Location = geom as MapPoint,
						Symbol = circlePtSymbol.MakeSymbolReference(),
						ReferenceScale = 100.1,
						Placement = Anchor.CenterPoint

					};
					ULDKDockpaneViewModel._graphicsLayer.AddElement(pointGraphic);
					ULDKDockpaneViewModel._graphicsLayer.ClearSelection();
					res = true;
				}
				else if (geom.GeometryType == GeometryType.Polyline)
				{

					var lineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.BlackRGB, 2.0, SimpleLineStyle.Dash);
					//create a CIMGraphic 
					var graphic = new CIMLineGraphic()
					{
						Symbol = lineSymbol.MakeSymbolReference(),
						Line = geom as Polyline
					};
					ULDKDockpaneViewModel._graphicsLayer.AddElement(graphic);
					ULDKDockpaneViewModel._graphicsLayer.ClearSelection();
					res = true;


				}
				else if (geom.GeometryType == GeometryType.Polygon)
				{
					CIMStroke outlineStroke = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlackRGB, 2, SimpleLineStyle.Solid);
					var polySymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.BlackRGB, SimpleFillStyle.BackwardDiagonal, outlineStroke);
					//create a CIMGraphic 
					var graphic = new CIMPolygonGraphic()
					{
						Symbol = polySymbol.MakeSymbolReference(),
						Polygon = geom as Polygon
					};
					ULDKDockpaneViewModel._graphicsLayer.AddElement(graphic);
					ULDKDockpaneViewModel._graphicsLayer.ClearSelection();
					res = true;


				}

				return res;

			});
		}
	}
}
