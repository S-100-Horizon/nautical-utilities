using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

/*
 * Select folder with raw symbols from IHO portrayal catalogs
 * 
 * Folders will be created for each css found in the raw symbols folder.
 * All referenced css files in the raw svgs will be verified and loaded before conversion
 * 
 * A new folder foreach css will be created at the same location as the raw folder 
 * 
 * All svgs will be converted and all source style and class atributes in the source svg will be replaced by a single inline style attribute containing the css
 * 
 * The stylx files will all be created in the RAW folder.
 *  
 */



#nullable disable
namespace Geodatastyrelsen.ArcGIS.Modules
{
    internal class ButtonConvertSvgToStylx : Button
    {
        protected override void OnClick() {
            var markerSymbolsPath = FolderSelector.SelectFolder("Select a folder with RAW svg marker symbol files to be added to a new stylx file");
            if (markerSymbolsPath == default)
                return;

            var type = "point";

            var referencedCss = new Dictionary<string, string>();


            try {
                using (var progress = new ProgressDialog("Initializing")) {
                    var status = new CancelableProgressorSource(progress);
                    progress.Show();


                    var referencedStyleSheets = new Dictionary<string, string>();

                    foreach (var svgFile in Directory.GetFiles(markerSymbolsPath, "*.svg")) {
                        using (StreamReader reader = new StreamReader(svgFile)) {
                            string line;
                            while ((line = reader.ReadLine()) != null) {
                                Match match = Regex.Match(line, @"href=""([^""]+)""");
                                if (match.Success) {
                                    var fullValue = match.Value;
                                    string cssFile = match.Groups[1].Value;
                                    if (cssFile.ToLower().EndsWith(".css")) {
                                        var fileName = System.IO.Path.GetFileNameWithoutExtension(cssFile);
                                        if (referencedCss.ContainsKey(fileName.ToLower()))
                                            continue;

                                        referencedCss.Add((System.IO.Path.GetFileNameWithoutExtension(cssFile).ToLower()), cssFile);
                                    }

                                }
                            }
                        }
                    }

                    var existingCssFiles = Directory.GetFiles(markerSymbolsPath, "*.css");

                    Dictionary<string, string> css = existingCssFiles.ToDictionary(
                        filePath => Path.GetFileNameWithoutExtension(filePath).ToLower(), 
                        filePath => filePath                   
                    );

                    foreach (var item in referencedCss) {
                        if (!css.ContainsKey(item.Key.ToLower())) {
                            MessageBox.Show($"Cannot find referenced css {item.Value}. Make sure this file is present in {markerSymbolsPath}.", "Convert SVG to stylx", System.Windows.MessageBoxButton.OK);
                            return;
                        }
                    }

                    QueuedTask.Run(async () => {
                        foreach (var style in css) {
                            var path = style.Value;
                            if (!System.IO.Path.Exists(path)) {
                                MessageBox.Show($"Cannot find {path}. Make sure this file is present.", "Convert SVG to stylx", System.Windows.MessageBoxButton.OK);
                                return false;
                                //throw new ArgumentException($"Cannot find {path}. Make sure this file is present.");
                            }
                        }


                        status.Progressor.Message = "Loading styles";
                        foreach (var style in css) {
                            var allStyles = new Dictionary<string, string>();
                            var outputFolder = System.IO.Path.GetFullPath(System.IO.Path.Join(markerSymbolsPath, "..", $"Symbols_Converted_{style.Key}"));

                            if (!Directory.Exists(outputFolder)) {
                                Directory.CreateDirectory(outputFolder);
                            }

                            string cssContent = File.ReadAllText(style.Value);
                            //Console.WriteLine(cssContent);

                            string pattern = @"\.([a-zA-Z0-9_-]+)\s*\{([^}]+)\}";

                            MatchCollection matches = Regex.Matches(cssContent, pattern);

                            foreach (Match match in matches) {
                                string className = match.Groups[1].Value; // Class name
                                string styles = match.Groups[2].Value.Trim().Trim(';'); // Styles

                                allStyles.Add(className, styles);
                            }

                            string classPattern = @"class\s*=\s*""([^""]*)""";

                            status.Progressor.Message = $"Converting SVG files for {style.Value}";
                            foreach (var svgFile in Directory.GetFiles(markerSymbolsPath, "*.svg")) {
                                var outputDocument = System.IO.Path.Join(outputFolder, @$"{System.IO.Path.GetFileName(svgFile)}");

                                using (StreamWriter writer = new StreamWriter(outputDocument)) {

                                    using (StreamReader reader = new StreamReader(svgFile)) {
                                        string line;
                                        while ((line = reader.ReadLine()) != null) {
                                            string styleSheetPattern = @"<\?xml-stylesheet[^>]*\?>";

                                            if (line.ToLower().StartsWith("<?xml-stylesheet"))
                                                line = Regex.Replace(line, styleSheetPattern, "");

                                            Match match = Regex.Match(line, classPattern);
                                            if (match.Success) {

                                                var fullValue = match.Value;
                                                string classValue = match.Groups[1].Value;

                                                string[] itemsArray = classValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                                List<string> itemsList = new List<string>(itemsArray);


                                                var builder = new StringBuilder();

                                                foreach (var item in itemsList) {
                                                    var styleValue = $"{allStyles[item]};";
                                                    builder.Append(styleValue);
                                                }

                                                string contentStyle = @"style\s*=\s*""([^""]*)""";

                                                Match styleMatch = Regex.Match(line, contentStyle);

                                                if (styleMatch.Success) {
                                                    // store existing style value
                                                    string styleValue = styleMatch.Groups[1].Value;
                                                    builder.Append(styleValue);


                                                    // Remove the style attribute 
                                                    string replacePattern = @"\s*style\s*=\s*""[^""]*""";

                                                    string result = Regex.Replace(line, replacePattern, "");
                                                    line = Regex.Replace(line, replacePattern, "");

                                                }

                                                var newStyleLine = $"style=\"{builder.ToString()}\"";

                                                var newline = line.Replace(fullValue, newStyleLine);

                                                writer.WriteLine(newline);
                                            }
                                            else {
                                                writer.WriteLine(line);
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        return true;
                    }, status.Progressor, System.Threading.Tasks.TaskCreationOptions.LongRunning);

                    QueuedTask.Run((Func<Task>)(() => {
                        try {

                            foreach (var styleSheet in css) {
                                var allStyles = new Dictionary<string, string>();
                                var outputFolder = System.IO.Path.GetFullPath(System.IO.Path.Join(markerSymbolsPath, "..", $"Symbols_Converted_{styleSheet.Key}"));


                                IEnumerable<string> svgPaths = Directory.EnumerateFiles(outputFolder, "*", SearchOption.TopDirectoryOnly).Where<string>((Func<string, bool>)(s => s.EndsWith(".svg")));
                                if (svgPaths.Count<string>() > 0) {
                                    var inputFolderName = System.IO.Path.GetFileName(outputFolder);
                                    string stylePath = System.IO.Path.Join(markerSymbolsPath, $"S-101_{inputFolderName}_v{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.stylx");

                                    StyleHelper.CreateStyle(Project.Current, stylePath);
                                    StyleProjectItem style = Project.Current.GetItems<StyleProjectItem>().First<StyleProjectItem>((Func<StyleProjectItem, bool>)(x => ((Item)x).Path == stylePath));
                                    SymbolStyleItem styleItem = (SymbolStyleItem)null;
                                    CIMSymbol symbol = (CIMSymbol)null;
                                    CIMMarker marker = (CIMMarker)null;
                                    if (type == "point") {
                                        foreach (string svg in svgPaths) {
                                            status.Progressor.Message = $"Loading {svg}";
                                            styleItem = new SymbolStyleItem();
                                            marker = SymbolFactory.Instance.ConstructMarkerFromFile(svg);
                                            symbol = (CIMSymbol)SymbolFactory.Instance.ConstructPointSymbol(marker);
                                            styleItem.Symbol = symbol;
                                            ((StyleItem)styleItem).Name = Path.GetFileNameWithoutExtension(svg);
                                            StyleHelper.AddItem(style, (StyleItem)styleItem);
                                        }
                                    }

                                    style = (StyleProjectItem)null;
                                    styleItem = (SymbolStyleItem)null;
                                    symbol = (CIMSymbol)null;
                                    marker = (CIMMarker)null;
                                }
                                svgPaths = (IEnumerable<string>)null;
                            }
                            MessageBox.Show($"Done", "Convert SVG to stylx", System.Windows.MessageBoxButton.OK);
                        }
                        catch (Exception ex) {
                            Trace.WriteLine(ex.Message);
                        }

                        return Task.CompletedTask;
                    }), status.Progressor, System.Threading.Tasks.TaskCreationOptions.LongRunning);
                }



            }
            catch (Exception ex) {
                MessageBox.Show($"{ex.Message}", "Convert SVG to stylx", System.Windows.MessageBoxButton.OK);
            }
        }

    }
}