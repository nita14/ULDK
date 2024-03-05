using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULDKClient.Utils
{
    class Helpers
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

        

    }
}
