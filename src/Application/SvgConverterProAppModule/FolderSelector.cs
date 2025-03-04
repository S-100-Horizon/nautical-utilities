using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using System.Linq;

namespace Geodatastyrelsen.ArcGIS.Modules
{
    internal static class FolderSelector
    {
        public static string SelectFolder(string title) {
            var folderFilter = BrowseProjectFilter.GetFilter("esri_browseDialogFilters_folders");
            var dlg = new OpenItemDialog() {
                BrowseFilter = folderFilter,
                Title = title
            };
            if (!dlg.ShowDialog().Value)
                return default;
            var item = dlg.Items.First();
            return item.Path;
        }
    }
}
