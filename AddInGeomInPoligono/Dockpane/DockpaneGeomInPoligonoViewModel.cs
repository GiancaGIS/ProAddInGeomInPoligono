using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace AddInGeomInPoligono
{
    internal class DockpaneGeomInPoligonoViewModel : DockPane
    {
        private const string _dockPaneID = "AddInGeomInPoligono_DockpaneGeomInPoligono";

        private ObservableCollection<string> _listaDiFeature = new ObservableCollection<string>();

        /// <summary>
        /// used to lock collections for use by multiple threads
        /// </summary>
        public readonly object LockCollections = new object();


        protected DockpaneGeomInPoligonoViewModel()
        {
            // Abilito il Binding tra i dati e la ListView!
            BindingOperations.EnableCollectionSynchronization(_listaDiFeature, LockCollections);
            Module1.DockpaneGeomInPoligonoViewModel = this;
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        #region Metodi per aggiornare i dati dall'esterno della Dockpane

        /// <summary>
        /// Clear the list of features from any thread
        /// </summary>
        internal void EliminaListaFeature()
        {
            lock (_listaDiFeature)
            {
                ProApp.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ListaFeature.Clear();
                }));
            }
        }

        public ObservableCollection<string> ListaFeature
        {
            get { return _listaDiFeature; }
        }

        /// <summary>
        /// Aggiorna la lista di Feature, direttamente sul Thread della GUI
        /// </summary>
        internal void AggiungiListaFeature(string addItem)
        {
            lock (_listaDiFeature)
            {
                ProApp.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ListaFeature.Add(addItem);
                }));
            }
        }
        #endregion 

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Close()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Dockpane Geometrie in Poligono";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class DockpaneGeomInPoligono_ShowButton : Button
    {
        protected override void OnClick()
        {
            DockpaneGeomInPoligonoViewModel.Show();
        }
    }
}
