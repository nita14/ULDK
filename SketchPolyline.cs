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
    internal class SketchPolyline : MapTool
    {
        public SketchPolyline()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Line;
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
                Polyline polyline = geometry as Polyline;

                //check the wkid
                if (polyline.SpatialReference.Wkid != Constants.SPATIAL_REF_2180_WKID)
                {
                    polyline = GeometryEngine.Instance.Project(polyline, ULDKDockpaneViewModel._sp2180) as Polyline;
                }

                //check the length
                if (polyline.Length > 500)
                {

                    MessageBox.Show(Resource.SKETCH_LINE_LENGTH_OVER_LIMIT);
                }

                await Helpers.ProcessPolylineFromSketchAsync(polyline);

                return true;


            });
                
        }
    }
}
