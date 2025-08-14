using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PDM.Src.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Net;

namespace PDM.Src.ViewModels
{
    class GroupMappingViewModel
    {
        public ObservableCollection<GroupMappingEntry> GroupMappings { get; set; }
        public ICommand PrintCommand { get; }

        private readonly Dictionary<string, string> _brandColors = new();
        private readonly List<string> AvailableColors = new()
        {
            "#f4cccc", // light pink
            "#c9daf8", // light blue
            "#d9ead3", // light green
            "#fff2cc", // light yellow
            "#ead1dc", // light lavender
            "#cfe2f3", // pale sky blue
            "#fce5cd", // peach
            "#d0e0e3", // teal gray
            "#e6b8af", // soft coral
            "#b6d7a8", // mint green
            "#a2c4c9", // dusty teal
            "#ffe599", // light amber
            "#b4a7d6", // pastel purple
            "#76a5af"  // muted aqua
        };

        public GroupMappingViewModel()
        {
            GroupMappings = new ObservableCollection<GroupMappingEntry>(LoadMappings());
            PrintCommand = new RelayCommand(Print);

        }

        private List<GroupMappingEntry> LoadMappings()
        {
            var db = App.ServiceProvider.GetRequiredService<DatabaseManager>().GetDatabase();
            if (db != null)
            {
                var phones = db.GetCollection<Phone>("phones").FindAll();
                return phones.GroupBy(p => p.GroupId).Select(g => {
                    string brand = g.First().Brand;

                    BrandColor(brand);

                    return new GroupMappingEntry
                    {
                        Brand = brand,
                        GroupId = g.Key,
                        Models = string.Join(", ", g.Select(x => x.Model.ToString()).Distinct())
                    };
                
                }).ToList();
            }
            return null;
        }

        private void Print()
        {
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "GroupMappingTemplate.html");
            if (!File.Exists(templatePath))
            {
                MessageBox.Show($"File not found: {templatePath}");
                return;
            }

            string template = File.ReadAllText(templatePath);

            var rows = new StringBuilder();
            foreach (var entry in GroupMappings)
            {
                string bg = BrandColor(entry.Brand);
                string brand = WebUtility.HtmlEncode(entry.Brand ?? "");
                string group = WebUtility.HtmlEncode(entry.GroupId.ToString() ?? "");

                // If Models is IEnumerable<string>
                string models = entry.Models is string seq
                    ? WebUtility.HtmlEncode(string.Join(", ", seq))
                    : WebUtility.HtmlEncode(entry.Models?.ToString() ?? "");

                rows.AppendLine(
                    $"<tr>" +
                    $"<td>{group}</td>" +
                    $"<td style=\"background:{bg};\">{brand}</td>" +
                    $"<td>{models}</td>" +
                    $"</tr>");
            }

            string finalHtml = template.Replace("{{rows}}", rows.ToString());

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "GroupMappingReport",
                DefaultExt = ".html",
                Filter = "HTML Files (*.html)|*.html"
            };

            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, finalHtml);
                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
        }

        private string BrandColor(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
                return "#eeeeee"; // default gray for missing brand

            // Already assigned
            if (_brandColors.TryGetValue(brand, out var existingColor))
                return existingColor;

            // No more colors in the pool
            if (AvailableColors.Count == 0)
                return "#eeeeee"; // default if out of colors

            // Take first available color
            var color = AvailableColors[0];
            AvailableColors.RemoveAt(0);

            // Assign and return
            _brandColors[brand] = color;
            return color;
        }

    }

    public class GroupMappingEntry
    {
        public string Brand { get; set; }
        public int GroupId { get; set; }
        public string Models { get; set; }
    }
}
