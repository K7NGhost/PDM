using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PDM.Src.Enums;
using PDM.Src.Models;
using PDM.ui.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PDM.Src.ViewModels
{
    class PhoneListViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<PhoneListViewModel> _logger;
        private readonly DatabaseManager _dbManager;
        private int _currentPage = 1;
        private const int PageSize = 200;

        // Filters (nullable means "no filter")
        public PhoneBrand? SelectedBrandFilter { get; set; }
        public PhoneModel? SelectedModelFilter { get; set; }
        public PhoneOS? SelectedOSFilter { get; set; }
        public PhoneCondition? SelectedConditionFilter { get; set; }
        public PhoneState? SelectedPhoneStateFilter { get; set; }
        public PhoneStatus? SelectedPhoneStatusFilter { get; set; }
        public PasscodeType? SelectedPasscodeTypeFilter { get; set; }

        public ObservableCollection<Phone> Phones { get; } = new();
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand EditPhoneCommand { get; }
        public ICommand OpenFilterCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public int CurrentPage
        {
            get => _currentPage;
            private set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public PhoneListViewModel()
        {
            _logger = App.ServiceProvider.GetRequiredService<ILogger<PhoneListViewModel>>();
            _dbManager = App.ServiceProvider.GetRequiredService<DatabaseManager>();

            NextPageCommand = new RelayCommand(NextPage, CanNextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage, () => _currentPage > 1);
            EditPhoneCommand = new RelayCommand<Phone>(OpenEditWindow);
            OpenFilterCommand = new RelayCommand(OpenFilterWindow);
            ClearFilterCommand = new RelayCommand(ClearFilters);

            LoadPage();
        }

        private void OpenFilterWindow()
        {
            var vm = new PhoneDataViewModel();
            var filterWindow = new FilterWindow { DataContext = vm };

            if (filterWindow.ShowDialog() == true)
            {
                SelectedBrandFilter.BrandName = vm.SelectedPhone.Brand;
                SelectedModelFilter.Brand = vm.SelectedPhone.Model;
                SelectedOSFilter.OSName = vm.SelectedPhone.OS;
                SelectedConditionFilter = vm.SelectedPhone.Condition;
                SelectedPhoneStateFilter = vm.SelectedPhone.PhoneState;
                SelectedPhoneStatusFilter = vm.SelectedPhone.Status;
                SelectedPasscodeTypeFilter = vm.SelectedPhone.PasscodeType;

                ApplyFilters();
                _logger.LogInformation("Filter applied");
            }
        }

        private void OpenEditWindow(Phone phone)
        {
            var vm = new PhoneDataViewModel
            {
                IsEditMode = true,
                SelectedPhone = new Phone
                {
                    Id = phone.Id,
                    GroupId = phone.GroupId,
                    Brand = phone.Brand,
                    Model = phone.Model,
                    OS = phone.OS,
                    IMEI = phone.IMEI,
                    Storage = phone.Storage,
                    Color = phone.Color,
                    Condition = phone.Condition,
                    PhoneState = phone.PhoneState,
                    PasscodeType = phone.PasscodeType,
                    PasscodeLength = phone.PasscodeLength,
                    Notes = phone.Notes,
                    ImageData = phone.ImageData,
                    Status = phone.Status
                }
            };

            var window = new EditPhoneWindow { DataContext = vm };
            window.Show();
        }

        private void LoadPage()
        {
            if (!_dbManager.IsOpen)
            {
                MessageBox.Show("Database not open");
                return;
            }

            Phones.Clear();
            var col = _dbManager.GetDatabase().GetCollection<Phone>("phones");
            var pageItems = col.Query()
                               .OrderBy(x => x.Id)
                               .Skip((_currentPage - 1) * PageSize)
                               .Limit(PageSize)
                               .ToList();

            foreach (var phone in pageItems)
                Phones.Add(phone);

            OnPropertyChanged(nameof(Phones));
        }

        private void NextPage()
        {
            _currentPage++;
            LoadPage();
            CommandManager.InvalidateRequerySuggested();
        }

        private void PreviousPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadPage();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public void ReloadPhones()
        {
            _currentPage = 1;
            LoadPage();
        }

        private bool CanNextPage()
        {
            var col = _dbManager.GetDatabase().GetCollection<Phone>("phones");
            var total = col.Count();
            return _currentPage * PageSize < total;
        }

        public void ApplyFilters()
        {
            if (!_dbManager.IsOpen)
            {
                MessageBox.Show("Database not open");
                return;
            }

            Phones.Clear();
            var col = _dbManager.GetDatabase().GetCollection<Phone>("phones");
            var query = col.Query();

            if (SelectedBrandFilter != null)
                query = query.Where(p => p.Brand == SelectedBrandFilter.BrandName);
            if (SelectedModelFilter != null)
                query = query.Where(p => p.Model == SelectedModelFilter.Brand);
            if (SelectedOSFilter != null)
                query = query.Where(p => p.OS == SelectedOSFilter.OSName);
            if (SelectedConditionFilter != null)
                query = query.Where(p => p.Condition == SelectedConditionFilter);
            if (SelectedPhoneStateFilter != null)
                query = query.Where(p => p.PhoneState == SelectedPhoneStateFilter);
            if (SelectedPhoneStatusFilter != null)
                query = query.Where(p => p.Status == SelectedPhoneStatusFilter);
            if (SelectedPasscodeTypeFilter != null)
                query = query.Where(p => p.PasscodeType == SelectedPasscodeTypeFilter);

            var filtered = query.OrderBy(p => p.Id).Limit(PageSize).ToList();

            foreach (var phone in filtered)
                Phones.Add(phone);

            OnPropertyChanged(nameof(Phones));
        }

        public void ClearFilters()
        {
            SelectedBrandFilter = null;
            SelectedModelFilter = null;
            SelectedOSFilter = null;
            SelectedConditionFilter = null;
            SelectedPhoneStateFilter = null;
            SelectedPhoneStatusFilter = null;
            SelectedPasscodeTypeFilter = null;

            ReloadPhones();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
