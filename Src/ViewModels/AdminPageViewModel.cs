using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PDM.Src.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PDM.Src.ViewModels
{
    internal class AdminPageViewModel : ObservableObject
    {
        private string _brandInput;
        public string BrandInput
        {
            get => _brandInput;
            set => SetProperty(ref _brandInput, value);
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
            SaveCommand = new RelayCommand(Save);
        }
        private void Save()
        {
            var db = App.ServiceProvider.GetRequiredService<DatabaseManager>().GetDatabase();

            // Save brand if not duplicate
            if (!string.IsNullOrWhiteSpace(BrandInput))
            {
                var brandCol = db.GetCollection<PhoneBrand>("brands");
                if (!brandCol.Exists(x => x.BrandName.Equals(BrandInput, StringComparison.OrdinalIgnoreCase)))
                {
                    brandCol.Insert(new PhoneBrand { BrandName = BrandInput });
                }
            }

            // Save model if not duplicate
            if (!string.IsNullOrWhiteSpace(ModelInput))
            {
                var modelCol = db.GetCollection<PhoneModel>("models");
                if (!modelCol.Exists(x => x.ModelName.Equals(ModelInput, StringComparison.OrdinalIgnoreCase)))
                {
                    modelCol.Insert(new PhoneModel { ModelName = ModelInput });
                }
            }

            // Save OS if not duplicate
            if (!string.IsNullOrWhiteSpace(OSInput))
            {
                var osCol = db.GetCollection<PhoneOS>("oses");
                if (!osCol.Exists(x => x.OSName.Equals(OSInput, StringComparison.OrdinalIgnoreCase)))
                {
                    osCol.Insert(new PhoneOS { OSName = OSInput });
                }
            }

            MessageBox.Show("Entries saved (no duplicates added).");
        }
    }
}
