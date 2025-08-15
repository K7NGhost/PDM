using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PDM.Src.Enums;
using PDM.Src.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PDM.Src.ViewModels.PartsVM
{
    internal partial class AddPartsViewModel : ObservableObject
    {
        private readonly ILogger<AddPartsViewModel> _logger;
        public bool IsEditMode { get; set; }
        public ObservableCollection<string> Brands { get; }
        public ObservableCollection<PhoneModel> Models { get; }
        public ObservableCollection<string> ModelList { get; }
        public ObservableCollection<string> FilteredModels { get; } = new();
        public ObservableCollection<PartType> PartTypes { get; }

        private DatabaseManager _dbManager = App.ServiceProvider.GetRequiredService<DatabaseManager>();

        [ObservableProperty]
        private PartType selectedPartType = PartType.None;

        [ObservableProperty]
        private string? selectedBrand;

        [ObservableProperty]
        public int groupNumber;

        public Dictionary<string, List<string>> BrandModelMap { get; private set; }

        [ObservableProperty]
        private Part selectedPart = new();

        [ObservableProperty]
        private int nextPartId;

        public Action? CloseWindowAction { get; set; }

        public ImageSource? PartImage
        {
            get
            {
                if (SelectedPart?.ImageData == null)
                {
                    return null;
                }

                using var stream = new MemoryStream(SelectedPart.ImageData);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        public AddPartsViewModel()
        {
            _logger = App.ServiceProvider.GetRequiredService<ILogger<AddPartsViewModel>>();
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
                PartTypes = new ObservableCollection<PartType>(Enum.GetValues<PartType>());
                NextPartId = _dbManager.GetNextPartId();
            }
        }

        partial void OnSelectedBrandChanged(string? value)
        {
            _logger.LogInformation("Selected brand changed Updating Models...");
            UpdateModels();
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

        private bool CanSave()
        {
            _logger.LogInformation($"Current in CanSave with value of Brand={SelectedPart.Brand}, Model={SelectedPart.Model}, PartType={SelectedPart.PartType}");
            return Enum.IsDefined(typeof(PartType), SelectedPartType) && SelectedPartType != PartType.None;
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                if (IsEditMode == true)
                {
                    _dbManager.UpdatePart(SelectedPart);
                    MessageBox.Show("Part Updated Successfully!");
                    CloseWindowAction?.Invoke();
                }
                else
                {
                    {
                        if (CanSave())
                        {
                            SelectedPart.Id = NextPartId;
                            SelectedPart.GroupId = GroupNumber;
                            SelectedPart.Brand = SelectedBrand;
                            SelectedPart.PartType = SelectedPartType;
                            _dbManager.SavePart(SelectedPart);
                            MessageBox.Show($"Part Saved Successfully, with id: {NextPartId}, and group id: {GroupNumber}");

                            SelectedPart = new Part();
                            NextPartId = _dbManager.GetNextPartId();
                        }
                        else
                        {
                            MessageBox.Show($"Ensure you have at least picked a part type!");
                        }
                    }
                }
                RefreshData();
                App.ServiceProvider.GetRequiredService<PartsListViewModel>().ReloadParts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save part: {ex.Message}");
            }
        }

        public void RefreshData()
        {
            var db = _dbManager.GetDatabase();
            if (db == null) return;

            var brandCol = db.GetCollection<PhoneBrand>("brands");
            Brands.Clear();
            foreach (var brand in brandCol.FindAll().Select(b => b.BrandName))
                Brands.Add(brand);

            var modelCol = db.GetCollection<PhoneModel>("models");
            Models.Clear();
            foreach (var model in modelCol.FindAll())
                Models.Add(model);

            BrandModelMap = Brands.ToDictionary(
                b => b,
                b => Models.Where(m => m.Brand.Contains(b, StringComparison.OrdinalIgnoreCase))
                           .Select(m => m.ModelName)
                           .ToList()
            );

            UpdateModels();
        }

        [RelayCommand]
        private void Cancel()
        {
            RefreshData();
            SelectedPart = new Part();
            OnPropertyChanged(nameof(SelectedPart));
            OnPropertyChanged(nameof(PartImage));
        }

        [RelayCommand]
        public void UploadImage()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedPart.ImageData = File.ReadAllBytes(openFileDialog.FileName);
                OnPropertyChanged(nameof(PartImage));
            }
        }

        [RelayCommand]
        private void RemoveImage()
        {
            if (SelectedPart == null) return;
            SelectedPart.ImageData = null;
            OnPropertyChanged(nameof(PartImage));
        }

        partial void OnSelectedPartTypeChanged(PartType value)
        {
            GroupNumber = (int)value;
        }

    }
}
