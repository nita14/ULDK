using ActiproSoftware.Products.Ribbon;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
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
                await PrepareAndConfigureLogger();

                //Getting Communes
                Log.Information("Getting Commune data from a remote URL...");
                try
                {
                    _communes = GetRemoteData.GetCommuneDataAsync().Result;
                    Log.Information("Communes downloaded: {0}.", _communes.Count());
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
        private async static Task PrepareAndConfigureLogger()
        {


            string CurrentDateTime = DateTime.Now.ToString("yyyy''MM''dd'T'HH'-'mm'-'ss");
            var detector = new Utils.LogErrorDetector();
            string folder = System.IO.Directory.GetParent(Project.Current.URI).FullName + @"\";

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
                .WriteTo.File(folder + "ULDK_" + CurrentDateTime + ".txt")  // log file.
                .WriteTo.Sink(detector)
                .CreateLogger();

            Log.Information("ULDK plugin v.1.0.");
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
