using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PDM.Src.Enums;
using PDM.Src.Models;
using PDM.ui.Windows;
using PDM.ui.Windows.PartsWindows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PDM.Src.ViewModels.PartsVM
{
    partial class PartsListViewModel : ObservableObject
    {
        private readonly ILogger<PartsListViewModel> _logger;
        private readonly DatabaseManager _dbManager;

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int totalPages = 1;

        private const int PageSize = 300;
        private Stack<int> _prevPageAnchors = new();

        public string? SelectedBrandFilter { get; set; }
        public string? SelectedModelFilter { get; set; }
        public PartType? SelectedPartTypeFilter { get; set; }

        public ObservableCollection<Part> Parts { get; } = new();

        [ObservableProperty]
        private int totalItems;

        public ObservableCollection<string> SortColumns { get; } = new()
        {
            nameof(Part.Id), nameof(Part.GroupId), nameof(Part.Brand), nameof(Part.Model), nameof(Part.PartType)
        };

        public ObservableCollection<string> SortDirection { get; } = new()
        {
            "Ascending", "Descending"
        };

        [ObservableProperty]
        private string selectedSortColumn = nameof(Part.Id);

        [ObservableProperty]
        private string selectedSortDirection = "Ascending";

        public PartsListViewModel()
        {
            _logger = App.ServiceProvider.GetRequiredService<ILogger<PartsListViewModel>>();
            _dbManager = App.ServiceProvider.GetRequiredService<DatabaseManager>();
            LoadPageByAnchor(null);
        }

        [RelayCommand]
        private void DeletePart(Part part)
        {
            if (part == null)
            {
                return;
            }
            var result = MessageBox.Show($"Are you sure you want to delete {part.Brand} {part.Model} {part.PartType}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                var db = App.ServiceProvider.GetRequiredService<DatabaseManager>();
                db.DeletePart(part.Id);
                Parts.Remove(part);

            }
        }

        private void LoadPageByAnchor(int? afterId = null)
        {
            if (!_dbManager.IsOpen)
            {
                MessageBox.Show("Database not open");
                return;
            }

            var col = _dbManager.GetDatabase().GetCollection<Part>("parts");

            if (SelectedSortColumn == nameof(Part.Id))
                col.EnsureIndex(x => x.Id);
            else
                col.EnsureIndex(SelectedSortColumn);

            TotalPages = (int)Math.Ceiling(col.Count() / (double)PageSize);
            var query = col.Query();

            if (SelectedSortColumn == nameof(Part.Id))
            {
                if (afterId is int anchor)
                {
                    _logger.LogInformation($"afterId anchor = {anchor}");
                    query = SelectedSortDirection == "Ascending" ? query.Where(x => x.Id > anchor) : query.Where(x => x.Id < anchor);
                }
                query = SelectedSortDirection == "Ascending" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id);
            }
            else
            {
                if (afterId != null)
                {
                    _logger.LogWarning($"Ignoring afterId because sorting by {SelectedSortColumn}. Implement per-column anchors to keyset paginate.");
                }
                query = SelectedSortDirection == "Ascending" ? query.OrderBy(SelectedSortColumn) : query.OrderByDescending(SelectedSortColumn);
            }

            var pageItems = query.Limit(PageSize).ToList();
            _logger.LogInformation($"Fetched {pageItems.Count} rows for page");
            Parts.Clear();
            foreach (var p in pageItems)
            {
                Parts.Add(p);
            }

            if (Parts.Count > 0)
            {
                var anchorToPush = SelectedSortDirection == "Ascending" ? Parts[^1].Id : Parts[0].Id;
                _prevPageAnchors.Push(anchorToPush);
            }
            else
            {
                _logger.LogWarning("No rows returned for this page.");
            }

            OnPropertyChanged(nameof(Parts));
            TotalItems = Parts.Count;
            _logger.LogInformation($"TotalItems is now {TotalItems} items");
        }

        [RelayCommand]
        private void NextPage()
        {
            CurrentPage++;
            _logger.LogInformation($"CurrentPage is now {CurrentPage}");
            var afterId = Parts.Last().Id;
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

        public void ReloadParts()
        {
            CurrentPage = 1;
            LoadPageByAnchor(null);
        }

        public void ApplyInMemorySort()
        {
            var view = CollectionViewSource.GetDefaultView(Parts);
            if (view == null) return;
            view.SortDescriptions.Clear();
            var direction = SelectedSortDirection == "Ascending" ? ListSortDirection.Ascending : ListSortDirection.Descending;
            view.SortDescriptions.Add(new SortDescription(SelectedSortColumn, direction));
            view.Refresh();
        }

        [RelayCommand]
        private void OpenFilterWindow()
        {
            var vm = new AddPartsViewModel();
            var partsFilterWindow = new PartsFilterWindow { DataContext = vm };
            if (partsFilterWindow.ShowDialog() == true)
            {
                SelectedBrandFilter = vm.SelectedBrand;
                SelectedModelFilter = vm.SelectedPart.Model;
                SelectedPartTypeFilter = vm.SelectedPartType;
                ApplyFilters();
                _logger.LogInformation("Filters applied");

            }
            
        }

        private void ApplyFilters()
        {
            if (!_dbManager.IsOpen)
            {
                MessageBox.Show("Database is not open");
                return;
            }

            Parts.Clear();
            var col = _dbManager.GetDatabase().GetCollection<Part>("parts");
            var query = col.Query();
            if (!string.IsNullOrEmpty(SelectedBrandFilter))
                query = query.Where(p => p.Brand == SelectedBrandFilter);
            if (!string.IsNullOrEmpty(SelectedModelFilter)) 
                query = query.Where(p => p.Model == SelectedModelFilter);
            if (SelectedPartTypeFilter != null && SelectedPartTypeFilter != PartType.None)
                query = query.Where(p => p.PartType == SelectedPartTypeFilter);

            var filtered = query.OrderBy(p => p.Id).Limit(PageSize).ToList();

            foreach (var part in filtered)
            {
                Parts.Add(part);
            }
            TotalItems = Parts.Count;
            OnPropertyChanged(nameof(Parts));
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedBrandFilter = null;
            SelectedModelFilter = null;
            SelectedPartTypeFilter = null;
            ReloadParts();
        }

        [RelayCommand]
        private void OpenEditWindow(Part part)
        {
            _logger.LogInformation($"The provided part object contains Id={part.Id}, GroupId={part.GroupId}, Brand={part.Brand}, Model={part.Model}, parttype={part.PartType}, notes={part.Notes}");
            var vm = new AddPartsViewModel
            {
                IsEditMode = true,
                SelectedPart = new Part
                {
                    Id = part.Id,
                    GroupId = part.GroupId,
                    Brand = part.Brand,
                    Model = part.Model,
                    PartType = part.PartType,
                    Notes = part.Notes,
                    ImageData = part.ImageData,
                }
            };
            var window = new EditPartWindow { DataContext = vm };
            vm.CloseWindowAction = () => window.Close();
            window.Show();
        }

        partial void OnSelectedSortColumnChanged(string value)
        {
            ApplyInMemorySort();
        }

        partial void OnSelectedSortDirectionChanged(string value)
        {
            ApplyInMemorySort();
        }

    }
}
