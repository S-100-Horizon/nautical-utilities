using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

using System;
using System.Linq;

namespace NauticalUtilitiesProAppModule.DynamicMenus
{
    internal class ButtonClear : Button
    {
        protected override async void OnClick() {
            await QueuedTask.Run(() => {
                var layer = MapView.Active.GetSelectedLayers().ToList().FirstOrDefault();

                if (layer is FeatureLayer) {
                    var featureLayer = (FeatureLayer)layer;

                    ClearDefinitionQuery(featureLayer);
                }
                else if (layer is GroupLayer) {
                    var groupLayer = (GroupLayer)layer;
                    foreach (var l in groupLayer.GetLayersAsFlattenedList()) {
                        if (l is FeatureLayer) {
                            var featureLayer = (FeatureLayer)l;
                            ClearDefinitionQuery(featureLayer);
                        }
                    }
                }
            });
        }

        private void ClearDefinitionQuery(FeatureLayer layer) {
            var queries = layer.DefinitionQueries;

            try {
                var q = queries.SingleOrDefault(e => e.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase));
                if (q != default) {
                    layer.SetActiveDefinitionQuery(q.Name);
                }
                else {
                    switch (layer.ActiveDefinitionQuery?.Name) {
                        case "Overview":
                        case "General":
                        case "Coastal":
                        case "Approach":
                        case "Harbour":
                        case "Berthing":
                            layer.RemoveActiveDefinitionQuery();
                            break;
                    }
                }
            }
            catch (System.InvalidOperationException ex) {
                Logger.Current.Error(ex, "ClearDefinitionQuery({layer})", layer.Name);
            }
        }
    }
}
