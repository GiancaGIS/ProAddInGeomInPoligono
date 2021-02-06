using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace funzioniGeneriche
{
    class FunzioniGlobali
    {
        public static async Task<Map> TrovaMapAperteEsistentiAsync(string mapName)
        {
            return await QueuedTask.Run(async () =>
            {
                Map map = null;
                Project proj = Project.Current;

                //Finding the first project item with name matches with mapName
                MapProjectItem mpi =
                    proj.GetItems<MapProjectItem>()
                        .FirstOrDefault(m => m.Name.Equals(mapName, StringComparison.CurrentCultureIgnoreCase));
                if (mpi != null)
                {
                    map = mpi.GetMap();
                    //Opening the map in a mapview
                    await ProApp.Panes.CreateMapPaneAsync(map);
                }
                return map;
            });
        }

        /// <summary>
        /// Funzione che restituisce la Mappa attiva nel progetto corrente di ArcGIS Pro
        /// </summary>
        /// <returns></returns>
        public static Map RicavaMappaAttiva()
        {
            var mapView = MapView.Active;
            if (mapView == null)
            {
                return null;
            }
            else
            {
                return mapView.Map;
            }
        }

        public static async Task<SpatialReference> RicavaSRLayer(Layer layer)
        {
            return await QueuedTask.Run(() =>
            {
                SpatialReference spatialReference = layer.GetSpatialReference();
                return spatialReference;
            });
        }

        public static Task<CIMMap> RicavaInfoMappaCIMMapClass(Map mappa)
        {
            return QueuedTask.Run(() =>
            {
                return mappa.GetDefinition();
            });
        }

        public async Task ProgressorFermabileAsync(int numSecondsDelay)
        {
            ArcGIS.Desktop.Framework.Threading.Tasks.CancelableProgressorSource cps =
              new ArcGIS.Desktop.Framework.Threading.Tasks.CancelableProgressorSource("Doing my thing - cancelable", "Canceled");

            //simulate doing some work which can be canceled
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                cps.Progressor.Max = (uint)numSecondsDelay;
                //check every second
                while (!cps.Progressor.CancellationToken.IsCancellationRequested)
                {
                    cps.Progressor.Value += 1;
                    cps.Progressor.Status = "Status " + cps.Progressor.Value;
                    cps.Progressor.Message = "Message " + cps.Progressor.Value;

                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("RunCancelableProgress Loop{0}", cps.Progressor.Value));
                    }
                    //are we done?
                    if (cps.Progressor.Value == cps.Progressor.Max) break;
                    //block the CIM for a second
                    Task.Delay(1000).Wait();

                }
                System.Diagnostics.Debug.WriteLine(string.Format("RunCancelableProgress: Canceled {0}",
                                                    cps.Progressor.CancellationToken.IsCancellationRequested));

            }, cps.Progressor);
        }

        public static IReadOnlyList<Map> RicavaMapsDalMapPanes()
        {

            //Gets the unique list of Maps from all the MapPanes.
            //Note: The list of maps retrieved from the MapPanes
            //maybe less than the total number of Maps in the project.
            //It depends on what maps the user has actually opened.
            var mapPanes = ProApp.Panes.OfType<IMapPane>()
                        .GroupBy((mp) => mp.MapView.Map.URI).Select(grp => grp.FirstOrDefault());

            List<Map> uniqueMaps = new List<Map>();

            foreach (var pane in mapPanes)
                uniqueMaps.Add(pane.MapView.Map);

            return uniqueMaps;
        }

        public static async Task<IReadOnlyList<Map>> RicavaMapsDalMapPanesAsync()
        {
            return await QueuedTask.Run(() =>
            {
                //Gets the unique list of Maps from all the MapPanes.
                //Note: The list of maps retrieved from the MapPanes
                //maybe less than the total number of Maps in the project.
                //It depends on what maps the user has actually opened.
                var mapPanes = ProApp.Panes.OfType<IMapPane>()
                            .GroupBy((mp) => mp.MapView.Map.URI).Select(grp => grp.FirstOrDefault());

                List<Map> uniqueMaps = new List<Map>();

                foreach (var pane in mapPanes)
                    uniqueMaps.Add(pane.MapView.Map);

                return uniqueMaps;
            });
        }

        /// <summary>
        /// Scopo di questo metodo consiste nel restituire una ObservableCollection di Stringhe, ogni stringa contiene info sugli oggetti che ricadono nella geometria scelta
        /// in un MapView scelto in input.
        /// </summary>
        /// <param name="mappa"></param>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static async Task<ObservableCollection<string>> RestituisciListaInfoFeatureAsync(MapView mappa, Geometry geometry)
        {
            // Inizializzo variabile con lista Feature:
            ObservableCollection<string> ListaInfoSuFeature = new ObservableCollection<string>();

            return await QueuedTask.Run(() =>
            {
                // Get the features that intersect the sketch geometry. 
                // getFeatures returns a dictionary of featurelayer and a list of Object ids for each
                Dictionary<BasicFeatureLayer, List<long>> featuresObjectIds = mappa.GetFeatures(geometry);

                // go through all feature layers and do a spatial query to find features 
                foreach (var coppiaValoriLayerListaOID in featuresObjectIds)
                {
                    var featLyr = coppiaValoriLayerListaOID.Key;
                    var qf = new QueryFilter() { ObjectIDs = coppiaValoriLayerListaOID.Value };

                    var rowCursor = featLyr.Search(qf);

                    while (rowCursor.MoveNext())
                    {
                        using (var feat = rowCursor.Current as Feature)
                        {
                            List<long> listOID = new List<long> { feat.GetObjectID() };
                            string displayExp = String.Join(Environment.NewLine, featLyr.QueryDisplayExpressions(listOID.ToArray()));

                            // Aggiungo le info alla lista di stringhe:
                            ListaInfoSuFeature.Add($@"Layer: {featLyr.Name}, OID: {feat.GetObjectID()}, Display Expression: {displayExp}");
                            
                            //Access all field values
                            //var count = feat.GetFields().Count();
                            //for (int i = 0; i < count; i++)
                            //{
                            //    var val = feat[i];
                            //    //TODO use the value(s)
                            //}
                        }
                    }
                }
                return ListaInfoSuFeature;
            });
        }
    }
}