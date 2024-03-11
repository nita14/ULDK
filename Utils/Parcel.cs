using ArcGIS.Core.Geometry;
using System;

namespace ULDKClient.Utils
{

    /// <summary>
    /// Parcel class for getting results from ULDK
    /// </summary>
   public class Parcel
    {
        private string _id;
        private string _idLong;
        private string _voivodeship;
        private string _county;
        private string _commune;
        private string _region;
        private DateTime _requestDate;
        private Polygon _geom;


        public Parcel(string id, string idLong, string voivodeship, string county, string commune, string region, DateTime requestDate, Polygon geom)
        {
            _id = id;
            _idLong = idLong;
            _voivodeship = voivodeship;
            _county = county;
            _commune = commune;
            _region = region;
            _requestDate = requestDate;
            _geom = geom;

       
        }


        public string Id { get { return _id; } }
        public string IdLong { get { return _idLong; } }
        public string Voivodeship { get { return _voivodeship; } }
        public string County { get { return _county; } }
        public string Commune { get { return _commune; } }
        public string Region { get { return _region; } }
        public DateTime RequestDate { get { return _requestDate; } }
        public Polygon Geom { get { return _geom; } }
      
  
    }
}
