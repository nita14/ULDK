using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Serilog;
using System.Threading.Tasks;
using System.Windows;
using ULDKClient.Utils;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace ULDKClient
{
    internal class SketchPolyline : MapTool
    {

        private static readonly ILogger log = Log.ForContext<Helpers>();

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
                DockPane pane = FrameworkApplication.DockPaneManager.Find("ULDKClient_ULDKDockpane");
                (pane as ULDKDockpaneViewModel).BusyVisibility = Visibility.Visible;

                Polyline polyline = geometry as Polyline;

                //check the wkid
                if (polyline.SpatialReference.Wkid != Constants.SPATIAL_REF_2180_WKID)
                {
                    polyline = GeometryEngine.Instance.Project(polyline, ULDKDockpaneViewModel._sp2180) as Polyline;
                }

                //check the length
                if (polyline.Length > Constants.POLYLINE_MAX_LENGTH_METERS)
                {

                    MessageBox.Show(Properties.Resources.SKETCH_LINE_LENGTH_OVER_LIMIT);
                    log.Information("Line length over the limit.");
                    return true;
                }

                //add point to the map
                bool isPolylineadded = await Helpers.AddSketchToGraphicLayerAsync(polyline);

                await Helpers.ProcessPolylineFromSketchAsync(polyline);
                (pane as ULDKDockpaneViewModel).BusyVisibility = Visibility.Collapsed;

                return true;


            });
                
        }
    }
}
