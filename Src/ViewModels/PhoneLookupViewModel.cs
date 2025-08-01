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
                    PerformSearch();
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

            // 🔹 Load from DB instead of enums
            var db = App.ServiceProvider.GetRequiredService<DatabaseManager>().GetDatabase();
            if (db != null)
            {
                Brands = new ObservableCollection<PhoneBrand>(db.GetCollection<PhoneBrand>("brands").FindAll());
                Models = new ObservableCollection<PhoneModel>(db.GetCollection<PhoneModel>("models").FindAll());

                OpenAddPhoneMapCommand = new RelayCommand(OpenAddPhoneMapWindow);
            }
            
        }

        private void OpenAddPhoneMapWindow()
        {
            _logger.LogInformation("Attempting to open phone mapping window");

            // Use a fresh mapping for the window
            var newVm = new PhoneLookupViewModel
            {
                SelectedItem = new PhoneMapping()
            };

            var window = new AddPhoneMappingWindow
            {
                DataContext = newVm
            };

            if (window.ShowDialog() == true && newVm.SelectedItem != null)
            {
                var newMapping = new PhoneMapping
                {
                    ManufacturerId = newVm.SelectedItem.ManufacturerId,
                    Brand = newVm.SelectedItem.Brand,
                    Model = newVm.SelectedItem.Model,
                    Countries = newVm.SelectedItem.Countries,
                    ReleaseYear = newVm.SelectedItem.ReleaseYear
                };

                var db = App.ServiceProvider.GetRequiredService<DatabaseManager>().GetDatabase();
                var col = db.GetCollection<PhoneMapping>("mappings");
                col.Insert(newMapping);

                MessageBox.Show("Successfully added new record to database");
                _logger.LogInformation("Successfully added new record to database");
            }
            else
            {
                _logger.LogInformation("AddPhoneMappingWindow was cancelled or returned null item");
            }
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchResult = null;
                return;
            }

            var db = App.ServiceProvider.GetRequiredService<DatabaseManager>().GetDatabase();
            if (db != null)
            {
                var col = db.GetCollection<PhoneMapping>("mappings");
                SearchResult = col.FindOne(x =>
                    !string.IsNullOrEmpty(x.ManufacturerId) &&
                    x.ManufacturerId.Equals(SearchText, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
