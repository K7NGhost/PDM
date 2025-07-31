using CommunityToolkit.Mvvm.Input;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PDM.Src.Enums;
using PDM.Src.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;

namespace PDM.Src.ViewModels
{
    internal class PhoneDataViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<PhoneDataViewModel> _logger;
        public bool IsEditMode { get; set; }
        public ObservableCollection<string> Brands { get; }
        public ObservableCollection<PhoneModel> Models { get; }
        public ObservableCollection<string> ModelList { get; }
        public ObservableCollection<string> OSes { get; }
        public ObservableCollection<PhoneState> PhoneStates { get; }
        public ObservableCollection<PasscodeType> PasscodeTypes { get; }
        public ObservableCollection<PhoneCondition> PhoneConditions { get; }
        public ObservableCollection<PhoneStatus> PhoneStatuses { get; }
        public ObservableCollection<string> FilteredModels { get; } = new();
        public ObservableCollection<string> FilteredOses { get; } = new();
        private DatabaseManager _dbManager = App.ServiceProvider.GetRequiredService<DatabaseManager>();
        public int GroupNumber => GetOrCreateGroupNumber(SelectedModel);
     
        public Dictionary<string, List<string>> BrandModelMap { get; private set; }

        private Phone _selectedPhone = new();
        public Phone SelectedPhone
        {
            get => _selectedPhone;
            set
            {
                _selectedPhone = value;
                OnPropertyChanged(nameof(SelectedPhone));
                if (!string.IsNullOrEmpty(_selectedPhone.Brand))
                {
                    SelectedBrand = Brands.FirstOrDefault(b => string.Equals(b, _selectedPhone.Brand, StringComparison.OrdinalIgnoreCase));
                }

            }
        }

        // Know when a brand is selected
        private string? _selectedBrand;
        public string? SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                if (_selectedBrand != value)
                {
                    _selectedBrand = value;
                    SelectedPhone.Brand = _selectedBrand ?? string.Empty;
                    OnPropertyChanged(nameof(SelectedBrand));
                    UpdateModels();
                    UpdateOSes();
                }
            }
        }

        private string _selectedModel;
        public string SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                if (_selectedModel != null)
                    SelectedPhone.Model = _selectedModel ?? string.Empty;
                else
                    SelectedPhone.Model = null;
                OnPropertyChanged(nameof(SelectedModel));
                OnPropertyChanged(nameof(GroupNumber));
            }
        }

        private int _nextPhoneId;
        public int NextPhoneId
        {
            get => _nextPhoneId;
            set
            {
                if (_nextPhoneId != value)
                {
                    _nextPhoneId = value;
                    OnPropertyChanged(nameof(NextPhoneId));
                }
            }
        }

        public ImageSource? PhoneImage
        {
            get
            {
                if (SelectedPhone?.ImageData == null)
                {
                    return null;
                }

                using var stream = new MemoryStream(SelectedPhone.ImageData);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand UploadImageCommand { get; }

        public PhoneDataViewModel()
        {
            _logger = App.ServiceProvider.GetRequiredService<ILogger<PhoneDataViewModel>>();
            var db = _dbManager.GetDatabase();
            if (db != null)
            {
                var brandCol = db.GetCollection<PhoneBrand>("brands");
                Brands = new ObservableCollection<string>(brandCol.FindAll().Select(b => b.BrandName));

                var modelCol = db.GetCollection<PhoneModel>("models");
                Models = new ObservableCollection<PhoneModel>(modelCol.FindAll().ToList());
                ModelList = new ObservableCollection<string>(Models.Select(m => m.ModelName));
                BrandModelMap = Brands.ToDictionary(
                                    b => b,
                                    b => Models.Where(m => m.Brand.Contains(b, StringComparison.OrdinalIgnoreCase)).Select(m => m.ModelName).ToList()
                                    );

                var osCol = db.GetCollection<PhoneOS>("oses");
                OSes = new ObservableCollection<string>(osCol.FindAll().Select(o => o.OSName));
                PhoneStates = new ObservableCollection<PhoneState>(Enum.GetValues<PhoneState>());
                PasscodeTypes = new ObservableCollection<PasscodeType>(Enum.GetValues<PasscodeType>());
                PhoneConditions = new ObservableCollection<PhoneCondition>(Enum.GetValues<PhoneCondition>());
                PhoneStatuses = new ObservableCollection<PhoneStatus>(Enum.GetValues<PhoneStatus>());
                _nextPhoneId = _dbManager.GetNextPhoneId();

                SaveCommand = new RelayCommand(Save, CanSave);
                CancelCommand = new RelayCommand(Cancel);
                UploadImageCommand = new RelayCommand(UploadImage);
            }
            
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SelectedPhone.Brand) || string.IsNullOrWhiteSpace(SelectedPhone.Model))
                {
                    MessageBox.Show("Brand and Model cannot be empty.");
                    return;
                }
                if (IsEditMode == true)
                {
                    _dbManager.UpdatePhone(SelectedPhone);
                    MessageBox.Show("Phone Updated Successfully!");
                }
                else
                {
                    SelectedPhone.Id = NextPhoneId;
                    SelectedPhone.GroupId = GroupNumber;
                    _dbManager.SavePhone(SelectedPhone);
                    MessageBox.Show($"Phone Saved Successfully, with id: {NextPhoneId}, and group id: {GroupNumber}");

                    SelectedPhone = new Phone();
                    OnPropertyChanged(nameof(SelectedPhone));

                    NextPhoneId = _dbManager.GetNextPhoneId();
                    OnPropertyChanged(nameof(NextPhoneId));
                }
                App.ServiceProvider.GetRequiredService<DashboardViewModel>().LoadData();
                RefreshData();
                App.ServiceProvider.GetRequiredService<PhoneListViewModel>().ReloadPhones();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save phone: {ex.Message}");
            }
        }

        private bool CanSave()
        {
            _logger.LogInformation($"Current in CanSave with value of Brand={SelectedPhone.Brand}, Model={SelectedPhone.Model}");
            return !string.IsNullOrWhiteSpace(SelectedPhone.Brand) && !string.IsNullOrWhiteSpace(SelectedPhone.Model);
        }

        private void Cancel()
        {
            RefreshData();
            SelectedPhone = new Phone();
            OnPropertyChanged(nameof(SelectedPhone));
            OnPropertyChanged(nameof(PhoneImage));
        }

        private void UpdateModels()
        {
            FilteredModels.Clear();
            if (SelectedBrand != null && !string.IsNullOrEmpty(SelectedBrand) &&
                BrandModelMap.TryGetValue(SelectedBrand, out var models))
            {
                foreach (var model in models.AsEnumerable().Reverse())
                    FilteredModels.Add(model);
            }
        }


        private void UpdateOSes()
        {
            FilteredOses.Clear();

            if (SelectedBrand != null && !string.IsNullOrWhiteSpace(SelectedBrand))
            {
                var osList = string.Equals(SelectedBrand, "Apple", StringComparison.OrdinalIgnoreCase)
                    ? OSes.Where(o => !string.IsNullOrWhiteSpace(o) &&
                                      o.Contains("iOS", StringComparison.OrdinalIgnoreCase))
                    : OSes.Where(o => !string.IsNullOrWhiteSpace(o) &&
                                      o.Contains("Android", StringComparison.OrdinalIgnoreCase));

                foreach (var os in osList.AsEnumerable().Reverse())
                    FilteredOses.Add(os);
            }
        }

        private void UploadImage()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedPhone.ImageData = File.ReadAllBytes(openFileDialog.FileName);
                OnPropertyChanged(nameof(PhoneImage));
            }
        }

        public int GetOrCreateGroupNumber(string phoneModel)
        {
            if (phoneModel == null || string.IsNullOrWhiteSpace(phoneModel))
            {
                return 0;
            }
            _logger.LogInformation($"In getorcreategroupnumber with phone model {phoneModel}");
            var phoneCollection = App.ServiceProvider.GetRequiredService<DatabaseManager>().GetDatabase().GetCollection<Phone>("phones");
            var existingModel = phoneCollection.FindOne(x => x.Model == phoneModel);
            
            if (existingModel != null && existingModel.GroupId != null)
            {
                _logger.LogInformation($"The existing model is {existingModel.Brand} with the groupid of {existingModel.GroupId}");
                _logger.LogInformation("Existing model does not equal null returning the groupID");
                return existingModel.GroupId;
            }
            _logger.LogInformation("No existing model ... Creating new groupID");
            // If there is no existing model create new groupId
            int newGroupNumber = 1;
            var lastGroup = phoneCollection.Query().OrderByDescending(x => x.GroupId).FirstOrDefault();
            if (lastGroup != null)
            {
                newGroupNumber = lastGroup.GroupId + 1;
            }
            Console.WriteLine($"The groupnumber is {newGroupNumber}");
            return newGroupNumber;
        }

        public void RefreshData()
        {
            var db = _dbManager.GetDatabase();
            if (db == null) return;


            // Refresh Brands
            var brandCol = db.GetCollection<PhoneBrand>("brands");
            Brands.Clear();
            foreach (var brand in brandCol.FindAll().Select(b => b.BrandName))
                Brands.Add(brand);

            // Refresh Models
            var modelCol = db.GetCollection<PhoneModel>("models");
            Models.Clear();
            foreach (var model in modelCol.FindAll())
                Models.Add(model);

            // Refresh OSes
            var osCol = db.GetCollection<PhoneOS>("oses");
            OSes.Clear();
            foreach (var os in osCol.FindAll())
                OSes.Add(os.OSName);

            // Rebuild brand -> model mapping
            BrandModelMap = Brands.ToDictionary(
                b => b,
                b => Models.Where(m => m.Brand.Contains(b, StringComparison.OrdinalIgnoreCase))
                           .Select(m => m.ModelName)
                           .ToList()
            );

            UpdateModels();
            UpdateOSes();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            if (name == nameof(SelectedPhone) || name == nameof(SelectedModel) || name == nameof(SelectedBrand))
            {
                (SaveCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }
            
    }
}
