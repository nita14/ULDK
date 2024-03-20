using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace ULDKClient
{
    internal class AddKIEGWMSToMap : Button
    {
        protected override void OnClick()
        {

            QueuedTask.Run(() =>
            {

                var _map = MapView.Active.Map;
                //get list of layers of WMS type
                List<WMSLayer> kiegLayers = _map.GetLayersAsFlattenedList().OfType<WMSLayer>().Where(f => f.URL == Utils.Constants.KIEG_WMS_URL).ToList();

                //remove it from the map
                if (kiegLayers.Count > 0)
                {
                    _map.RemoveLayers(kiegLayers);

                }
                else
                {
                    //add it to the map

                    CIMInternetServerConnection serverConnection = new CIMInternetServerConnection { URL = Utils.Constants.KIEG_WMS_URL };
                    CIMWMSServiceConnection connection = new CIMWMSServiceConnection { ServerConnection = serverConnection };
                    LayerCreationParams layerParams = new LayerCreationParams(connection);
                    LayerFactory.Instance.CreateLayer<WMSLayer>(layerParams, _map);

                }

            });

        }
    }
}
