using ArcGIS.Desktop.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ULDKClient.Utils
{
    class Constants
    {

        public static string FC_RESULTS_NAME = @"ULDK";
        public static string COMMUNE_DATA_URL = @"https://raw.githubusercontent.com/nita14/ULDK/master/Config/Data/terc.json";
        public static string REGION_DATA_URL = @"https://raw.githubusercontent.com/nita14/ULDK/master/Config/Data/regions.json";
        public static string FIND_COMMUNE_BY_ID_ULDK_URL = @"https://uldk.gugik.gov.pl/?request=GetCommuneById&result=geom_extent&id=";
        public static string FIND_REGION_BY_ID_ULDK_URL = @"https://uldk.gugik.gov.pl/?request=GetRegionById&result=geom_extent&id=";
        public static string FIND_PARCEL_BY_ID_ULDK_URL = @"https://uldk.gugik.gov.pl/?request=GetParcelByIdOrNr&result=geom_wkt,voivodeship,county,commune,region&id=";




    }
}
