using LiteDB;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PDM.Src.Enums;
using PDM.Src.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDM.Src.ViewModels
{
    internal class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<DashboardViewModel> _logger;
        private readonly DatabaseManager _dbManager;

        private ObservableCollection<ISeries<int>> _brandSeries;
        public ObservableCollection<ISeries<int>> BrandSeries
        {
            get => _brandSeries;
            set { _brandSeries = value; OnPropertyChanged(nameof(BrandSeries)); }
        }

        private ObservableCollection<ISeries> _statusesSeries;
        public ObservableCollection<ISeries> StatusesSeries
        {
            get => _statusesSeries;
            set { _statusesSeries = value; OnPropertyChanged(nameof(StatusesSeries)); }
        }

        private Axis[] _statusesXAxis;
        public Axis[] StatusesXAxis
        {
            get => _statusesXAxis;
            set { _statusesXAxis = value; OnPropertyChanged(nameof(StatusesXAxis)); }
        }
        private int _totalPhones;
        public int TotalPhones
        {
            get => _totalPhones;
            set { _totalPhones = value; OnPropertyChanged(nameof(TotalPhones)); }
        }

        private int _uniqueModels;
        public int UniqueModels
        {
            get => _uniqueModels;
            set { _uniqueModels = value; OnPropertyChanged(nameof(UniqueModels)); }
        }

        private int _phonesNotAnalyzed;
        public int PhonesNotAnalyzed
        {
            get => _phonesNotAnalyzed;
            set { _phonesNotAnalyzed = value; OnPropertyChanged(nameof(PhonesNotAnalyzed)); }
        }

        public IEnumerable<ISeries> Series2 { get; set; } =
        new[]
        {
            new PieSeries<int> { Values = new[]{ 2 } },
            new PieSeries<int> { Values = new[]{ 4 } },
            new PieSeries<int> { Values = new[]{ 1 } },
            new PieSeries<int> { Values = new[]{ 4 } },
            new PieSeries<int> { Values = new[]{ 3 } },
        };

        public IEnumerable<ISeries> Series { get; set; } =
        new[] { 2, 4, 1, 4, 3 }.AsPieSeries();


        public DashboardViewModel()
        {
            _logger = App.ServiceProvider.GetRequiredService<ILogger<DashboardViewModel>>();
            _dbManager = App.ServiceProvider.GetRequiredService<DatabaseManager>();
            LoadData();
        }

        public void LoadData()
        {
            _logger.LogInformation("LoadData Called");
            var db = _dbManager.GetDatabase();
            if (db != null)
            {
                _logger.LogInformation("Database in LoadData not null");
                var phones = db.GetCollection<Phone>("phones").FindAll().ToList();

                var brandGroups = phones.GroupBy(p => p.Brand).Select(g => new { Brand = g.Key, Count = g.Count() });
                BrandSeries = new ObservableCollection<ISeries<int>>(brandGroups.Select(g => new PieSeries<int>
                {
                    Values = new[] { g.Count },
                    Name = g.Brand,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = p => $"{g.Brand} ({g.Count})"
                }));
                _logger.LogInformation($"The amount of brands are {BrandSeries.Count}");

                var statusesGroup = phones.GroupBy(p => p.Status.ToString()).Select(g => new { Status = g.Key, Count = g.Count() });
                StatusesSeries = new ObservableCollection<ISeries>
                {
                    new ColumnSeries<int>
                    {
                        Values = statusesGroup.Select(g => g.Count).ToArray(),
                        Name = "Statuses",
                        Fill = new SolidColorPaint(SKColors.CadetBlue)
                    }
                };

                StatusesXAxis = new[]
                {
                    new Axis
                    {
                        Labels = statusesGroup.Select(g => g.Status).ToArray()
                    }
                };

                TotalPhones = phones.Count;
                UniqueModels = phones.Select(p => p.Model).Distinct().Count();
                PhonesNotAnalyzed = phones.Count(p => p.Status == PhoneStatus.NotAnalyzed);

            }
            
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
