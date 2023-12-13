using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using ValetAccountingMaster.Data;
using ValetAccountingMaster.Model;
using ValetAccountingMaster.View;

namespace ValetAccountingMaster.ViewModel
{
    [QueryProperty(nameof(CurrentDateTime), nameof(CurrentDateTime))]
    [QueryProperty(nameof(Sites), nameof(Sites))]
    [QueryProperty(nameof(SelectedSiteIndex), nameof(SelectedSiteIndex))]
    [QueryProperty(nameof(SelectedSite), nameof(SelectedSite))]
    [QueryProperty(nameof(SqlRecords), nameof(SqlRecords))]
    [QueryProperty(nameof(SqlMonthRecords), nameof(SqlMonthRecords))]
    [QueryProperty(nameof(OperatingSqlRecord), nameof(OperatingSqlRecord))]
    [QueryProperty(nameof(OperatingSqlMonthRecord), nameof(OperatingSqlMonthRecord))]
    [QueryProperty(nameof(CurrentViewMonthRecord), nameof(CurrentViewMonthRecord))]

    public partial class MonthDetailsViewModel : BaseViewModel
    {
        private readonly DatabaseContext context;

        [ObservableProperty]
        ISeries[] series1 = new ISeries[]
        {
            new PieSeries<double> {Values= new double[] {2}},
            new PieSeries<double> {Values= new double[] {4}},
        };
        //[ObservableProperty]
        //IEnumerable<ISeries> series1 = new GaugeBuilder()
        //.WithLabelsSize(20)
        //.WithLabelsPosition(PolarLabelsPosition.Start)
        //.WithLabelFormatter(point => $"{point.PrimaryValue} {point.Context.Series.Name}")
        //.WithInnerRadius(20)
        //.WithOffsetRadius(8)
        //.WithBackgroundInnerRadius(20)
        //.AddValue(20, "Tips")
        //.AddValue(50, "Expenses")
        //.AddValue(80, "Income")
        //.BuildSeries();

        public bool IsAlreadyCreated { get; internal set; }

        [ObservableProperty]
        private int pieTotal;
        [ObservableProperty]
        public ObservableCollection<SiteName> sites = new();
        [ObservableProperty]
        private int selectedSiteIndex;
        [ObservableProperty]
        private SiteName selectedSite = new();

        [ObservableProperty]
        private List<Record> records = new();
        [ObservableProperty]
        private List<SqlRecord> sqlRecords = new();
        [ObservableProperty]
        private List<SqlMonthRecord> sqlMonthRecords = new();

        [ObservableProperty]
        private SqlRecord operatingSqlRecord = new();
        [ObservableProperty]
        private SqlMonthRecord operatingSqlMonthRecord = new();
        [ObservableProperty]
        private SqlMonthRecord currentViewMonthRecord = new();
        [ObservableProperty]
        private double expensesPercent;

        [ObservableProperty]
        private DateTime currentDateTime = new();

        public MonthDetailsViewModel(DatabaseContext databaseContext)
        {
            this.Title = "SINGLE SITE SUMMERY";
            context = databaseContext;
            CurrentDateTime = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, 1);
        }

        

        public async Task UpdateCurrentViewMonthRecord()
        {
            if (!(SqlRecords.Any()))
                return;
            CurrentDateTime = new DateTime(CurrentDateTime.Year, CurrentDateTime.Month, 1);
            if (Sites.Count > 0)
            {
                var id = Sites[SelectedSiteIndex].ID;
                DateTime date = CurrentDateTime;
                CurrentViewMonthRecord = (await context.GetFilteredAsync<SqlMonthRecord>(p => (p.ID == id && p.Date == date))).FirstOrDefault();
            }

            if (CurrentViewMonthRecord == null)
            {
                CurrentViewMonthRecord = new();
            }

            Series1 = new ISeries[]
            {
                new PieSeries<double>
                {
                    Values= new double[] {CurrentViewMonthRecord.DailyExp },
                    Name = "Expenses",
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 15,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point=>  $"Expenses {CurrentViewMonthRecord.DailyExp}",
                    DataLabelsRotation = LiveCharts.TangentAngle,
                    Pushout = 10,
                    Fill = new SolidColorPaint(SKColors.BlueViolet)
                },
                new PieSeries<double>
                {
                    Values= new double[] {CurrentViewMonthRecord.Income},
                    Name = "Income",
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 15,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point=>  $"Income {CurrentViewMonthRecord.Income}",
                    DataLabelsRotation = LiveCharts.TangentAngle,
                    Fill = new SolidColorPaint(SKColors.OrangeRed)
                },
            };

            //Series1 = new GaugeBuilder()
            //.WithLabelsSize(40)
            //.WithLabelsPosition(PolarLabelsPosition.Start)
            //.WithLabelFormatter(point => $"{point.PrimaryValue} {point.Context.Series.Name}")
            //.WithInnerRadius(20)
            //.WithOffsetRadius(8)
            //.WithBackgroundInnerRadius(20)
            //.AddValue(CurrentViewMonthRecord.Tip, "Tips")
            //.AddValue(CurrentViewMonthRecord.DailyExp, "Expenses")
            //.AddValue(CurrentViewMonthRecord.Income, "Income")
            //.BuildSeries();
            PieTotal = (int)Double.Round((CurrentViewMonthRecord.Income+ CurrentViewMonthRecord.DailyExp));

            if(CurrentViewMonthRecord.Income!=0)
            ExpensesPercent = (CurrentViewMonthRecord.DailyExp / CurrentViewMonthRecord.Income) * 100;
        }
        [RelayCommand]
        async Task GoToDayDetails()
        {
            await Shell.Current.GoToAsync(nameof(DayDetailsPage), true, new Dictionary<string, object>
            {
                {"CurrentDateTime",CurrentDateTime },
                {"Sites",Sites },
                {"SelectedSiteIndex",SelectedSiteIndex },
                {"SqlRecords",SqlRecords },
                {"SelectedSite",SelectedSite },
            });
        }
    }
}
