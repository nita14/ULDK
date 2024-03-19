using ActiproSoftware.Windows.Extensions;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using ULDKClient.Utils;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace ULDKClient
{
	public class ULDKDockpaneViewModel : DockPane
	{
		private const string _dockPaneID = "ULDKClient_ULDKDockpane";
		private bool _isInitialized = false;
		public static GraphicsLayer _graphicsLayer = null;
		public static Map _currentMap = null;
		public static string _projectParentFolder = null;
		public static MapView _mapView = null;
		public static SpatialReference _sp2180 = null;
		public static GetRemoteData _grdInstance = null;

		protected ULDKDockpaneViewModel()
		{
			BindingOperations.EnableCollectionSynchronization(
				this.Regions, Module1._Lock);

			BindingOperations.EnableCollectionSynchronization(
				this.Communes, Module1._Lock);
		}

		protected override async void OnActivate(bool isActive)
		{
			if (!_isInitialized)
			{

				_projectParentFolder = Directory.GetParent(Project.Current.URI).FullName + @"\";
				//Getting the logger
				PrepareAndConfigureLogger(_projectParentFolder);

				//Getting dictionaries
				Log.Information("Getting dictionary data from a remote URL and setting the local db...");
				try
				{
					//getting communes - use a local list
					var communes = await GetRemoteData.GetInstance().GetCommuneDataAsync();
					if (communes == null)
					{
						Log.Error("Error encountered while getting communes.");
					}
					else
					{
						Log.Information("Communes downloaded: {0}.", communes.Count());
						if (communes.Count() > 0)
						{
							//now populate the observable collection with a lock
							lock (Module1._Lock)
							{
								_communes.Clear();
								_communes.AddRange(communes);
							}
						}
					}

					//getting region - use a local list
					var regions = await GetRemoteData.GetInstance().GetRegionDataAsync();
					if (regions == null)
					{
						Log.Error("Error encountered while getting regions.");
					}
					else
					{
						Log.Information("Communes downloaded: {0}.", regions.Count());
						if (regions.Count() > 0)
						{
							//now populate the observable collection with a lock
							lock (Module1._Lock)
							{
								_regions.Clear();
								_regions.AddRange(regions);
							}
						}
					}

					//initialize constants
					await QueuedTask.Run(() =>
					{
						//local gdb and fc setup
						Helpers.CheckOrCreateLocalGDB(_projectParentFolder);
						Log.Information("Local GDB and fc created.");

						_currentMap = MapView.Active.Map;
						_graphicsLayer = Helpers.GetGraphicsLayer(_currentMap);
						_mapView = MapView.Active;
						_sp2180 = new SpatialReferenceBuilder(Constants.SPATIAL_REF_2180_WKID).ToSpatialReference();
					});

					_isInitialized = true;


				}
				catch (Exception ex)
				{
					MessageBox.Show(Resource.COMMUNE_REGION_ERROR);
					Log.Fatal(ex.InnerException.Message);
					Log.Fatal(ex.StackTrace.ToString());
					return;
				};



			}

		}

		protected async override Task UninitializeAsync()
		{
			await base.UninitializeAsync();
		}

		/// <summary>
		/// Override the default behavior when the dockpane's help icon is clicked
		/// or the F1 key is pressed.
		/// </summary>
		protected override void OnHelpRequested()
		{
			var startInfo = new System.Diagnostics.ProcessStartInfo(@"https://github.com/nita14/ULDK#readme");
			startInfo.UseShellExecute = true;
			System.Diagnostics.Process.Start(startInfo);
		}



		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal async static void Show()
		{
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null)
				return;

			pane.Activate();

		}



		/// <summary>
		/// Creates a logger and saves a log to the current project's folder
		/// </summary>
		/// <returns></returns>
		private static void PrepareAndConfigureLogger(string projectParentFolder)
		{


			string CurrentDateTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss");
			var detector = new Utils.LogErrorDetector();

			//check or create an ULDK directory
			System.IO.Directory.CreateDirectory(Path.Join(projectParentFolder, Constants.PROJECT_SUBFOLDER));

			Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
					.WriteTo.File(Path.Join(projectParentFolder, Constants.PROJECT_SUBFOLDER, "ULDK_log_" + CurrentDateTime + ".txt")) // log file.
					.WriteTo.Sink(detector)
					.CreateLogger();

			Log.Information("ULDK plugin v.1.0.");
			Log.Information("Project URL is: " + projectParentFolder);


		}

		private string _parcelIdFull = "";
		public string ParcelIdFull
		{
			get => _parcelIdFull;
			set => SetProperty(ref _parcelIdFull, value);
		}

		private string _statusProcessingSketch = Constants.STATUS_PROCES_FREE;
		public string StatusProcessingSketch
		{
			get => _statusProcessingSketch;
			set => SetProperty(ref _statusProcessingSketch, value);
		}



		private string _parcelId = "";
		public string ParcelId
		{
			get => _parcelId;
			set

			{

				SetProperty(ref _parcelId, value);
				ShowGeoportalBtnEnabled = false;

				if (value != null && value.Length > 0 && ParcelIdFull.Split(".").Length == 2)
				{
					ParcelIdFull += "." + value;
				}
				else if (value != null && value.Length > 0 && ParcelIdFull.Split(".").Length > 2)
				{
					ParcelIdFull = ParcelIdFull.Split(".")[0] + "." + ParcelIdFull.Split(".")[1] + "." + value;

				}


			}
		}
		/// <summary>
		/// Sketch reference
		/// </summary>
		/// 

		//Region dropdwon handlers

		//Once it is bound do not re-set the pointer or u break the
		//binding
		private ObservableCollection<Region> _regions = new ObservableCollection<Region>();
		public ObservableCollection<Region> Regions
		{

			get => _regions;
			//set => SetProperty(ref _regions, value, () => Regions);
		}


		private Region _selectedRegion;
		public Region SelectedRegion
		{
			get => _selectedRegion;
			set
			{

				SetProperty(ref _selectedRegion, value, () => SelectedRegion);
				ShowGeoportalBtnEnabled = false;
				if (value != null)
				{

					ParcelId = "";

					if (value != null && ParcelIdFull.Split(".").Length >= 2)
					{
						ParcelIdFull = ParcelIdFull.Split(".")[0] + "." + value.Number;
					}
					else if (value != null && ParcelIdFull.Length > 0)
					{
						ParcelIdFull += "." + value.Number;
					}

					ZoomToTercExtentAsync(value.CommuneId + "." + value.Number, "Region");
				}
			}
		}

		//isEnabled

		private bool _showGeoportalBtnEnabled = false;
		public bool ShowGeoportalBtnEnabled
		{

			get => _showGeoportalBtnEnabled;
			set => SetProperty(ref _showGeoportalBtnEnabled, value, () => ShowGeoportalBtnEnabled);


		}


		//Commune dropdwon handlers
		private ObservableCollection<Commune> _communes = new ObservableCollection<Commune>();
		public ObservableCollection<Commune> Communes
		{

			get => _communes;
			//set => SetProperty(ref _communes, value, () => Communes);
		}

		private Commune _selectedCommune;
		public Commune SelectedCommune
		{
			get => _selectedCommune;
			set
			{

				SetProperty(ref _selectedCommune, value, () => SelectedCommune);
				ShowGeoportalBtnEnabled = false;
				if (value != null)
				{

					ParcelIdFull = value.Id;
					ParcelId = "";
					ZoomToTercExtentAsync(ParcelIdFull, "Commune");
					//Update list of regions based on commune id

					CollectionView regionsOriginalView = (CollectionView)CollectionViewSource.GetDefaultView(Regions);
					regionsOriginalView.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(Region.Number), ListSortDirection.Ascending));

					regionsOriginalView.Filter = r =>
					{

						Region vRegion = r as Region;
						return (vRegion != null && vRegion.CommuneId == ParcelIdFull);

					};
					int cnt = regionsOriginalView.Count;


				}
			}
		}


		private static async Task ZoomToTercExtentAsync(string id, string TercType)
		{
			Polygon poly = await GetRemoteData.GetInstance().GetTercExtentByIDAsync(id, TercType);
			//Get the active map view.
			if (_mapView == null || poly == null)
				return;

			await _mapView.ZoomToAsync(poly, TimeSpan.FromSeconds(1));
		}

		//button click handlers

		public ICommand CmdShowParcelOnTheMap
		{
			get
			{
				return new RelayCommand(async () =>
				{

					if (ParcelIdFull != null && ParcelIdFull != "" && ParcelIdFull.Split(".").Length == 3)
					{
						//execute command
						Parcel parcel = await GetRemoteData.GetInstance().GetParcelByIdAsync(ParcelIdFull);

						//parcel with the specified id does not exist
						if (parcel == null)
						{
							MessageBox.Show(Resource.PARCEL_ID_NOT_EXIST_ERROR);
							Log.Information("Cannot find parcel with the id: " + ParcelIdFull);
							return;
						}

						Log.Information("Found a parcel with the id: " + ParcelIdFull);

						//Enable show in geoportal button
						ShowGeoportalBtnEnabled = true;

						await _mapView.ZoomToAsync(parcel.Geom, TimeSpan.FromSeconds(1));

						//add parcel to graphics layer and feature class

						bool isGraphicadded = await Helpers.AddGeometrytoGraphicLayerAsync(_graphicsLayer, parcel.Geom, parcel.Id);
						if (isGraphicadded)
						{
							Log.Information("Parcel added to graphics layer.");

						}
						else
						{

							Log.Information("Cannot add a parcel to graphics layer.");
						}

						bool isFeatureAdded = await Helpers.AddParceltoFeatureClassAsync(parcel, _projectParentFolder);
						if (isFeatureAdded)
						{
							Log.Information("Parcel added to feature class.");

						}
						else
						{

							Log.Information("Cannot add a parcel to feature class.");
						}


					}
					else
					{
						MessageBox.Show(Resource.ADD_PARCEL_FAILED);


					}
				});
			}
		}

		public ICommand CmdShowParcelInGeoportal
		{
			get
			{
				return new RelayCommand(() =>
				{
					System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
					{
						FileName = Constants.GEOPORTAL_LOCATE_PARCEL_URL + ParcelIdFull,
						UseShellExecute = true
					});



				});
			}
		}

		public ICommand CmdStartSketchPoint
		{
			get
			{
				return new RelayCommand(() =>
				{
					FrameworkApplication.SetCurrentToolAsync("ULDKClient_SketchPoint");
				});
			}
		}

		public ICommand CmdStartSketchPolyline
		{
			get
			{
				return new RelayCommand(() =>
				{
					FrameworkApplication.SetCurrentToolAsync("ULDKClient_SketchPolyline");

				});
			}
		}

		public ICommand CmdStartSketchPolygon
		{
			get
			{
				return new RelayCommand(() =>
				{
					FrameworkApplication.SetCurrentToolAsync("ULDKClient_SketchPolygon");

				});
			}
		}

	}




	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class ULDKDockpane_ShowButton : ArcGIS.Desktop.Framework.Contracts.Button
	{
		protected override void OnClick()
		{
			ULDKDockpaneViewModel.Show();
		}
	}
}
