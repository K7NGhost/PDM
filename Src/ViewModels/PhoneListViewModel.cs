using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
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
using System.Diagnostics;
using System.Windows.Data;

namespace PDM.Src.ViewModels
{
    internal partial class PhoneListViewModel : ObservableObject
    {
        private readonly ILogger<PhoneListViewModel> _logger;
        private readonly DatabaseManager _dbManager;

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int totalPages = 1;

        private const int PageSize = 300;
        private Stack<int> _prevPageAnchors = new();

        // Filters (nullable means "no filter")
        public string? SelectedBrandFilter { get; set; }
        public string? SelectedModelFilter { get; set; }
        public string? SelectedOSFilter { get; set; }
        public DeviceType? SelectedDeviceTypeFilter { get; set; }
        public PhoneCondition? SelectedConditionFilter { get; set; }
        public PhoneState? SelectedPhoneStateFilter { get; set; }
        public PhoneStatus? SelectedPhoneStatusFilter { get; set; }
        public PasscodeType? SelectedPasscodeTypeFilter { get; set; }

        public ObservableCollection<Phone> Phones { get; } = new();
        public ICommand EditPhoneCommand { get; }
        public ICommand OpenFilterCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand DeletePhoneCommand { get; }
        public ICommand ShowGroupMappingCommand { get; }

        [ObservableProperty]
        private int totalItems;

        public ObservableCollection<string> SortColumns { get; } = new()
        {
            nameof(Phone.Id), nameof(Phone.GroupId), nameof(Phone.Brand), nameof(Phone.Model), nameof(Phone.OS), nameof(Phone.Status), nameof(Phone.PasscodeType), nameof(Phone.Condition)
        };
        public ObservableCollection<string> SortDirection { get; } = new()
        {
            "Ascending", "Descending"
        };

        [ObservableProperty]
        private string selectedSortColumn = nameof(Phone.Id);

        [ObservableProperty]
        private string selectedSortDirection = "Ascending";

        public PhoneListViewModel()
        {
            _logger = App.ServiceProvider.GetRequiredService<ILogger<PhoneListViewModel>>();
            _dbManager = App.ServiceProvider.GetRequiredService<DatabaseManager>();
            EditPhoneCommand = new RelayCommand<Phone>(OpenEditWindow);
            OpenFilterCommand = new RelayCommand(OpenFilterWindow);
            ClearFilterCommand = new RelayCommand(ClearFilters);
            DeletePhoneCommand = new RelayCommand<Phone>(DeletePhone);
            ShowGroupMappingCommand = new RelayCommand(OpenGroupMapping);

            //LoadPage();
            LoadPageByAnchor(null);
        }

        private void DeletePhone(Phone phone)
        {
            if (phone == null)
            {
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete {phone.Brand} {phone.Model}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                var db = App.ServiceProvider.GetRequiredService<DatabaseManager>();
                db.DeletePhone(phone.Id);
                Phones.Remove(phone);
                App.ServiceProvider.GetRequiredService<DashboardViewModel>().LoadData();
            }
        }

        private void OpenGroupMapping()
        {
            var window = new GroupMappingWindow();
            window.DataContext = new GroupMappingViewModel();
            window.ShowDialog();
        }

        private void OpenFilterWindow()
        {
            var vm = new PhoneDataViewModel();
            var filterWindow = new FilterWindow { DataContext = vm };

            if (filterWindow.ShowDialog() == true)
            {
                SelectedBrandFilter = vm.SelectedBrand;
                SelectedModelFilter = vm.SelectedPhone.Model;
                SelectedOSFilter = vm.SelectedPhone.OS;
                SelectedDeviceTypeFilter = vm.SelectedPhone.DeviceType;
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
                    DeviceType = phone.DeviceType,
                    Color = phone.Color,
                    Condition = phone.Condition,
                    PhoneState = phone.PhoneState,
                    PasscodeType = phone.PasscodeType,
                    PasscodeLength = phone.PasscodeLength,
                    Notes = phone.Notes,
                    ImageDataB = phone.ImageDataB,
                    ImageDataF = phone.ImageDataF,
                    Status = phone.Status
                }
            };

            _logger.LogInformation("SelectedPhone.Brand = {Brand}, SelectedPhone.Model = {Model}, SelectedPhone.OS = {OS}",
    vm.SelectedPhone.Brand,
    vm.SelectedPhone.Model,
    vm.SelectedPhone.OS);


            var window = new EditPhoneWindow { DataContext = vm };
            vm.CloseWindowAction = () => window.Close();
            window.Show();
        }

        private void LoadPageByAnchor(int? afterId = null)
        {
            var sw = Stopwatch.StartNew();

            if (!_dbManager.IsOpen)
            {
                MessageBox.Show("Database not open");
                return;
            }

            var col = _dbManager.GetDatabase().GetCollection<Phone>("phones");

            // Make sure we have an index for whatever we're sorting on
            if (SelectedSortColumn == nameof(Phone.Id))
                col.EnsureIndex(x => x.Id);
            else
                col.EnsureIndex(SelectedSortColumn);   // dynamic field index

            // Count once per load (better: cache and refresh when data changes)
            TotalPages = (int)Math.Ceiling(col.Count() / (double)PageSize);

            var query = col.Query();

            // Keyset pagination ONLY works correctly when sorting by Id.
            if (SelectedSortColumn == nameof(Phone.Id))
            {
                if (afterId is int anchor)
                {
                    _logger.LogInformation("afterId anchor = {Anchor}", anchor);
                    query = SelectedSortDirection == "Ascending"
                        ? query.Where(x => x.Id > anchor)
                        : query.Where(x => x.Id < anchor);
                }

                query = SelectedSortDirection == "Ascending"
                    ? query.OrderBy(x => x.Id)
                    : query.OrderByDescending(x => x.Id);
            }
            else
            {
                // When sorting by a non-Id column we can't use an Id anchor safely.
                // Fall back to plain ordered first page. (Option: implement a per-column anchor.)
                if (afterId != null)
                    _logger.LogWarning("Ignoring afterId because sorting by {Col}. Implement per-column anchors to keyset paginate.", SelectedSortColumn);

                query = SelectedSortDirection == "Ascending"
                    ? query.OrderBy(SelectedSortColumn)
                    : query.OrderByDescending(SelectedSortColumn);
            }

            var pageItems = query
                .Limit(PageSize)
                .ToList();

            _logger.LogInformation("Fetched {Count} rows for page", pageItems.Count);

            Phones.Clear();
            foreach (var p in pageItems)
                Phones.Add(p);

            if (Phones.Count > 0)
            {
                // Anchor for "Next": last Id when ascending, first Id when descending
                var anchorToPush = SelectedSortDirection == "Ascending"
                    ? Phones[^1].Id
                    : Phones[0].Id;

                _prevPageAnchors.Push(anchorToPush);
            }
            else
            {
                _logger.LogWarning("No rows returned for this page.");
            }

            OnPropertyChanged(nameof(Phones));
            TotalItems = Phones.Count;
            _logger.LogInformation($"TotalItems is now {TotalItems} items");
            sw.Stop();
            _logger.LogInformation("LoadPageByAnchor took {ElapsedMs} ms", sw.ElapsedMilliseconds);
        }

        [RelayCommand]
        private void NextPage()
        {
            CurrentPage++;
            _logger.LogInformation($"CurrentPage is now {CurrentPage}");
            var afterId = Phones.Last().Id;
            _logger.LogInformation($"AfterId is {afterId}");
            LoadPageByAnchor(afterId);
            CommandManager.InvalidateRequerySuggested();
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                _logger.LogInformation($"CurrentPage is now {CurrentPage}");
                LoadPageByAnchor(_prevPageAnchors.Pop());
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public void ReloadPhones()
        {
            CurrentPage = 1;
            LoadPageByAnchor(null);
        }

        private bool CanNextPage()
        {
            var col = _dbManager.GetDatabase().GetCollection<Phone>("phones");
            var total = col.Count();
            return CurrentPage * PageSize < total;
        }

        public void ApplyFilters()
        {
            _logger.LogInformation(
    "Filters applied: Brand={Brand}, Model={Model}, OS={OS}, Condition={Condition}, State={State}, Status={Status}, Passcode={Passcode}",
    SelectedBrandFilter ?? "Any",
    SelectedModelFilter ?? "Any",
    SelectedOSFilter ?? "Any",
    SelectedConditionFilter?.ToString() ?? "Any",
    SelectedPhoneStateFilter?.ToString() ?? "Any",
    SelectedPhoneStatusFilter?.ToString() ?? "Any",
    SelectedPasscodeTypeFilter?.ToString() ?? "Any"
);
            if (!_dbManager.IsOpen)
            {
                MessageBox.Show("Database not open");
                return;
            }

            Phones.Clear();
            var col = _dbManager.GetDatabase().GetCollection<Phone>("phones");
            var query = col.Query();

            if (!string.IsNullOrEmpty(SelectedBrandFilter))
                query = query.Where(p => p.Brand == SelectedBrandFilter);
            if (!string.IsNullOrEmpty(SelectedModelFilter))
                query = query.Where(p => p.Model == SelectedModelFilter);
            if (!string.IsNullOrEmpty(SelectedOSFilter))
                query = query.Where(p => p.OS == SelectedOSFilter);
            if (SelectedDeviceTypeFilter != null)
                query = query.Where(p => p.DeviceType == SelectedDeviceTypeFilter);
            if (SelectedConditionFilter != null && SelectedConditionFilter != PhoneCondition.None)
                query = query.Where(p => p.Condition == SelectedConditionFilter);
            if (SelectedPhoneStateFilter != null && SelectedPhoneStateFilter != PhoneState.None)
                query = query.Where(p => p.PhoneState == SelectedPhoneStateFilter);
            if (SelectedPhoneStatusFilter != null && SelectedPhoneStatusFilter != PhoneStatus.None)
                query = query.Where(p => p.Status == SelectedPhoneStatusFilter);
            if (SelectedPasscodeTypeFilter != null && SelectedPasscodeTypeFilter != PasscodeType.None)
                query = query.Where(p => p.PasscodeType == SelectedPasscodeTypeFilter);

            var filtered = query.OrderBy(p => p.Id).Limit(PageSize).ToList();

            foreach (var phone in filtered)
                Phones.Add(phone);
            TotalItems = Phones.Count;
            OnPropertyChanged(nameof(Phones));
        }

        public void ApplyInMemorySort()
        {
            var view = CollectionViewSource.GetDefaultView(Phones);
            if (view == null) return;

            view.SortDescriptions.Clear();
            var direction = SelectedSortDirection == "Ascending" ? ListSortDirection.Ascending : ListSortDirection.Descending;
            view.SortDescriptions.Add(new SortDescription(SelectedSortColumn, direction));
            view.Refresh();
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

        partial void OnSelectedSortColumnChanged(string value)
        {
            ApplyInMemorySort();
        }

        // Called when SelectedSortDirection changes
        partial void OnSelectedSortDirectionChanged(string value)
        {
            ApplyInMemorySort();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
