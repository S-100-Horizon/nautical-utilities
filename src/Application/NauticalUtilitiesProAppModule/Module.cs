﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.KnowledgeGraph;

namespace NauticalUtilitiesProAppModule
{
    internal class Module : ArcGIS.Desktop.Framework.Contracts.Module {
        private static Module _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module Current => _this ??= (Module)FrameworkApplication.FindModule("NauticalUtilitiesProAppModule_Module");

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
