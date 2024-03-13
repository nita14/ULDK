using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            Polygon polygon = geometry as Polygon;

            //check the wkid
            if (polygon.SpatialReference.Wkid != 2180)
            {
                polygon = GeometryEngine.Instance.Project(polygon, ULDKDockpaneViewModel._sp2180) as Polygon;
            }

            //check the area
            if (polygon.Area > 1000)
            {

                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Resource.SKETCH_POLYGON_AREA_OVER_LIMIT);

            }

            FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
            ULDKDockpaneViewModel.ProcessPolygonFromSketch(polygon);
            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
