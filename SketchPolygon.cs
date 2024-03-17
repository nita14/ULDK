using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Serilog;
using System.Threading.Tasks;
using ULDKClient.Utils;

namespace ULDKClient
{
    internal class SketchPolygon : MapTool
    {

        private static readonly ILogger log = Log.ForContext<Helpers>();
        public SketchPolygon()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Polygon;
            SketchOutputMode = SketchOutputMode.Map;
            ContextToolbarID = "";
        }

        protected override Task OnToolActivateAsync(bool active)
        {

            DockPane pane = FrameworkApplication.DockPaneManager.Find("ULDKClient_ULDKDockpane");
            pane.Activate();
            return base.OnToolActivateAsync(active);
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {

            return QueuedTask.Run(async () =>
            {
                Polygon polygon = geometry as Polygon;

                //check the wkid
                if (polygon.SpatialReference.Wkid != Constants.SPATIAL_REF_2180_WKID)
                {
                    polygon = GeometryEngine.Instance.Project(polygon, ULDKDockpaneViewModel._sp2180) as Polygon;
                }

                //check the area
                if (polygon.Area > Constants.POLYGON_MAX_AREA_SQ_METERS)
                {

                    MessageBox.Show(Resource.SKETCH_POLYGON_AREA_OVER_LIMIT);
                    log.Information("Polygon area over the limit.");
                    return true;
                }

                //add point to the map
                bool isPolygoneadded = await Helpers.AddSketchToGraphicLayerAsync(polygon);

                await Helpers.ProcessPolygonFromSketchAsync(polygon);
                return true;

            });
           

        }
    }
}
