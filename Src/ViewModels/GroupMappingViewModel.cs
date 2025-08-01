using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PDM.Src.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PDM.Src.ViewModels
{
    class GroupMappingViewModel
    {
        public ObservableCollection<GroupMappingEntry> GroupMappings { get; set; }
        public ICommand PrintCommand { get; }

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
                return phones.GroupBy(p => p.GroupId).Select(g => new GroupMappingEntry { GroupId = g.Key, Models = string.Join(", ", g.Select(x => x.Model.ToString()).Distinct())}).ToList();
            }
            return null;
        }

        private void Print()
        {
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "GroupMappingTemplate.html");
            string template = File.ReadAllText(templatePath);

            var rows = new StringBuilder();
            foreach (var entry in GroupMappings)
            {
                rows.AppendLine($"<tr><td>{entry.GroupId}</td><td>{string.Join(", ", entry.Models)}</td></tr>");
            }

            string finalHtml = template.Replace("{{rows}}", rows.ToString());

            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GroupMappingReport.html");
            File.WriteAllText(outputPath, finalHtml);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = outputPath,
                UseShellExecute = true
            });
        }
    }

    public class GroupMappingEntry
    {
        public int GroupId { get; set; }
        public string Models { get; set; }
    }
}
