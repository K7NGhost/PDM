using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly DatabaseManager _dbManager;
        private int _currentPage = 1;
        private const int PageSize = 200;

        public ObservableCollection<Phone> Phones { get; } = new();
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand EditPhoneCommand { get; }
        public ICommand OpenFilterCommand { get; }

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
            _dbManager = App.ServiceProvider.GetRequiredService<DatabaseManager>();
            NextPageCommand = new RelayCommand(NextPage, CanNextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage, () => _currentPage > 1);
            EditPhoneCommand = new RelayCommand<Phone>(OpenEditWindow);
            OpenFilterCommand = new RelayCommand(() =>
            {
                var filterWindow = new FilterWindow();
                if (filterWindow.ShowDialog() == true)
                {
                    Console.WriteLine("logged");
                }
            });
            // With the following corrected line:
            LoadPage();
        }

        private void OpenEditWindow(Phone phone)
        {
            var vm = new PhoneDataViewModel
            {
                IsEditMode = true,
                SelectedPhone = new Phone
                {
                    Id = phone.Id,
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
                MessageBox.Show("dbmanager is not open");
                return;
            }

            Phones.Clear();
            var col = _dbManager.GetDatabase().GetCollection<Phone>("phones");
            var pageItems = col.Query().OrderBy(x => x.Id).Skip((_currentPage - 1) * PageSize).Limit(PageSize).ToList();
            foreach (var phone in pageItems)
            {
                Phones.Add(phone);
            }
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

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
