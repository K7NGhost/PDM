using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PDM.Src.Models;
using PDM.Src.ViewModels.PartsVM;
using PDM.ui.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PDM.Src.ViewModels
{
    internal class AdminPageViewModel : ObservableObject
    {
        public ObservableCollection<string> Brands { get; }

        private string _brandInput;
        public string BrandInput
        {
            get => _brandInput;
            set => SetProperty(ref _brandInput, value);
        }

        private string _modelBrandInput;
        public string ModelBrandInput
        {
            get => _modelBrandInput;
            set => SetProperty(ref _modelBrandInput, value);
        }

        private string _modelInput;
        public string ModelInput
        {
            get => _modelInput;
            set => SetProperty(ref _modelInput, value);
        }

        private string _osInput;
        public string OSInput
        {
            get => _osInput;
            set => SetProperty(ref _osInput, value);
        }

        public IRelayCommand SaveCommand { get; }

        public AdminPageViewModel()
        {
            var brandCol = App.ServiceProvider.GetRequiredService<DatabaseManager>().GetDatabase().GetCollection<PhoneBrand>("brands");
            Brands = new ObservableCollection<string>(brandCol.FindAll().Select(b => b.BrandName));
            SaveCommand = new RelayCommand(Save);
        }
        private void Save()
        {
            var db = App.ServiceProvider.GetRequiredService<DatabaseManager>().GetDatabase();
            var addPhoneVm = App.ServiceProvider.GetRequiredService<PhoneDataViewModel>();
            var addPartVm = App.ServiceProvider.GetRequiredService<AddPartsViewModel>();

            // Save brand if not duplicate
            if (!string.IsNullOrWhiteSpace(BrandInput))
            {
                var brandCol = db.GetCollection<PhoneBrand>("brands");
                if (!brandCol.Exists(x => x.BrandName.Equals(BrandInput, StringComparison.OrdinalIgnoreCase)))
                {
                    PhoneBrand newPhoneBrand = new PhoneBrand { BrandName = BrandInput };
                    brandCol.Insert(newPhoneBrand);
                    addPhoneVm.Brands.Add(newPhoneBrand.BrandName);
                    Brands.Add(newPhoneBrand.BrandName);
                }
            }

            // Save model if not duplicate
            if (!string.IsNullOrWhiteSpace(ModelInput))
            {
                var modelCol = db.GetCollection<PhoneModel>("models");
                if (!modelCol.Exists(x => x.ModelName.Equals(ModelInput, StringComparison.OrdinalIgnoreCase)))
                {
                    PhoneModel newPhoneModel = new PhoneModel { Brand = ModelBrandInput, ModelName = ModelInput };
                    modelCol.Insert(newPhoneModel);
                    addPhoneVm.Models.Add(newPhoneModel);
                }
            }

            // Save OS if not duplicate
            if (!string.IsNullOrWhiteSpace(OSInput))
            {
                var osCol = db.GetCollection<PhoneOS>("oses");
                if (!osCol.Exists(x => x.OSName.Equals(OSInput, StringComparison.OrdinalIgnoreCase)))
                {
                    PhoneOS newPhoneOS = new PhoneOS { OSName = OSInput };
                    osCol.Insert(newPhoneOS);
                    addPhoneVm.OSes.Add(newPhoneOS.OSName);
                }
            }
            addPhoneVm.RefreshData();
            addPartVm.RefreshData();

            MessageBox.Show("Entries saved (no duplicates added).");
        }
    }
}
