namespace ULDKClient.Utils
{
    public class Constants
    {

        public static string FC_RESULTS_NAME = @"ULDK";
        public static string GRAPHICS_LAYER_NAME = @"ULDK";
        public static string PROJECT_SUBFOLDER = @"GUGIK";
        public static string GDB_NAME_WITH_EXT = @"GUGIK.gdb";
        public static string COMMUNE_DATA_URL = @"https://raw.githubusercontent.com/nita14/ULDK/master/Config/Data/terc.json";
        public static string REGION_DATA_URL = @"https://raw.githubusercontent.com/nita14/ULDK/master/Config/Data/regions.json";
        public static string FIND_COMMUNE_BY_ID_ULDK_URL = @"https://uldk.gugik.gov.pl/?request=GetCommuneById&result=geom_extent&id=";
        public static string FIND_REGION_BY_ID_ULDK_URL = @"https://uldk.gugik.gov.pl/?request=GetRegionById&result=geom_extent&id=";
        public static string FIND_PARCEL_BY_ID_ULDK_URL = @"https://uldk.gugik.gov.pl/?request=GetParcelById&result=geom_wkt,voivodeship,county,commune,region,id,teryt&id=";
        public static string FIND_PARCEL_BY_MAP_POINT_ULDK_URL = @"https://uldk.gugik.gov.pl/?request=GetParcelByXY&result=geom_wkt,voivodeship,county,commune,region,id,teryt&xy=";
        public static int SPATIAL_REF_2180_WKID = 2180;
        public static int POLYLINE_MAX_LENGTH_METERS = 1000;
        public static int POLYGON_MAX_AREA_SQ_METERS = 1000000;
        public static string GEOPORTAL_LOCATE_PARCEL_URL = @"https://mapy.geoportal.gov.pl/imap/?identifyParcel=";
        public static string KIEG_WMS_URL = @"https://integracja.gugik.gov.pl/cgi-bin/KrajowaIntegracjaEwidencjiGruntow";
        public static string REPO_URL = @"https://github.com/nita14/ULDK/blob/master/Docs/ULDK_instrukcja_obs%C5%82ugi.pdf";




    }
}
