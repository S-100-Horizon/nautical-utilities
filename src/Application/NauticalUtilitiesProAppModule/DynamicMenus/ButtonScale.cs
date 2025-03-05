using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

using System;
using System.Linq;
using System.Reflection;

namespace NauticalUtilitiesProAppModule.DynamicMenus
{
    internal class ButtonScale : Button
    {
        protected override void OnClick() {
            var compilationScale = base.ID switch {
                "NauticalUtilitiesProAppModule_DynamicMenus_Button1000" => 1000,
                "NauticalUtilitiesProAppModule_DynamicMenus_Button2000" => 2000,
                "NauticalUtilitiesProAppModule_DynamicMenus_Button3000" => 3000,
                "NauticalUtilitiesProAppModule_DynamicMenus_Button4000" => 4000,
                "NauticalUtilitiesProAppModule_DynamicMenus_Button8000" => 8000,
                "NauticalUtilitiesProAppModule_DynamicMenus_Button12000" => 12000,
                "NauticalUtilitiesProAppModule_DynamicMenus_Button22000" => 22000,
                "NauticalUtilitiesProAppModule_DynamicMenus_Button45000" => 45000,
                "NauticalUtilitiesProAppModule_DynamicMenus_Button90000" => 90000,
                "NauticalUtilitiesProAppModule_DynamicMenus_Button180000" => 180000,
                "NauticalUtilitiesProAppModule_DynamicMenus_Button350000" => 350000,
                _ => 45000,
            };

            var whereClause = $"PLTS_COMP_SCALE = {compilationScale}";

            var name = "CustomScale";

            QueuedTask.Run(() => {
                //var name = MapView.Active.Map.Name;

                var layer = MapView.Active.GetSelectedLayers().ToList().FirstOrDefault();

                if (layer is FeatureLayer) {
                    var featureLayer = (FeatureLayer)layer;

                    var fields = featureLayer.GetFieldDescriptions();
                    if (fields.Any(e => e.Name.Equals("PLTS_COMP_SCALE", StringComparison.InvariantCultureIgnoreCase)))
                        this.SetDefinitionQuery(featureLayer, name, whereClause);
                    else if (fields.Any(e => e.Name.Equals("maximumDisplayScale", StringComparison.InvariantCultureIgnoreCase)))
                        this.SetDefinitionQuery(featureLayer, name, whereClause);
                }
                else if (layer is GroupLayer) {
                    var groupLayer = (GroupLayer)layer;
                    foreach (var l in groupLayer.GetLayersAsFlattenedList()) {
                        if (l is FeatureLayer) {
                            var featureLayer = (FeatureLayer)l;

                            var fields = featureLayer.GetFieldDescriptions();
                            if (fields.Any(e => e.Name.Equals("PLTS_COMP_SCALE", StringComparison.InvariantCultureIgnoreCase)))
                                this.SetDefinitionQuery(featureLayer, name, whereClause);
                            else if (fields.Any(e => e.Name.Equals("maximumDisplayScale", StringComparison.InvariantCultureIgnoreCase)))
                                this.SetDefinitionQuery(featureLayer, name, whereClause);
                        }
                    }
                }

                //base.IsChecked = true;

                var pluginWrapper = FrameworkApplication.GetPlugInWrapper("esri_maritime_currentS57CompilationScale");
                if (pluginWrapper != null) {
                    var propInfo = pluginWrapper.GetType().GetProperty("PlugIn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (propInfo != null) {
                        var oPlugin = propInfo.GetValue(pluginWrapper);
                        if (oPlugin != null) {
                            var compScalePropInfo = oPlugin.GetType().GetProperty("CompilationScaleString", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                            if (compScalePropInfo != null) {
                                compScalePropInfo.SetValue(oPlugin, $"{compilationScale}");
                            }
                        }
                    }
                }

            });
        }

        private void SetDefinitionQuery(FeatureLayer layer, string name, string whereClause) {
            var queries = layer.DefinitionQueries;

            var query = string.Empty;

            if (layer.ActiveDefinitionQuery != default) {
                query = layer.ActiveDefinitionQuery.Name switch {
                    "Overview" => string.Empty,
                    "General" => string.Empty,
                    "Coastal" => string.Empty,
                    "Approach" => string.Empty,
                    "Harbour" => string.Empty,
                    "Berthing" => string.Empty,
                    "CustomScale" => string.Empty,
                    "Default" => layer.ActiveDefinitionQuery.WhereClause,
                    _ => layer.ActiveDefinitionQuery.WhereClause,
                };

                if (string.IsNullOrEmpty(query)) {
                    var q = layer.DefinitionQueries.SingleOrDefault(e => e.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase));
                    if (q != default)
                        query = q.WhereClause;
                }
            }

            if (queries.Any(e => e.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                layer.RemoveDefinitionQueries([name]);

            if (string.IsNullOrEmpty(query))
                layer.InsertDefinitionQuery(new DefinitionQuery(name, whereClause), true);
            else {
                layer.InsertDefinitionQuery(new DefinitionQuery(name, $"({query}) AND ({whereClause})"), true);
            }

        }
    }
}
