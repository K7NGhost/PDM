using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PDM.Src.ViewModels
{
    class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<SettingsViewModel> _logger;
        private readonly string ThemeFolder = "Themes";
        private ObservableCollection<string> _themes = new();
        public ObservableCollection<string> Themes
        {
            get => _themes;
            set
            {
                _themes = value;
                OnPropertyChanged(nameof(Themes));
            }
        }

        private string? _selectedTheme;
        public string? SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme != value)
                {
                    _selectedTheme = value;
                    OnPropertyChanged(nameof(SelectedTheme));

                    if (!string.IsNullOrEmpty(_selectedTheme))
                        ChangeTheme(_selectedTheme);
                }
            }
        }

        public SettingsViewModel()
        {
            _logger = App.ServiceProvider.GetRequiredService<ILogger<SettingsViewModel>>();
            LoadAvailableThemes();
        }
        private void LoadAvailableThemes()
        {
            var themesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ThemeFolder);
            if (!Directory.Exists(themesPath))
            {
                _logger.LogWarning($"{themesPath} does not exist");
                Themes = new ObservableCollection<string>();
                return;
            }

            var themeList = Directory
                .GetFiles(themesPath, "*.xaml", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();

            Themes = new ObservableCollection<string>(themeList);
            _logger.LogInformation($"Loaded {Themes.Count} themes");
        }

        public void ChangeTheme(string themeName)
        {
            string themeFile = Path.Combine(ThemeFolder, themeName + ".xaml");
            if (!File.Exists(themeFile))
            {
                throw new FileNotFoundException($"Theme file not found: {themeFile}");
            }

            var appResources = Application.Current.Resources.MergedDictionaries;
            appResources[1] = new ResourceDictionary
            {
                Source = new Uri(themeFile, UriKind.RelativeOrAbsolute)
            };
      
            _logger.LogInformation($"Theme changed to {themeName}");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
