using CommunityToolkit.Mvvm.ComponentModel;
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
    internal partial class DashboardViewModel : ObservableObject
    {
        private readonly ILogger<DashboardViewModel> _logger;
        private readonly DatabaseManager _dbManager;

        [ObservableProperty]
        private ObservableCollection<ISeries<int>> brandSeries;

        [ObservableProperty]
        private ObservableCollection<ISeries> statusesSeries;


        [ObservableProperty]
        private Axis[] statusesXAxis;

        [ObservableProperty]
        private int totalPhones;

        [ObservableProperty]
        private int uniqueModels;

        [ObservableProperty]
        private int phonesNotAnalyzed;

        [ObservableProperty]
        private int phonesUnlocked;

        [ObservableProperty]
        private int phonesWithSixDPass;

        [ObservableProperty]
        private int phonesWithFourDPass;

        [ObservableProperty]
        private int phonesWithFFSExt;

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

                PhonesUnlocked = phones.Count(p => p.PhoneState ==  PhoneState.Unlocked);
                PhonesWithSixDPass = phones.Count(p => p.PasscodeLength == 6);
                PhonesWithFourDPass = phones.Count(p => p.PasscodeLength == 4);
                PhonesWithFFSExt = phones.Count(p => p.Status == PhoneStatus.FullFileSystem);
            }
            
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
