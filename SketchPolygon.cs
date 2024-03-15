using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;
using ULDKClient.Utils;

namespace ULDKClient
{
    internal class SketchPolygon : MapTool
    {
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
                if (polygon.Area > 10000)
                {

                    MessageBox.Show(Resource.SKETCH_POLYGON_AREA_OVER_LIMIT);

                }

                await Helpers.ProcessPolygonFromSketchAsync(polygon);
                return true;

            });
           

        }
    }
}
