using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;
using System.Windows.Controls;

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
            Polyline polyline = geometry as Polyline;

            //check the wkid
            if (polyline.SpatialReference.Wkid != 2180)
            {
                polyline = GeometryEngine.Instance.Project(polyline, ULDKDockpaneViewModel._sp2180) as Polyline;
            }

            //check the length
            if (polyline.Length > 500) {

                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Resource.SKETCH_LINE_LENGTH_OVER_LIMIT);          
            
            }

            FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
            ULDKDockpaneViewModel.ProcessPolylineFromSketch(polyline);

            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
