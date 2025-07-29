using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PDM.Src.Enums;
using PDM.Src.Models;
using PDM.ui.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PDM.Src.ViewModels
{
    internal class PhoneLookupViewModel : ObservableObject
    {
        private readonly ILogger<PhoneLookupViewModel> _logger;
        public ObservableCollection<PhoneBrand> Brands { get; }
        public ObservableCollection<PhoneModel> Models { get; }
        private PhoneMapping _selectedItem;
        public PhoneMapping SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    PerformSearch();
                }
            }
        }

        private PhoneMapping _searchResult;
        public PhoneMapping SearchResult
        {
            get => _searchResult;
            set => SetProperty(ref _searchResult, value);
        }

        public IRelayCommand OpenAddPhoneMapCommand { get; }

        public PhoneLookupViewModel()
        {
            _logger = App.ServiceProvider.GetRequiredService<ILogger<PhoneLookupViewModel>>();
            Brands = new ObservableCollection<PhoneBrand>(Enum.GetValues<PhoneBrand>());
            Models = new ObservableCollection<PhoneModel>(Enum.GetValues<PhoneModel>());

            OpenAddPhoneMapCommand = new RelayCommand(OpenAddPhoneMapWindow);
        }

        private void OpenAddPhoneMapWindow()
        {
            _logger.LogInformation("Attempting to open phone mapping window");
            SelectedItem = new PhoneMapping();
            var window = new AddPhoneMappingWindow
            {
                DataContext = this
            };

            if (window.ShowDialog() == true)
            {
                if (SelectedItem == null) return;

                var newMapping = new PhoneMapping
                {
                    ManufacturerId = SelectedItem.ManufacturerId,
                    Brand = SelectedItem.Brand,
                    Model = SelectedItem.Model,
                    ReleaseYear = SelectedItem.ReleaseYear
                };

                var db = App.ServiceProvider.GetRequiredService<DatabaseManager>().GetDatabase();
                var col = db.GetCollection<PhoneMapping>("mappings");
                col.Insert(newMapping);
                MessageBox.Show("Successfully Added new record to database");
                _logger.LogInformation("Successfully Added new record to database");
            }
            else
            {
                _logger.LogInformation("show dialog for (addphonemap) did not return true");
            }
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchText = null;
                return;
            }

            var db = App.ServiceProvider.GetRequiredService<DatabaseManager>().GetDatabase();
            if (db != null)
            {
                var col = db.GetCollection<PhoneMapping>("mappings");
                SearchResult = col.FindOne(x => x.ManufacturerId.Equals(SearchText, StringComparison.OrdinalIgnoreCase));
            }
            
        }
    }
}
