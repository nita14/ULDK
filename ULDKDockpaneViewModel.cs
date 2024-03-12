using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Editing;
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
       





        protected ULDKDockpaneViewModel()
        {




        }

        protected override async void OnActivate(bool isActive)
        {
            if (!_isInitialized)
            {

                _projectParentFolder = System.IO.Directory.GetParent(Project.Current.URI).FullName + @"\";
                

                PrepareAndConfigureLogger(_projectParentFolder);

                _isInitialized = true;

                //Getting Communes
                Log.Information("Getting dictionary data from a remote URL...");
                try
                {

                    _communes = await GetRemoteData.GetCommuneDataAsync();
                    Log.Information("Communes downloaded: {0}.", _communes.Count());

                    _regions = await GetRemoteData.GetRegionDataAsync();
                    Log.Information("Regions downloaded: {0}.", _regions.Count());

                    await CheckOrCreateLocalGDB(_projectParentFolder);


                    await QueuedTask.Run(async () =>
                    {
                        _currentMap = MapView.Active.Map;
                        _graphicsLayer = Helpers.GetGraphicsLayer(_currentMap);
                        _mapView = MapView.Active;
                        _sp2180 = new SpatialReferenceBuilder(Constants.SPATIAL_REF_2180_WKID).ToSpatialReference();
                    });



                }
                catch (Exception ex)
                {
                    //string errormsg = Resources.ResourceManager
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Resource.COMMUNE_REGION_ERROR);
                    Log.Fatal(ex.InnerException.Message);
                    Log.Fatal(ex.StackTrace.ToString());
                };


            }

        }

        protected override async void OnShow(bool isVisible)
        {



        }


        protected async override Task UninitializeAsync()
        {
            // Reset Default Save Directory when dockpane closes // 
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);

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
            System.IO.Directory.CreateDirectory(Path.Join(projectParentFolder, "GUGIK"));

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
                .WriteTo.File(Path.Join(projectParentFolder, "GUGIK", "ULDK_log_" + CurrentDateTime + ".txt")) // log file.
                .WriteTo.Sink(detector)
                .CreateLogger();

            Log.Information("ULDK plugin v.1.0.");
            Log.Information("Project URL is: " + projectParentFolder);


        }

        /// <summary>
        /// Checks if the file gdb exists, if not creates it with feature class
        /// </summary>
        /// <param name="projectParentFolder"></param>
        /// <returns></returns>
        private async static Task CheckOrCreateLocalGDB(string projectParentFolder)

        {

            string gdbPath = Path.Combine(projectParentFolder, "GUGIK", "GUGIK.gdb");

            if (Directory.Exists(gdbPath))
            {
                Log.Information("File GDB already exists.");

                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    new Geodatabase(new FileGeodatabaseConnectionPath(new System.Uri(gdbPath)));
                });


            }
            else
            {

                Log.Information("File GDB does not exists. Creating...");


                FileGeodatabaseConnectionPath fileGdbConnectionPath =
                    new FileGeodatabaseConnectionPath(new System.Uri(gdbPath));

                try
                {

                    using (Geodatabase geodatabase =
                        SchemaBuilder.CreateGeodatabase(fileGdbConnectionPath))

                    {
                    }

                }
                catch (GeodatabaseWorkspaceException ex)
                {
                    Log.Information("File gdb already exists. Path: " + fileGdbConnectionPath.ToString());
                }
                catch (GeodatabaseException gex)
                {
                    Log.Fatal("A geodatabase-related exception has occurred.");
                    Log.Fatal(gex.Message.ToLower());
                }
                catch (Exception aex)
                {
                    Log.Fatal("Another fatal error occurred.");
                    Log.Fatal(aex.GetType().FullName);
                    Log.Fatal(aex.Message.ToLower());
                }

                Log.Information("File GDB created. Path: " + fileGdbConnectionPath.Path.AbsolutePath);

            }

            //check or create Results feature class
            Log.Information("Checking if the results feature class already exists...");
            bool fcExists = await Utils.Helpers.FeatureClassExistsAsync(gdbPath, Utils.Constants.FC_RESULTS_NAME);

            if (fcExists)
            {
                Log.Information("The results feature class already exists.");
            }
            else
            {
                Log.Information("The results feature class needs to be created.");
                bool fcCreated = await Utils.Helpers.CreateResultsFeatureClassAsync(gdbPath, Utils.Constants.FC_RESULTS_NAME);

                if (fcCreated)
                {
                    Log.Information("The results feature class has been created.");
                }
                else
                {
                    Log.Fatal("The results feature class has NOT been created.");

                }

            }
            Log.Information("Initilizing UI...");
        }



        private string _parcelIdFull = "";
        public string ParcelIdFull
        {
            get => _parcelIdFull;
            set => SetProperty(ref _parcelIdFull, value);
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

        private ObservableCollection<Region> _regions = new ObservableCollection<Region>();
        public ObservableCollection<Region> Regions
        {

            get => _regions;
            set => SetProperty(ref _regions, value, () => Regions);


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
                    } else if (value != null && ParcelIdFull.Length > 0) {
                        ParcelIdFull += "." + value.Number;
                    }

                    ZoomToTercExtent(value.CommuneId + "." + value.Number, "Region");


                }
            }
        }

        //isEbaled

        private bool _showGeoportalBtnEnabled = false;
        public bool ShowGeoportalBtnEnabled {
            
            get => _showGeoportalBtnEnabled;
            set => SetProperty(ref _showGeoportalBtnEnabled, value, () => ShowGeoportalBtnEnabled);


        }


        //Commune dropdwon handlers
        private ObservableCollection<Commune> _communes = new ObservableCollection<Commune>();
        public ObservableCollection<Commune> Communes
        {

            get => _communes;
            set => SetProperty(ref _communes, value, () => Communes);


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
                    ZoomToTercExtent(value.Id, "Commune");
                    //Update list of regions based on commune id

                    CollectionView regionsOriginalView = (CollectionView)CollectionViewSource.GetDefaultView(Regions);
                    regionsOriginalView.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(Region.Number), ListSortDirection.Ascending));

                    regionsOriginalView.Filter = r =>
                    {

                        Region vRegion = r as Region;
                        return (vRegion != null && vRegion.CommuneId == value.Id);

                    };
                    int cnt = regionsOriginalView.Count;


                }
            }
        }


        private static async void ZoomToTercExtent(string id, string TercType)
        {

            await QueuedTask.Run(async () =>
            {

                Polygon poly = await GetRemoteData.GetTercExtentByIDAsync(id, TercType);
                //Get the active map view.
                var mapView = MapView.Active;
                if (mapView == null || poly == null)
                    return;

                await mapView.ZoomToAsync(poly, TimeSpan.FromSeconds(1));
            });
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
                        Parcel parcel = await GetRemoteData.GetParcelByIdAsync(ParcelIdFull);

                        //parcel with the specified id does not exist
                        if (parcel == null)
                        {
                            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Resource.PARCEL_ID_NOT_EXIST_ERROR);
                            Log.Information("Cannot find parcel with the id: " + ParcelIdFull);
                            return;
                        }

                        Log.Information("Found a parcel with the id: " + ParcelIdFull);

                        //Enable show in geoportal button
                        ShowGeoportalBtnEnabled = true;
                        
                        _mapView.ZoomToAsync(parcel.Geom, TimeSpan.FromSeconds(1));

                        //add parcel to graphics layer and feature class

                        AddParcelToGraphicsAndFeatureClass(parcel);

                    }
                    else
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Resource.COMMUNE_REGION_ERROR);
                        

                    }
                });
            }
        }

        public static async Task<bool> AddParcelToGraphicsAndFeatureClass(Parcel parcel)
        {
       
            try 
            {

                //process the geometry
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

                if (isGraphicadded && isFeatureAdded)

                { return true;
                } else { return false; }

            }
            catch (Exception ex) {
                Log.Fatal("AddParcelToGraphicsAndFeatureClass: something went wrong");
                Log.Fatal(ex.Message.ToString());
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Resource.PARCEL_CANNOT_SAVE_GL_FC_ERROR);
                return false;
            }



        }

        public ICommand CmdShowParcelInGeoportal
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://mapy.geoportal.gov.pl/imap/?identifyParcel=" + ParcelIdFull,
                        UseShellExecute = true
                    });
                    

                 
                });
            }
        }

        public ICommand CmdStartSketchPoint
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    ArcGIS.Desktop.Framework.FrameworkApplication.SetCurrentToolAsync("ULDKClient_SketchPoint");
                });
            }
        }

        public ICommand CmdStartSketchPolyline
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    ArcGIS.Desktop.Framework.FrameworkApplication.SetCurrentToolAsync("ULDKClient_SketchPolyline");



                });
            }
        }

        public ICommand CmdStartSketchPolygon
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://mapy.geoportal.gov.pl/imap/?identifyParcel=" + ParcelIdFull,
                        UseShellExecute = true
                    });



                });
            }
        }

        public static async void AddParcelToMapfromSketch(MapPoint point) {

            Parcel parcel = await GetRemoteData.GetParcelByPointAsync(point);

            await AddParcelToGraphicsAndFeatureClass(parcel);

            _mapView.ZoomToAsync(parcel.Geom, TimeSpan.FromSeconds(1));

        }

        public static async void ProcessPolylineFromSketch(Polyline polyline)
        {
            // 1. Densify the line

            Polyline polyDensified = GeometryEngine.Instance.DensifyByLength(polyline, 1) as Polyline;
                
            // 2. Get first vertex and fetch parcel and iterate
            while (polyDensified.Points.Count > 0)
            {
                MapPoint mp = polyDensified.Points[0];
                Parcel parcel = await GetRemoteData.GetParcelByPointAsync(mp);
                // 3. Add pacel to map, gl and fc
                await AddParcelToGraphicsAndFeatureClass(parcel);
                // 4. Bufer the parcel geom by 30cm
                Polygon parcelGeomBuffer = GeometryEngine.Instance.Buffer(parcel.Geom, 0.50) as Polygon; 
                // 5. Perform difference densyfied line and parcel geom
                Polyline polyDensifiedNew = GeometryEngine.Instance.Difference(polyDensified, parcelGeomBuffer) as Polyline;
                polyDensified = polyDensifiedNew;
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
