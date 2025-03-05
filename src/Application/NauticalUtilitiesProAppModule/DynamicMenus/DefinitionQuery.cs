using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using System.Collections.Concurrent;
using System.Linq;

namespace NauticalUtilitiesProAppModule.DynamicMenus
{
    internal class DynamicMenuDefinitionQuery : DynamicMenu
    {
        
        protected override void OnPopup() {
            var layer = MapView.Active.GetSelectedLayers().ToList().FirstOrDefault();

            base.Enabled = layer != default;

            this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_ButtonOverview");
            this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_ButtonGeneral");
            this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_ButtonCoastal");
            this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_ButtonApproach");
            this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_ButtonHarbour");
            //this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_Button1000");
            //this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_Button2000");
            //this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_Button3000");
            //this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_Button4000");
            //this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_Button8000");
            //this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_Button12000");
            //this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_Button22000");
            //this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_Button45000");
            //this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_Button90000");
            //this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_Button120000");
            //this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_Button180000");
            //this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_Button350000");
            this.AddReference("NauticalUtilitiesProAppModule_DynamicMenus_ButtonClear");
        }

        protected override void OnUpdate() {
            base.OnUpdate();
        }
    }
}
