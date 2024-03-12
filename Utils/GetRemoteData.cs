﻿using ArcGIS.Desktop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using System.Collections.ObjectModel;
using ArcGIS.Core.Geometry;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ULDKClient.Utils
{
    public class GetRemoteData
    {

 
        /// <summary>
        /// Gets communes dictionary from a remote endpoint
        /// </summary>
        /// <returns></returns>
        public async static Task<ObservableCollection<Commune>> GetCommuneDataAsync()
        {

            ObservableCollection<Commune> communes;
            try { 
            //Get data from the endpoint 
                Log.Information("Preparing Commune request...");
                EsriHttpClient esriHttpClient = new EsriHttpClient();
                esriHttpClient.Timeout = TimeSpan.FromSeconds(10);
      
                EsriHttpResponseMessage responseMessage = esriHttpClient.Get(Constants.COMMUNE_DATA_URL);
                responseMessage.EnsureSuccessStatusCode();

                var resp = await responseMessage.Content.ReadAsStringAsync();
                communes = JsonConvert.DeserializeObject<ObservableCollection<Commune>>(resp);
                return communes;
            } catch (Exception ex)
            {
                string m =  ex.Message;
                Log.Error(m);
                return null;
            }
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
        /// Get TERC extent by the id and type (commune, region)
        /// </summary>
        /// <param name="tercId"></param>
        /// <param name="tercType"></param>
        /// <returns></returns>
        public async static Task<Polygon> GetTercExtentByIDAsync(string tercId, string tercType)
        {

            //Get data from the endpoint 
            Log.Information("Preparing GetTercExtentByIDAsync request...");
            EsriHttpClient esriHttpClient = new EsriHttpClient();
            esriHttpClient.Timeout = TimeSpan.FromSeconds(10);

            string requestURL;

            if (tercType == "Commune")
            {
                requestURL = Constants.FIND_COMMUNE_BY_ID_ULDK_URL + tercId;
            }
            else {
                requestURL = Constants.FIND_REGION_BY_ID_ULDK_URL + tercId;

            }
            Log.Information("Request URL is: " + requestURL);
            EsriHttpResponseMessage responseMessage = esriHttpClient.Get(requestURL);
            responseMessage.EnsureSuccessStatusCode();

            var response = await responseMessage.Content.ReadAsStringAsync();
            var status = response.Split("\n")[0];

            if (status != "0") { 
                Log.Fatal("GetTercExtentByIDAsync error. Status different than 0.");
                return null;
            
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

        public static async Task<Parcel> GetParcelByIdAsync(string parcelIdFull)
        {
            //Get data from the endpoint 

            Log.Information("Preparing GetParcelByIdAsync request...");
            EsriHttpClient esriHttpClient = new EsriHttpClient();
            esriHttpClient.Timeout = TimeSpan.FromSeconds(10);


            Log.Information("Request URL is: " + Constants.FIND_PARCEL_BY_ID_ULDK_URL + parcelIdFull);
            EsriHttpResponseMessage responseMessage = esriHttpClient.Get(Constants.FIND_PARCEL_BY_ID_ULDK_URL + parcelIdFull);
            
            responseMessage.EnsureSuccessStatusCode();

            var response = await responseMessage.Content.ReadAsStringAsync();
            var status = response.Split("\n")[0];

            if (status.Length >= 2 && status.Substring(0,2) == "-1")
            {
                Log.Fatal("GetParcelByIdAsync error. Cannot find the parcel with the id provided.");
                return null;

            }

            Parcel parcel = await GetParcelFromHTTPRequestAsync(response);
            return parcel;
         }

        private static async Task<Parcel> GetParcelFromHTTPRequestAsync(string response)
        {
            Parcel parcel = null;

            string geomCoords = response.Split("\n")[1].Split(";")[1];
            Regex regex = new Regex(@"\(([0-9]+\.[0-9]+ [0-9]+\.[0-9]+\,?)+\)");
            List<Match> rings = regex.Matches(geomCoords).ToList();

            PolygonBuilderEx polygonBuilder = new PolygonBuilderEx(ULDKDockpaneViewModel._sp2180);



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

            //get attributes
            Regex regexattr = new Regex(@"\|.{0,}");
            List<Match> matchesattr = regexattr.Matches(geomCoords).ToList();
            string[] attrs = matchesattr[0].Value.Split("|");

            parcel = new Parcel(attrs[5].Split(".")[2], attrs[6], attrs[1], attrs[2], attrs[3], attrs[4], DateTime.Now, poly);
            return parcel;
        }

        public static async Task<Parcel> GetParcelByPointAsync(MapPoint point)
        {
            //Get data from the endpoint 

            Log.Information("Preparing GetParcelByPointAsync request...");
            EsriHttpClient esriHttpClient = new EsriHttpClient();
            esriHttpClient.Timeout = TimeSpan.FromSeconds(10);

            string requestCoords = point.Coordinate2D.X.ToString() + "," + point.Coordinate2D.Y.ToString() + "," + point.SpatialReference.LatestWkid.ToString();

            Log.Information("Request URL is: " + Constants.FIND_PARCEL_BY_MAP_POINT_ULDK_URL + requestCoords);
            EsriHttpResponseMessage responseMessage = esriHttpClient.Get(Constants.FIND_PARCEL_BY_MAP_POINT_ULDK_URL + requestCoords);

            responseMessage.EnsureSuccessStatusCode();

            var response = await responseMessage.Content.ReadAsStringAsync();
            var status = response.Split("\n")[0];

            if (status.Length >= 2 && status.Substring(0, 2) == "-1")
            {
                Log.Fatal("GetParcelByPointAsync error. Cannot find the parcel with the xy provided.");
                return null;

            }
            Parcel parcel = await GetParcelFromHTTPRequestAsync(response);
            return parcel;

        }
    }
}
