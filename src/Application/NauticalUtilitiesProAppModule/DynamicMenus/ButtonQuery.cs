using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NauticalUtilitiesProAppModule.DynamicMenus
{
    internal abstract class ButtonQuery : Button
    {
        protected abstract string Name { get; }

        //protected abstract string WhereClause { get; }

        protected override void OnClick() {
            QueuedTask.Run(() => {
                var layer = MapView.Active.GetSelectedLayers().ToList().FirstOrDefault();

                string compilationScale = "45000";

                if (layer is FeatureLayer) {
                    var featureLayer = (FeatureLayer)layer;

                    var v = featureLayer.GetCompilationScale(this.Name);

                    compilationScale = $"{v.CompilationScale}";

                    var fields = featureLayer.GetFieldDescriptions();
                    if (fields.Any(e => e.Name.Equals("PLTS_COMP_SCALE", StringComparison.InvariantCultureIgnoreCase)))
                        this.SetDefinitionQuery(featureLayer, Name, v.WhereClause);
                    else if (fields.Any(e => e.Name.Equals("maximumDisplayScale", StringComparison.InvariantCultureIgnoreCase)))
                        this.SetDefinitionQuery(featureLayer, Name, v.WhereClause);
                }
                else if (layer is GroupLayer) {
                    var groupLayer = (GroupLayer)layer;
                    foreach (var l in groupLayer.GetLayersAsFlattenedList()) {
                        if (l is FeatureLayer) {
                            var featureLayer = (FeatureLayer)l;

                            var v = featureLayer.GetCompilationScale(this.Name);

                            compilationScale = $"{v.CompilationScale}";

                            var fields = featureLayer.GetFieldDescriptions();
                            if (fields.Any(e => e.Name.Equals("PLTS_COMP_SCALE", StringComparison.InvariantCultureIgnoreCase)))
                                this.SetDefinitionQuery(featureLayer, Name, v.WhereClause);
                            else if (fields.Any(e => e.Name.Equals("maximumDisplayScale", StringComparison.InvariantCultureIgnoreCase)))
                                this.SetDefinitionQuery(featureLayer, Name, v.WhereClause);
                        }
                    }
                }

                var pluginWrapper = FrameworkApplication.GetPlugInWrapper("esri_maritime_currentS57CompilationScale");
                if (pluginWrapper != null) {
                    var propInfo = pluginWrapper.GetType().GetProperty("PlugIn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (propInfo != null) {
                        var oPlugin = propInfo.GetValue(pluginWrapper);
                        if (oPlugin != null) {
                            var compScalePropInfo = oPlugin.GetType().GetProperty("CompilationScaleString", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                            if (compScalePropInfo != null) {
                                compScalePropInfo.SetValue(oPlugin, compilationScale);
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
                layer.SetActiveDefinitionQuery(name);
            else {
                if (string.IsNullOrEmpty(query))
                    layer.InsertDefinitionQuery(new DefinitionQuery(name, whereClause), true);
                else {
                    layer.InsertDefinitionQuery(new DefinitionQuery(name, $"({query}) AND ({whereClause})"), true);
                }
            }
        }
    }
}

namespace ArcGIS.Desktop.Mapping
{
    public static class Extension
    {
        static ConcurrentDictionary<string, Func<string, (long, string)>> _compilationScale = new ConcurrentDictionary<string, Func<string, (long, string)>>();

        public static (long CompilationScale, string WhereClause) GetCompilationScale(this FeatureLayer featureLayer, string name) {
            _compilationScale.GetOrAdd(MapView.Active.Map.Name, (name) => {
                using var datastore = featureLayer.GetFeatureClass().GetDatastore();

                if (!(datastore is UnknownDatastore)) {

                    Geodatabase geodatabase = datastore as Geodatabase;

                    var syntax = geodatabase.GetSQLSyntax();
                    var tableNames = syntax.ParseTableName(featureLayer.GetFeatureClass().GetName());


                    using var table = geodatabase.OpenDataset<Table>(syntax.QualifyTableName(tableNames.Item1, tableNames.Item2, "EditingProperties"));

                    var query = new QueryFilter {
                        WhereClause = $"UPPER(Username) = '{Environment.UserName.ToUpperInvariant()}'",
                    };

                    using var cursor = table.Search(query, true);

                    cursor.MoveNext();
                    if (cursor.Current != null) {
                        var agency = Convert.ToString(cursor.Current["Agency"]);

                        return agency?.ToUpperInvariant() switch {
                            "US" or "U1" or "U2" or "U3" or "U4" => (name) => name switch {
                                "Overview" => (700000, $"(PLTS_COMP_SCALE >= {700000})"),
                                "General" => (700000, $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {700000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE >= {700000} AND PLTS_COMP_SCALE < {3500000}))"),
                                "Coastal" => (180000, $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {180000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE >= {180000} AND PLTS_COMP_SCALE < {700000}))"),
                                "Approach" => (45000, $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {45000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE >= {45000} AND PLTS_COMP_SCALE < {180000}))"),
                                "Harbour" => (12000, $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {12000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE >= {12000} AND PLTS_COMP_SCALE < {45000}))"),
                                "Berthing" => (2000, $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {12000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE < {12000}))"),
                                _ => (45000, $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {45000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE >= {45000} AND PLTS_COMP_SCALE < {180000}))"),
                            },
                            "DK" => (name) => name switch {
                                "Overview" => (3500000, $"(PLTS_COMP_SCALE >= {3500000})"),
                                "General" => (180000, $"(PLTS_COMP_SCALE >= {180000} AND PLTS_COMP_SCALE < {700000})"),
                                "Coastal" => (90000, $"(PLTS_COMP_SCALE >= {90000} AND PLTS_COMP_SCALE < {180000})"),
                                "Approach" => (22000, $"(PLTS_COMP_SCALE >= {22000} AND PLTS_COMP_SCALE < {90000})"),
                                "Harbour" or "Berthing" => (12000, $"(PLTS_COMP_SCALE < {22000})"),
                                _ => (22000, $"(PLTS_COMP_SCALE >= {22000} AND PLTS_COMP_SCALE < {90000})"),
                            },

                            _ => (name) => (45000, $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {45000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE >= {45000} AND PLTS_COMP_SCALE < {180000}))"),
                        };
                    }
                }

                return (name) => (45000, $"(IS_CONFLATE = 1 And PLTS_COMP_SCALE >= {45000}) Or ((IS_CONFLATE = 0 Or IS_CONFLATE IS NULL) And (PLTS_COMP_SCALE >= {45000} AND PLTS_COMP_SCALE < {180000}))");
            });

            return _compilationScale[MapView.Active.Map.Name].Invoke(name);
        }
    }
}