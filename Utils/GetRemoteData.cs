using ArcGIS.Desktop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using System.Collections.ObjectModel;
using ArcGIS.Core.Geometry;
using System.Globalization;

namespace ULDKClient.Utils
{
    class GetRemoteData
    {

        private static readonly ILogger log = Log.ForContext<GetRemoteData>();

        /// <summary>
        /// Gets communes dictionary from a remote endpoint
        /// </summary>
        /// <returns></returns>
        public async static Task<ObservableCollection<Commune>> GetCommuneDataAsync()
        {

            ObservableCollection<Commune> communes;

            //Get data from the endpoint 
            Log.Information("Preparing Commune request...");
            EsriHttpClient esriHttpClient = new EsriHttpClient();
            esriHttpClient.Timeout = TimeSpan.FromSeconds(10);
      
            EsriHttpResponseMessage responseMessage = esriHttpClient.Get(Constants.COMMUNE_DATA_URL);
            responseMessage.EnsureSuccessStatusCode();

            var resp = await responseMessage.Content.ReadAsStringAsync();
            communes = JsonConvert.DeserializeObject<ObservableCollection<Commune>>(resp);
            return communes;
    
        }

        /// <summary>
        /// Gets regions dictionary from a remote endpoint
        /// </summary>
        /// <returns></returns>

        public async static Task<ObservableCollection<Region>> GetRegionDataAsync()
        {

            ObservableCollection<Region> regions;

            //Get data from the endpoint 
            Log.Information("Preparing Region request...");
            EsriHttpClient esriHttpClient = new EsriHttpClient();
            esriHttpClient.Timeout = TimeSpan.FromSeconds(10);

            EsriHttpResponseMessage responseMessage = esriHttpClient.Get(Constants.REGION_DATA_URL);
            responseMessage.EnsureSuccessStatusCode();

            var response = await responseMessage.Content.ReadAsStringAsync();
            regions = JsonConvert.DeserializeObject<ObservableCollection<Region>>(response);
            return regions;

        }

        /// <summary>
        /// Gets commune extent by the TERYT id
        /// </summary>
        /// <param name="communeId"></param>
        /// <returns></returns>
        public async static Task<Polygon> GetCommuneExtentByIDAsync(string communeId)
        {

            //Get data from the endpoint 
            Log.Information("Preparing Commune request...");
            EsriHttpClient esriHttpClient = new EsriHttpClient();
            esriHttpClient.Timeout = TimeSpan.FromSeconds(10);

            EsriHttpResponseMessage responseMessage = esriHttpClient.Get(Constants.FIND_COMMUNE_BY_ID_ULDK_URL + communeId);
            responseMessage.EnsureSuccessStatusCode();

            var response = await responseMessage.Content.ReadAsStringAsync();
            var status = response.Split("\n")[0];

            if (status != "0") { 
                Log.Fatal("GetCommuneExtentByIDAsync error. Status different than 0.");
                return null;
            
            }

            List<string> extentCoords = response.Split("\n")[1].Split(",").ToList();

            MapPoint minPt = MapPointBuilderEx.CreateMapPoint(Convert.ToDouble(extentCoords[0], CultureInfo.InvariantCulture), Convert.ToDouble(extentCoords[1], CultureInfo.InvariantCulture));
            MapPoint maxPt = MapPointBuilderEx.CreateMapPoint(Convert.ToDouble(extentCoords[2], CultureInfo.InvariantCulture), Convert.ToDouble(extentCoords[3], CultureInfo.InvariantCulture));

            // Create an envelope
            Envelope env = EnvelopeBuilderEx.CreateEnvelope(minPt, maxPt);
            PolygonBuilderEx polygonBuilderEx = new PolygonBuilderEx(env);
            polygonBuilderEx.SpatialReference = new SpatialReferenceBuilder(2180).ToSpatialReference();
            Polygon polygonFromEnv = polygonBuilderEx.ToGeometry() as Polygon;

            return polygonFromEnv;

        }

    }
}
