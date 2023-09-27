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
using ValetAccountingMaster.Services;
using ValetAccountingMaster.View;

namespace ValetAccountingMaster.ViewModel
{
    public partial class RecordsViewModel : BaseViewModel
    {
        IConnectivity connectivity;
        FireBaseService fireBaseService;
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
        private ObservableCollection<SiteName> sites = new();
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
        private double allSitesIncome;
        [ObservableProperty]
        private double allSitesExpenses;
        [ObservableProperty]
        private double allSitesNet;
        [ObservableProperty]
        private double allSitesWorkers;
        [ObservableProperty]
        private double allSitesTips;
        [ObservableProperty]
        private double allExpensesPercent;

        [ObservableProperty]
        private DateTime currentDateTime = new();
        [ObservableProperty]
        private bool isBusy;
        [ObservableProperty]
        private string busyText;

        public RecordsViewModel(DatabaseContext databaseContext, IConnectivity connectivity)
        {
            //TODO:1- add number of employees
            //TODO:2- prohibit adding more than one record for the same day
            //TODO:3- make all days show on the day view remove charts
            //TODO:6- dateTime picker to only show months
            //TODO:9- Ex/in for days *************************************************************************
            //TODO:10- pie charts .. 2 colors ,, writing inside -- remove tips *******************************
            //TODO:5- apple apk*******************************************************************************
            //TODO:11- colors styles and themes 
            //TODO:12- tabs instead of buttons

            //dotnet publish -c Release -f:net7.0-android

            //File.Delete(Path.Combine(FileSystem.AppDataDirectory, "MyDatabase.db3"));
            //dotnet publish -f net7.0-ios -c Release -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="Apple Distribution: John Smith (AY2GDE9QM7)" -p:CodesignProvision="MyMauiApp"

            Title = "TOTAL MONTH SUMMERY";
            fireBaseService = new FireBaseService(Records);
            context = databaseContext;
            this.connectivity = connectivity;
            CurrentDateTime = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, 1);
        }

        public async Task GetFirebaseRecordAsync()
        {
            await CheckConnectivity();

            await fireBaseService.GetRecords();

        }

        public async Task GetSqlAllRecordsAsync()
        {
            await ExcuteAsync(async () =>
            {
                var sqlRecords = await context.GetAllAsync<SqlRecord>();
                if (sqlRecords is not null && sqlRecords.Any())
                {
                    SqlRecords ??= new List<SqlRecord>();
                    foreach (var sqlRecord in sqlRecords)
                    {
                        SqlRecords.Add(sqlRecord);
                    }
                }
            }, "Fetching Records");

            await ExcuteAsync(async () =>
            {
                var sqlMonthRecords = await context.GetAllAsync<SqlMonthRecord>();
                if (sqlMonthRecords is not null && sqlMonthRecords.Any())
                {
                    sqlMonthRecords ??= new List<SqlMonthRecord>();
                    foreach (var sqlMonthRecord in sqlMonthRecords)
                    {
                        SqlMonthRecords.Add(sqlMonthRecord);
                    }
                }
            }, "Fetching MonthRecords");

            await ExcuteAsync(async () =>
            {
                var sites = await context.GetAllAsync<SiteName>();
                if (sites is not null && sites.Any())
                {
                    sites ??= new List<SiteName>();
                    foreach (var site in sites)
                    {
                        Sites.Add(site);
                    }
                }
            }, "Fetching MonthRecords");
            if (Sites is not null && Sites.Any())
            {
                SelectedSite = Sites.FirstOrDefault();
            }
        }

        public async Task MatchRecords()
        {
            await CheckConnectivity();
            var tempRecs = Records.OrderBy(o => o.Date).ToList();
            foreach (var record in tempRecs)
            {
                SqlRecord sqlRec = new();
                sqlRec.Workers = record.Workers;
                sqlRec.DailyExp = record.DailyExp;
                sqlRec.DailyNet = record.DailyNet;
                sqlRec.Tip = record.Tip;
                sqlRec.ID = record.ID;
                sqlRec.Date = record.Date;
                sqlRec.Income = record.Income;
                SetOperatingSqlRecord(sqlRec);
                CheckSite(sqlRec);
                await SaveRecordAsync();
                await DeleteFirebaseRecordAsync(record);
                Records.Remove(record);
                await AddToMonthRecords();
            }
        }

        private async void CheckSite(SqlRecord sqlRec)
        {
            if ((await context.GetFilteredAsync<SiteName>(p => p.ID == sqlRec.ID)).FirstOrDefault() is null)
            {
                var newSite = new SiteName { ID = sqlRec.ID };
                await context.AddItemAsync<SiteName>(newSite);
                Sites.Add(newSite);
            }
        }
        [RelayCommand]
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

            //var tempMonthRecords = SqlMonthRecords.Where(o=>o.Date == CurrentDateTime)

            var x = SqlMonthRecords.Where(
                        p =>
                        p.Date.Year == CurrentDateTime.Date.Year &&
                        p.Date.Month == CurrentDateTime.Month &&
                        p.Date.Day == 1).ToList();

            AllSitesIncome = 0;
            AllSitesExpenses = 0;
            AllSitesTips = 0;
            AllSitesWorkers = 0;
            AllSitesNet = 0;
            foreach (var item in x)
            {
                AllSitesIncome += item.Income;
                AllSitesExpenses += item.DailyExp;
                AllSitesTips += item.Tip;
                AllSitesWorkers += item.Workers;
                AllSitesNet += item.DailyNet;
            }
            if(AllSitesIncome!=0)
            AllExpensesPercent = (AllSitesExpenses / AllSitesIncome)*100;
            Series1 = new ISeries[]
            {
                new PieSeries<double>
                {
                    Values= new double[] {AllSitesExpenses },
                    Name = "Expenses",
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 15,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point=>  $"Expenses {AllSitesExpenses}",
                    DataLabelsRotation = LiveCharts.TangentAngle,
                    Pushout = 10,
                    Fill = new SolidColorPaint(SKColors.BlueViolet)
                },
                new PieSeries<double>
                {
                    Values= new double[] {AllSitesIncome},
                    Name = "Income",
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 15,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point=>  $"Income {AllSitesIncome}",
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
            //.AddValue(AllSitesTips, "Tips")
            //.AddValue(AllSitesExpenses, "Expenses")
            //.AddValue(AllSitesIncome, "Income")
            //.BuildSeries();

            PieTotal = (int)Double.Round(AllSitesIncome * 1.25);
        }
        public async Task AddToMonthRecords()
        {
            await CheckConnectivity();

            await SaveToMonthRecordAsync();
        }
        [RelayCommand]

        public async Task DeleteFirebaseRecordAsync(Record rec)
        {
            await CheckConnectivity();

            await fireBaseService.DeleteRecord(rec);
        }


        [RelayCommand]
        private async Task SaveRecordAsync()
        {
            if (OperatingSqlRecord is null)
                return;

            var (isValid, errorMsg) = OperatingSqlRecord.Validate();
            if (!isValid)
            {
                await Shell.Current.DisplayAlert("Validation Error", errorMsg, "Ok");
                return;
            }
            var busyText = OperatingSqlRecord.Key == 0 ? "Creating record..." : "Updating Record...";
            await ExcuteAsync(async () =>
            {
                if (OperatingSqlRecord.Key == 0)
                {
                    //Create
                    if ((await context.GetFilteredAsync<SqlRecord>(p => p.Date == OperatingSqlRecord.Date)).FirstOrDefault() is null)
                    {
                        await context.AddItemAsync<SqlRecord>(OperatingSqlRecord);
                        SqlRecords.Add(OperatingSqlRecord);
                    }
                }
                else
                {
                    //Update
                    await context.UpdateItemAsync<SqlRecord>(OperatingSqlRecord);
                    var sqlRecCopy = OperatingSqlRecord.Clone();
                    var index = SqlRecords.IndexOf(OperatingSqlRecord);
                    SqlRecords.RemoveAt(index);
                    SqlRecords.Insert(index, sqlRecCopy);
                }
                //SetOperatingSqlRecordCommand.Execute(new());
            }, busyText);
        }

        private async Task SaveToMonthRecordAsync()
        {
            if (OperatingSqlRecord is null)
                return;

            var (isValid, errorMsg) = OperatingSqlRecord.Validate();
            if (!isValid)
            {
                await Shell.Current.DisplayAlert("Validation Error", errorMsg, "Ok");
                return;
            }
            var busyText = OperatingSqlRecord.Key == 0 ? "Creating record..." : "Updating Record...";
            await ExcuteAsync(async () =>
            {
                var temp = new DateTime(OperatingSqlRecord.Date.Year, OperatingSqlRecord.Date.Month, 1);
                if ((await context.GetFilteredAsync<SqlMonthRecord>(
                    p => (p.Date == (temp).Date &&
                     p.ID == OperatingSqlRecord.ID))).FirstOrDefault() is null)
                {
                    //Create
                    OperatingSqlMonthRecord = new SqlMonthRecord
                    {
                        Date = new DateTime(OperatingSqlRecord.Date.Year, OperatingSqlRecord.Date.Month, 1),
                        ID = OperatingSqlRecord.ID,
                        Workers = OperatingSqlRecord.Workers,
                        Income = OperatingSqlRecord.Income,
                        Tip = OperatingSqlRecord.Tip,
                        DailyExp = OperatingSqlRecord.DailyExp,
                        DailyNet = OperatingSqlRecord.DailyNet,
                        NumOfRecords = 1
                    };
                    await context.AddItemAsync<SqlMonthRecord>(OperatingSqlMonthRecord);
                    SqlMonthRecords.Add(OperatingSqlMonthRecord);
                    await fireBaseService.SaveMonthRecord(OperatingSqlMonthRecord);
                }
                else
                {
                    //Update
                    var tempSqlMonthRecord = (await context.GetFilteredAsync<SqlMonthRecord>(
                    p => p.Date == (temp).Date &&
                     p.ID == OperatingSqlRecord.ID)).FirstOrDefault();

                    SetOperatingSqlMonthRecord(tempSqlMonthRecord);
                    var index = SqlMonthRecords.IndexOf(SqlMonthRecords.Where(
                        o => (o.Date == OperatingSqlMonthRecord.Date &&
                        o.ID == OperatingSqlMonthRecord.ID)).FirstOrDefault());

                    tempSqlMonthRecord.Workers += OperatingSqlRecord.Workers;
                    tempSqlMonthRecord.Income += OperatingSqlRecord.Income;
                    tempSqlMonthRecord.Tip += OperatingSqlRecord.Tip;
                    tempSqlMonthRecord.DailyNet += OperatingSqlRecord.DailyNet;
                    tempSqlMonthRecord.DailyExp += OperatingSqlRecord.DailyExp;
                    tempSqlMonthRecord.NumOfRecords++;
                    if ((
                    tempSqlMonthRecord.Date.Month == 1 ||
                    tempSqlMonthRecord.Date.Month == 3 ||
                    tempSqlMonthRecord.Date.Month == 5 ||
                    tempSqlMonthRecord.Date.Month == 7 ||
                    tempSqlMonthRecord.Date.Month == 8 ||
                    tempSqlMonthRecord.Date.Month == 10 ||
                    tempSqlMonthRecord.Date.Month == 12) && OperatingSqlRecord.Date.Day == 31)
                    {
                        tempSqlMonthRecord.IsClosed = true;
                    }
                    else if (
                    (
                    tempSqlMonthRecord.Date.Month == 4 ||
                    tempSqlMonthRecord.Date.Month == 6 ||
                    tempSqlMonthRecord.Date.Month == 9 ||
                    tempSqlMonthRecord.Date.Month == 11) && OperatingSqlRecord.Date.Day == 30)
                    {
                        tempSqlMonthRecord.IsClosed = true;
                    }
                    else if ((
                    tempSqlMonthRecord.Date.Month == 4) &&
                    DateTime.IsLeapYear(tempSqlMonthRecord.Date.Year) &&
                    OperatingSqlRecord.Date.Day == 29)
                    {
                        tempSqlMonthRecord.IsClosed = true;
                    }
                    else if ((
                    tempSqlMonthRecord.Date.Month == 4) &&
                    !DateTime.IsLeapYear(tempSqlMonthRecord.Date.Year) &&
                    OperatingSqlRecord.Date.Day == 28)
                    {
                        tempSqlMonthRecord.IsClosed = true;
                    }

                    SetOperatingSqlMonthRecord(tempSqlMonthRecord);
                    await context.UpdateItemAsync<SqlMonthRecord>(OperatingSqlMonthRecord);
                    var sqlMonthRecCopy = OperatingSqlMonthRecord.Clone();

                    SqlMonthRecords.RemoveAt(index);
                    SqlMonthRecords.Insert(index, sqlMonthRecCopy);
                    await fireBaseService.SaveMonthRecord(OperatingSqlMonthRecord);
                }
                //SetOperatingSqlMonthRecordCommand.Execute(new());
            }, busyText);
        }

        [RelayCommand]
        async Task GoToMonthDetails()
        {
            var date = 0;
            if (date == null)
                return;
            await Shell.Current.GoToAsync(nameof(MonthDetailsPage), true, new Dictionary<string, object>
            {
                {"CurrentDateTime",CurrentDateTime },
                {"Sites",Sites },
                {"SelectedSiteIndex",SelectedSiteIndex },
                {"SelectedSite",SelectedSite },
                {"SqlRecords",SqlRecords },
                {"SqlMonthRecords",SqlMonthRecords },
                {"OperatingSqlRecord",OperatingSqlRecord },
                {"OperatingSqlMonthRecord",OperatingSqlMonthRecord },
                {"CurrentViewMonthRecord",CurrentViewMonthRecord },
            });
        }

        [RelayCommand]
        private async Task DeleteRecordAsync(int key)
        {
            await ExcuteAsync(async () =>
            {
                if (await context.DeleteItemByKeyAsync<SqlRecord>(key))
                {
                    var rec = SqlRecords.FirstOrDefault(p => p.Key == key);
                    SqlRecords.Remove(rec);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Delete error", "Records was not deleted", "Ok");
                }
            }, "Deleting record");
        }

        private async Task ExcuteAsync(Func<Task> operation, string? busyText = null)
        {
            IsBusy = true;
            BusyText = busyText ?? "Processing...";
            try
            {
                await operation.Invoke();
            }
            finally
            {
                IsBusy = false;
                BusyText = "Processing...";
            }
        }

        [RelayCommand]
        private void SetOperatingSqlRecord(SqlRecord? sqlRecord) => OperatingSqlRecord = sqlRecord ?? new();
        [RelayCommand]
        private void SetOperatingSqlMonthRecord(SqlMonthRecord? sqlMonthRecord) => OperatingSqlMonthRecord = sqlMonthRecord ?? new();
        public async Task CheckConnectivity()
        {
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("Connection failure", "No internet available", "Ok");
                return;
            }
        }
    }
}
