using ArcGIS.Desktop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ULDKClient.Utils
{
    class Constansts
    {

        public static void Constants() { 
        
        }

        public static string COMMUNE_DATA_URL = @"https://raw.githubusercontent.com/nita14/ULDK/main/Data/terc.json";
        public static string FIND_COMMUNE_BY_ID_ULDK_URL = @"https://uldk.gugik.gov.pl/?request=GetCommuneById&result=geom_extent&id=";




    }
}
