using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows;
using ULDKClient.Utils;

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
			return QueuedTask.Run(async () =>
			{

                DockPane pane = FrameworkApplication.DockPaneManager.Find("ULDKClient_ULDKDockpane");
				(pane as ULDKDockpaneViewModel).BusyVisibility = Visibility.Visible;

				
                //ULDKDockpaneViewModel._busyVisibility = Visibility.Visible;
				MapPoint point = geometry as MapPoint;


				//check the spatial reference
				if (point.SpatialReference.Wkid != Constants.SPATIAL_REF_2180_WKID)
				{
					point = GeometryEngine.Instance.Project(point, ULDKDockpaneViewModel._sp2180) as MapPoint;
				}
				//add point to the map
				bool isPointadded = await Helpers.AddSketchToGraphicLayerAsync(point);


				//get parcel
				Parcel parcel = await GetRemoteData.GetInstance().GetParcelByPointAsync(point);
				//add parcel to graphics layer
				bool isGraphicadded = await Helpers.AddGeometrytoGraphicLayerAsync(ULDKDockpaneViewModel._graphicsLayer, parcel.Geom, parcel.Id);
				if (isGraphicadded)
				{
					Log.Information("Parcel added to graphics layer.");

				}
				else
				{

					Log.Information("Cannot add a parcel to graphics layer.");
				}
				//add parcel to feature class
				bool isFeatureAdded = await Helpers.AddParceltoFeatureClassAsync(parcel, ULDKDockpaneViewModel._projectParentFolder);
				if (isFeatureAdded)
				{
					Log.Information("Parcel added to feature class.");

				}
				else
				{

					Log.Information("Cannot add a parcel to feature class.");
				}

				await ULDKDockpaneViewModel._mapView.ZoomToAsync(parcel.Geom, TimeSpan.FromSeconds(1));
                (pane as ULDKDockpaneViewModel).BusyVisibility = Visibility.Collapsed;
                return true;


			});

		}
	}
}
