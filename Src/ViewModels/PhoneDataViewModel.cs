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

namespace PDM.Src.ViewModels
{
    internal class PhoneDataViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<PhoneDataViewModel> _logger;
        public bool IsEditMode { get; set; }
        public ObservableCollection<PhoneBrand> Brands { get; }
        public ObservableCollection<PhoneModel> Models { get; }
        public ObservableCollection<PhoneOS> OSes { get; }
        public ObservableCollection<PhoneState> PhoneStates { get; }
        public ObservableCollection<PasscodeType> PasscodeTypes { get; }
        public ObservableCollection<PhoneCondition> PhoneConditions { get; }
        public ObservableCollection<PhoneStatus> PhoneStatuses { get; }
        public ObservableCollection<PhoneModel> FilteredModels { get; } = new();
        public ObservableCollection<PhoneOS> FilteredOses { get; } = new();
        private DatabaseManager _dbManager = App.ServiceProvider.GetRequiredService<DatabaseManager>();
        public int GroupNumber => GetOrCreateGroupNumber(SelectedModel);
     
        private static readonly Dictionary<PhoneBrand, List<PhoneModel>> BrandModelMap = new()
        {
            { PhoneBrand.Apple, GetIPhoneModels() },
            {PhoneBrand.Samsung,  GetSamsungModels() }
        };

        private static readonly List<PhoneOS> AppleOSes = GetAppleOses();

        private static readonly List<PhoneOS> AndroidOSes = GetAndroidOses();

        private Phone _selectedPhone = new();
        public Phone SelectedPhone
        {
            get => _selectedPhone;
            set
            {
                _selectedPhone = value;
                OnPropertyChanged(nameof(SelectedPhone));
                SelectedBrand = _selectedPhone.Brand;

            }
        }

        // Know when a brand is selected
        private PhoneBrand? _selectedBrand;
        public PhoneBrand? SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                if (_selectedBrand != value)
                {
                    _selectedBrand = value;
                    SelectedPhone.Brand = (PhoneBrand)_selectedBrand;
                    OnPropertyChanged(nameof(SelectedBrand));
                    UpdateModels();
                    UpdateOSes();
                }
            }
        }

        private PhoneModel _selectedModel;
        public PhoneModel SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                SelectedPhone.Model = _selectedModel;
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
            Brands = new ObservableCollection<PhoneBrand>(Enum.GetValues<PhoneBrand>());
            Models = new ObservableCollection<PhoneModel>(Enum.GetValues<PhoneModel>());
            OSes = new ObservableCollection<PhoneOS>(Enum.GetValues<PhoneOS>());
            PhoneStates = new ObservableCollection<PhoneState>(Enum.GetValues<PhoneState>());
            PasscodeTypes = new ObservableCollection<PasscodeType>(Enum.GetValues<PasscodeType>());
            PhoneConditions = new ObservableCollection<PhoneCondition>(Enum.GetValues<PhoneCondition>());
            PhoneStatuses = new ObservableCollection<PhoneStatus>(Enum.GetValues<PhoneStatus>());
            _nextPhoneId = _dbManager.GetNextPhoneId();

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            UploadImageCommand = new RelayCommand(UploadImage);
        }

        private void Save()
        {
            try
            {
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
                    
                App.ServiceProvider.GetRequiredService<PhoneListViewModel>().ReloadPhones();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save phone: {ex.Message}");
            }
        }

        private void Cancel()
        {
            // Cancel logic here
        }

        private void UpdateModels()
        {
            FilteredModels.Clear();
            if (BrandModelMap.TryGetValue((PhoneBrand)SelectedBrand, out var models))
            {
                foreach (var model in models)
                {
                    FilteredModels.Add(model);
                }
            }
        }

        private void UpdateOSes()
        {
            FilteredOses.Clear();
            var osList = _selectedBrand == PhoneBrand.Apple ? AppleOSes : AndroidOSes;
            foreach (var os in osList)
            {
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
                OnPropertyChanged(nameof(SelectedPhone));
                OnPropertyChanged(nameof(PhoneImage));
            }
        }

        public int GetOrCreateGroupNumber(PhoneModel phoneModel)
        {
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

        private static List<PhoneModel> GetIPhoneModels()
        {
            var iPhoneList = Enum.GetValues(typeof(PhoneModel)).Cast<PhoneModel>().Where(m => m.ToString().Contains("iPhone", StringComparison.OrdinalIgnoreCase)).ToList();
            iPhoneList.Reverse();
            return iPhoneList;
        }

        private static List<PhoneModel> GetSamsungModels()
        {
            var samsungList = Enum.GetValues(typeof(PhoneModel)).Cast<PhoneModel>().Where(m => m.ToString().Contains("Galaxy", StringComparison.OrdinalIgnoreCase) || m.ToString().Contains("Note", StringComparison.OrdinalIgnoreCase)).ToList();
            samsungList.Reverse();
            return samsungList;
        }

        private static List<PhoneOS> GetAppleOses()
        {
            var osList = Enum.GetValues(typeof(PhoneOS)).Cast<PhoneOS>().Where(x => x == PhoneOS.None || x.ToString().Contains("iOS", StringComparison.OrdinalIgnoreCase)).ToList();
            osList.Reverse();
            return osList;
        }

        private static List<PhoneOS> GetAndroidOses()
        {
            var osList = Enum.GetValues(typeof(PhoneOS)).Cast<PhoneOS>().Where(x => x == PhoneOS.None || x.ToString().Contains("Android", StringComparison.OrdinalIgnoreCase)).ToList();
            osList.Reverse();
            return osList;
        }



        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
