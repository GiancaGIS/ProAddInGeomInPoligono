using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Framework;
using funzioniGeneriche;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace AddInGeomInPoligono
{
    /// <summary>
    /// Interaction logic for DockpaneGeomInPoligonoView.xaml
    /// </summary>
    public partial class DockpaneGeomInPoligonoView : UserControl
    {
        //private static Map mappa = null;
        private static Map mappa = null;
        private static Dictionary<int, Map> dizIdxMappa = new Dictionary<int, Map>();
        private static int idxMappaScelta = 0;

        // Salvo in un dizionario le informazioni sui Layer posizionali
        private static Dictionary<int, FeatureLayer> dizFLayerInTOC = new Dictionary<int, FeatureLayer>();
        private static FeatureLayer _featureLayerPoligonaleScelto = null;


        public DockpaneGeomInPoligonoView()
        {
            InitializeComponent();
            this.AgganciaEventi();
            this.PopolaComboboxMappe();
            Module1.DockpaneGeomInPoligonoView = this; // Ora l'oggetto è istanziabile da fuori
        }

        private void AgganciaEventi()
        {
            DrawCompleteEvent.Subscribe(MappaDrawCompleted);
            LayersAddedEvent.Subscribe(EventoLayerInTOC);
            LayersMovedEvent.Subscribe(EventoLayerInTOC);
            LayersRemovedEvent.Subscribe(EventoLayerInTOC);
            MapClosedEvent.Subscribe(AllaChiusuraMappa);
            MapViewInitializedEvent.Subscribe(OnMapViewCaricata);
            MapPropertyChangedEvent.Subscribe(OnMapPropertiesVariate);
        }

        private void SganciaEventi()
        {
            DrawCompleteEvent.Unsubscribe(MappaDrawCompleted);
            LayersAddedEvent.Unsubscribe(EventoLayerInTOC);
            LayersMovedEvent.Unsubscribe(EventoLayerInTOC);
            LayersRemovedEvent.Unsubscribe(EventoLayerInTOC);
            MapClosedEvent.Unsubscribe(AllaChiusuraMappa);
            MapViewInitializedEvent.Unsubscribe(OnMapViewCaricata);
            MapPropertyChangedEvent.Unsubscribe(OnMapPropertiesVariate);
        }


        #region Metodi per esporre le variabili all'esterno

        /// <summary>
        /// Proprietà della classe che restituisce il Feature Layer Poligonale scelto
        /// </summary>
        public FeatureLayer FeatureLayerPoligonoScelto
        {
            get { return _featureLayerPoligonaleScelto; }
        }

        public Map MappaCorrente
        {
            get { return mappa; }
        }

        #endregion


        private void PopolaComboboxMappe()
        {
            try
            {
                this.comboBox_Mappe.Items.Clear();
                dizIdxMappa.Clear();
                int contatore = 0;

                IReadOnlyList<Map> listaMappe = FunzioniGlobali.RicavaMapsDalMapPanes();

                foreach (Map mappa in listaMappe)
                {
                    if (mappa.MapType == ArcGIS.Core.CIM.MapType.Map)
                    {
                        this.comboBox_Mappe.Items.Add(mappa.Name);
                        dizIdxMappa.Add(contatore, mappa);
                        contatore++;
                    }
                }
                this.comboBox_Mappe.Items.Refresh();
            }
            catch (Exception errore)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(errore.Message,
                    "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Funzioni per eventi

        private async void OnMapPropertiesVariate(MapPropertyChangedEventArgs args)
        {
            //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Proprietà mappa variate!");
            
            // ATTENZIONE! E' NECESSARIO ESEGUIRE IL METODO NEL MEDESIMO THREAD DELLA UI!
            // O SI SCHIANTERA' IL PRO!
            await Utils.RunOnUIThread(() =>
            {
                this.PopolaComboboxMappe();
                this.comboBoxLayerPoligonali.Items.Clear();
            });
        }

        private void OnMapViewCaricata(MapViewEventArgs args)
        {
            this.PopolaComboboxMappe();
        }

        private void MappaDrawCompleted(MapViewEventArgs args)
        {
            //MessageBox.Show("Evento draw completed intercettato!");
            mappa = FunzioniGlobali.RicavaMappaAttiva();
        }

        private void AllaChiusuraMappa(MapClosedEventArgs args)
        {
            this.PopolaComboboxMappe();
        }

        private void EventoLayerInTOC(LayerEventsArgs args)
        {
            //MessageBox.Show("Evento Layer in TOC intercettato!");
            this.PuliscoAggiornoComboboxLayer();
        }

        #endregion


        private void PuliscoAggiornoComboboxLayer()
        {
            FrameworkApplication.Current.Dispatcher.Invoke(() =>
           {
               this.comboBoxLayerPoligonali.Items.Clear();
           });

            if (this.MappaCorrente != null)
            {
                dizFLayerInTOC.Clear();

                var listaLayers = this.MappaCorrente.Layers.OfType<FeatureLayer>().ToList();

                if (listaLayers.Count > 0)
                {
                    int idxProgressivo = 0;

                    foreach (FeatureLayer featureLayer in listaLayers)
                    {
                        var shapeType = featureLayer.ShapeType;
                        if (shapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon)
                        {
                            FrameworkApplication.Current.Dispatcher.Invoke(() =>
                          {
                              this.comboBoxLayerPoligonali.Items.Add(featureLayer.Name);
                          });
                            dizFLayerInTOC.Add(idxProgressivo, featureLayer);
                            idxProgressivo++;
                        }
                    }
                    FrameworkApplication.Current.Dispatcher.Invoke(() =>
                    {
                        this.comboBoxLayerPoligonali.Items.Refresh();
                    });
                }
            }

        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int idx = this.comboBoxLayerPoligonali.SelectedIndex;
                if (idx >= 0)
                _featureLayerPoligonaleScelto = dizFLayerInTOC[idx];
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void comboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                idxMappaScelta = this.comboBox_Mappe.SelectedIndex;

                //if (idxMappaScelta > 0)
                //{
                //    this.SganciaEventi();
                //    this.AgganciaEventi();
                //}

                if (idxMappaScelta >= 0)
                {
                    mappa = dizIdxMappa[idxMappaScelta];
                    this.PuliscoAggiornoComboboxLayer();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Module1.DockpaneGeomInPoligonoViewModel.EliminaListaFeature();
        }
    }
}
