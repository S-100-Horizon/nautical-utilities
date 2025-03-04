using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;



namespace Geodatastyrelsen.ArcGIS.Modules
{
    internal class SvgConverterProAppModule : Module
    {
        private static SvgConverterProAppModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static SvgConverterProAppModule Current => _this ??= (SvgConverterProAppModule)FrameworkApplication.FindModule("SvgConverterProAppModule_Module");

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload() {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
