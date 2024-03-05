using ActiproSoftware.Products.Ribbon;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Internal.Mapping.Georeference;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ULDKClient.Utils;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace ULDKClient
{
    internal class ULDKDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "ULDKClient_ULDKDockpane";
        private bool _isInitialized = false;
        






        protected ULDKDockpaneViewModel() {




        }

        protected override async void OnActivate(bool isActive)
        {
            if (!_isInitialized)
            {

                string ProjectParentFolder = System.IO.Directory.GetParent(Project.Current.URI).FullName + @"\";


                PrepareAndConfigureLogger(ProjectParentFolder);
                

                //Getting Communes
                Log.Information("Getting dictionary data from a remote URL...");
                try
                {
                    _communes = await GetRemoteData.GetCommuneDataAsync();
                    Log.Information("Communes downloaded: {0}.", _communes.Count());

                    _regions = await GetRemoteData.GetRegionDataAsync();
                    Log.Information("Regions downloaded: {0}.", _regions.Count());

                    await CheckOrCreateLocalGDB(ProjectParentFolder);

                    _isInitialized = true;


                }
                catch (Exception ex)
                {
                    //string errormsg = Resources.ResourceManager
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Resource.COMMUNE_ERROR);
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
            System.IO.Directory.CreateDirectory(Path.Join(projectParentFolder, "ULDK"));

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
                .WriteTo.File(Path.Join(projectParentFolder,"ULDK","ULDK_log_" + CurrentDateTime + ".txt")) // log file.
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

            string gdbPath = Path.Combine(projectParentFolder, "ULDK", "ULDK.gdb");

            if (Directory.Exists(gdbPath))
            {
                Log.Information("File GDB already exists.");

                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    new Geodatabase(new FileGeodatabaseConnectionPath(new System.Uri(gdbPath)));
                });
                

            }
            else {

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
                catch (Exception aex){
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
            else {
                Log.Information("The results feature class needs to be created.");
                bool fcCreated = await Utils.Helpers.CreateResultsFeatureClassAsync(gdbPath, Utils.Constants.FC_RESULTS_NAME);

                if (fcCreated)
                {
                    Log.Information("The results feature class has been created.");
                }
                else {
                    Log.Fatal("The results feature class has NOT been created.");

                }

            }
            Log.Information("Initilizing UI...");
        }



        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "My DockPane";
        public string Heading
        {
            get => _heading;
            set => SetProperty(ref _heading, value);
        }
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

            }
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
                if (value != null) {
                    ZoomToCommuneExtent(value.Id);
                    //Update list of regions based on commune id

                    List<Region> filtered = Regions.Where(r => r.CommuneId.Equals(value.Id)).ToList();
                    filtered.Count();

                }
            }
        }


      

        private static async void ZoomToCommuneExtent(string id)
        {

            await QueuedTask.Run(async () =>
            {

                Polygon poly = await GetRemoteData.GetCommuneExtentByIDAsync(id);
                //Get the active map view.
                var mapView = MapView.Active;
                if (mapView == null || poly ==null)
                    return;

                await mapView.ZoomToAsync(poly, TimeSpan.FromSeconds(1));
            });
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
