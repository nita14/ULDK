﻿
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;

namespace ULDKClient
{
    internal class SketchPoint : MapTool
    {
        public SketchPoint()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
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

            MapPoint point = geometry as MapPoint;

            if (point.SpatialReference.Wkid != Utils.Constants.SPATIAL_REF_2180_WKID)
            {
                point = GeometryEngine.Instance.Project(point, ULDKDockpaneViewModel._sp2180) as MapPoint;
            }

            ULDKDockpaneViewModel.AddParcelToMapfromSketch(point);
            FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
