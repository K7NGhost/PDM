using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PDM.Src.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PDM.Src.ViewModels
{
    internal class DatabaseManagerViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseManager _dbManager = App.ServiceProvider.GetRequiredService<DatabaseManager>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand CreateNewCommand { get; }
        public ICommand OpenCommand { get; }

        public string? Path => _dbManager.DatabasePath;

        public DatabaseManagerViewModel()
        {
            CreateNewCommand = new RelayCommand(CreateNew, () => true);
            OpenCommand = new RelayCommand(Open, () => true);
        }

        private void CreateNew()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "LiteDB (*.db)|*.db",
                Title = "Create New Database"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _dbManager.CreateNew(dlg.FileName);
                    OnPropertyChanged(nameof(Path));
                    MessageBox.Show("New database created successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create DB: {ex.Message}");
                }
            }
        }

        private void Open()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "LiteDB (*.db)|*.db",
                Title = "Open Existing Database"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _dbManager.Open(dlg.FileName);
                    OnPropertyChanged(nameof(Path));
                    MessageBox.Show("Database opened.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open DB: {ex.Message}");
                }
            }
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private RelayCommand openAdminPanelCommand;
        public ICommand OpenAdminPanelCommand => openAdminPanelCommand ??= new RelayCommand(OpenAdminPanel);

        private void OpenAdminPanel()
        {
        }
    }
}
