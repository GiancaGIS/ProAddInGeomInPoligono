using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;


namespace AddInGeomInPoligono
{
    internal class ToolSelezPoligono : MapTool
    {
        public ToolSelezPoligono()
        {
            IsSketchTool = true; // If IsSketchTool is not set to true, a sketch is not produced.
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            try
            {
                //var pane = FrameworkApplication.DockPaneManager.Find("AddInGeomInPoligono_DockpaneGeomInPoligono");
                //pane.Activate();

                // Come con gli ArcObjects, ricavo info sulle coordinate del punto cliccato
                Envelope envelope = geometry.Extent;
                double coordX = envelope.Center.X;
                double coordY = envelope.Center.Y;

                Geometry geometriaSelezionata = null;

                var spatialQuery = new SpatialQueryFilter()
                {
                    FilterGeometry = geometry,
                    SpatialRelationship = SpatialRelationship.Intersects
                };

                if (Module1.DockpaneGeomInPoligonoView.FeatureLayerPoligonoScelto != null)
                {
                    await QueuedTask.Run(() =>
                    {
                        Module1.DockpaneGeomInPoligonoView.FeatureLayerPoligonoScelto.Select(spatialQuery);

                        Selection selezione = Module1.DockpaneGeomInPoligonoView.FeatureLayerPoligonoScelto.GetSelection();
                        if (selezione.GetCount() > 0)
                        {
                            IReadOnlyList<long> listaOID = selezione.GetObjectIDs();
                            long OID = listaOID[0];

                            QueryFilter queryFilter = new QueryFilter()
                            {
                                ObjectIDs = listaOID
                            };

                            RowCursor rowCursor = Module1.DockpaneGeomInPoligonoView.FeatureLayerPoligonoScelto.Search(queryFilter);

                            while (rowCursor.MoveNext())
                            {
                                using (var feat = rowCursor.Current as Feature)
                                {
                                    geometriaSelezionata = feat.GetShape();
                                    break;
                                }
                            }
                        }
                    });

                    // Ho terminato l'esecuzione del codice nel QueuedTask, ora procedo con l'analisi spaziale:

                    if (geometriaSelezionata != null)
                    {
                        await QueuedTask.Run(() =>
                        {
                            // Inizializzo variabile con lista Feature:
                            ObservableCollection<string> ListaInfoSuFeature = new ObservableCollection<string>();

                            FeatureLayer featureLayer = null;
                            List<long> listaOID = null;

                            Dictionary<BasicFeatureLayer, List<long>> featuresObjectIds = MapView.Active.GetFeatures(geometriaSelezionata);
                            
                            foreach (var fLayerOID in featuresObjectIds)
                            {
                                featureLayer = fLayerOID.Key as FeatureLayer;
                                listaOID = fLayerOID.Value;

                                int numOggetti = listaOID.Count;

                                ListaInfoSuFeature.Add($@"{featureLayer.Name}, # geometrie:  {numOggetti}");
                            }

                            _ = QueuedTask.Run(() =>
                              {
                                  Notification notification = new Notification
                                  {
                                      Title = "GiancaGIS",
                                      Message = "Analisi spaziale effettuata!"
                                  };
                                //notification.ImageUrl =
                                //@"pack://application:,,,/AddInGeomInPoligono;component/Images/Profilo.png";

                                FrameworkApplication.AddNotification(notification);

                                  Module1.DockpaneGeomInPoligonoViewModel.EliminaListaFeature();

                                  foreach (string info in ListaInfoSuFeature)
                                  {
                                      Module1.DockpaneGeomInPoligonoViewModel.AggiungiListaFeature(info);
                                  };
                              });
                        });
                    }
                }
            }
            catch (System.Exception errore)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(errore.Message, "errore", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                throw;
            }

            return true;
        }

    }
}
