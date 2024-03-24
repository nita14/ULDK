using ArcGIS.Desktop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using ArcGIS.Core.Geometry;
using System.Globalization;
using System.Text.RegularExpressions;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ULDKClient.Utils
{
    public sealed class GetRemoteData
    {

        public static EsriHttpClient _esriHttpClient;
        private static GetRemoteData _instance;

        private GetRemoteData()
        {


            _esriHttpClient = new EsriHttpClient();
            _esriHttpClient.Timeout = TimeSpan.FromSeconds(10);


        }

        public static GetRemoteData GetInstance()
        {
            if (_instance == null)
            {
                _instance = new GetRemoteData();
            }
            return _instance;
        }
        /// <summary>
        /// Gets communes dictionary from a remote endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<List<Commune>> GetCommuneDataAsync()
        {

            List<Commune> communes;
            try
            {
                //Get data from the endpoint 

                Log.Information("Preparing Commune request...");
                EsriHttpResponseMessage responseMessage = _esriHttpClient.Get(Constants.COMMUNE_DATA_URL);
                responseMessage.EnsureSuccessStatusCode();

                var resp = await responseMessage.Content.ReadAsStringAsync();
                communes = JsonConvert.DeserializeObject<List<Commune>>(resp);
                return communes;

            }
            catch (Exception ex)
            {
                string m = ex.Message;
                Log.Fatal(m);
                return null;
            }
        }

        /// <summary>
        /// Gets regions dictionary from a remote endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<List<Region>> GetRegionDataAsync()
        {
            try
            {
                List<Region> regions;

                //Get data from the endpoint 
                Log.Information("Preparing Region request...");

                EsriHttpResponseMessage responseMessage = _esriHttpClient.Get(Constants.REGION_DATA_URL);
                responseMessage.EnsureSuccessStatusCode();

                var response = await responseMessage.Content.ReadAsStringAsync();
                regions = JsonConvert.DeserializeObject<List<Region>>(response);
                return regions;
            }
            catch (Exception ex)
            {
                string m = ex.Message;
                Log.Fatal(m);
                return null;
            }


        }

        /// <summary>
        /// Get TERC extent by the id and type (commune or region)
        /// </summary>
        /// <param name="tercId"></param>
        /// <param name="tercType"></param>
        /// <returns>Polygon with the extent</returns>
        public async Task<Polygon> GetTercExtentByIDAsync(string tercId, string tercType)
        {

            try
            {
                //Get data from the endpoint 
                Log.Information("Preparing GetTercExtentByIDAsync request...");

                string requestURL;

                if (tercType == "Commune")
                {
                    requestURL = Constants.FIND_COMMUNE_BY_ID_ULDK_URL + tercId;
                }
                else
                {
                    requestURL = Constants.FIND_REGION_BY_ID_ULDK_URL + tercId;

                }
                Log.Information("Request URL is: " + requestURL);
                EsriHttpResponseMessage responseMessage = _esriHttpClient.Get(requestURL);
                responseMessage.EnsureSuccessStatusCode();

                var response = await responseMessage.Content.ReadAsStringAsync();
                var status = response.Split("\n")[0];

                if (status != "0")
                {
                    Log.Fatal("GetTercExtentByIDAsync error. Response status different than 0.");
                    throw new Exception();

                }

                List<string> extentCoords = response.Split("\n")[1].Split(",").ToList();

                MapPoint minPt = MapPointBuilderEx.CreateMapPoint(Convert.ToDouble(extentCoords[0], CultureInfo.InvariantCulture), Convert.ToDouble(extentCoords[1], CultureInfo.InvariantCulture));
                MapPoint maxPt = MapPointBuilderEx.CreateMapPoint(Convert.ToDouble(extentCoords[2], CultureInfo.InvariantCulture), Convert.ToDouble(extentCoords[3], CultureInfo.InvariantCulture));

                // Create an envelope
                Envelope env = EnvelopeBuilderEx.CreateEnvelope(minPt, maxPt);
                PolygonBuilderEx polygonBuilderEx = new PolygonBuilderEx(env);
                polygonBuilderEx.SpatialReference = ULDKDockpaneViewModel._sp2180;
                Polygon polygonFromEnv = polygonBuilderEx.ToGeometry() as Polygon;
                return polygonFromEnv;
            }
            catch (Exception ex)
            {
                string m = ex.Message;
                Log.Fatal(m);
                return null;
            }

        }

        /// <summary>
        /// Get parcel by full id
        /// </summary>
        /// <param name="parcelIdFull">parcel full id</param>
        /// <returns>Parcel</returns>
        public async Task<Parcel> GetParcelByIdAsync(string parcelIdFull)
        {
            //Get data from the endpoint 
            try
            {
                Log.Information("Preparing GetParcelByIdAsync request...");
                Log.Information("Request URL is: " + Constants.FIND_PARCEL_BY_ID_ULDK_URL + parcelIdFull);
                EsriHttpResponseMessage responseMessage = _esriHttpClient.Get(Constants.FIND_PARCEL_BY_ID_ULDK_URL + parcelIdFull);

                responseMessage.EnsureSuccessStatusCode();

                var response = await responseMessage.Content.ReadAsStringAsync();
                var status = response.Split("\n")[0];

                if (status.Length >= 2 && status.Substring(0, 2) == "-1")
                {
                    Log.Fatal("GetParcelByIdAsync error. Cannot find the parcel with the id provided.");
                    throw new Exception();

                }

                Parcel parcel = await GetParcelFromHTTPRequestAsync(response);
                return parcel;
            }
            catch (Exception ex)
            {
                string m = ex.Message;
                Log.Fatal(m);
                return null;
            }
        }

        /// <summary>
        /// Get parcel from http response
        /// </summary>
        /// <param name="response"></param>
        /// <returns>parcel</returns>
        public static Task<Parcel> GetParcelFromHTTPRequestAsync(string response)
        {
            return QueuedTask.Run(() =>
            {
                Parcel parcel;

                //get coordinates string
                string geomCoords = response.Split("\n")[1].Split(";")[1];
                Regex regex = new Regex(@"\(([0-9]+\.[0-9]+ [0-9]+\.[0-9]+\,?)+\)");
                List<Match> rings = regex.Matches(geomCoords).ToList();

                PolygonBuilderEx polygonBuilder = new PolygonBuilderEx(ULDKDockpaneViewModel._sp2180);

                //process rings
                foreach (Match ring in rings)
                {
                    //remove parenthesis
                    string ringVal = ring.Value;
                    string ringValidated = ringVal.Substring(1, ringVal.Length - 2);
                    string[] coordPairs = ringValidated.Split(",");

                    List<MapPoint> mapPoints = new List<MapPoint>();

                    foreach (string coordPair in coordPairs)
                    {
                        string[] coords = coordPair.Split(" ");

                        MapPoint pt = MapPointBuilderEx.CreateMapPoint(Convert.ToDouble(coords[0], CultureInfo.InvariantCulture), Convert.ToDouble(coords[1], CultureInfo.InvariantCulture));
                        mapPoints.Add(pt);
                    }

                    polygonBuilder.AddPart(mapPoints);

                }


                Polygon poly = polygonBuilder.ToGeometry() as Polygon;

                //get attributes from the http response
                Regex regexattr = new Regex(@"\|.{0,}");
                List<Match> matchesattr = regexattr.Matches(geomCoords).ToList();
                string[] attrs = matchesattr[0].Value.Split("|");
                //build a parcel
                parcel = new Parcel(attrs[5].Split(".")[2], attrs[6], attrs[1], attrs[2], attrs[3], attrs[4], DateTime.Now, poly);
                return parcel;
            });


        }



        /// <summary>
        /// Get parcel based on a point from a map
        /// </summary>
        /// <param name="point">point from a sketch</param>
        /// <returns>parcel</returns>
        public async Task<Parcel> GetParcelByPointAsync(MapPoint point)
        {


            try
            {
                //Get data from the endpoint 

                Log.Information("Preparing GetParcelByPointAsync request...");
                string requestCoords = point.Coordinate2D.X.ToString() + "," + point.Coordinate2D.Y.ToString() + "," + point.SpatialReference.LatestWkid.ToString();

                Log.Information("Request URL is: " + Constants.FIND_PARCEL_BY_MAP_POINT_ULDK_URL + requestCoords);
                EsriHttpResponseMessage responseMessage = _esriHttpClient.Get(Constants.FIND_PARCEL_BY_MAP_POINT_ULDK_URL + requestCoords);

                responseMessage.EnsureSuccessStatusCode();

                var response = await responseMessage.Content.ReadAsStringAsync();
                var status = response.Split("\n")[0];

                if (status.Length >= 2 && status.Substring(0, 2) == "-1")
                {
                    Log.Fatal("GetParcelByPointAsync error. Cannot find the parcel with the xy provided.");
                    throw new Exception();

                }
                Parcel parcel = await GetParcelFromHTTPRequestAsync(response);
                return parcel;
            }
            catch (Exception ex)
            {
                string m = ex.Message;
                Log.Fatal(m);
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message);
                return null;
            }

        }
    }
}
