using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System.Collections.ObjectModel;
using ValetAccountingMaster.Model;
using ValetAccountingMaster.Data;

namespace ValetAccountingMaster.ViewModel
{
    [QueryProperty(nameof(CurrentDateTime), nameof(CurrentDateTime))]
    [QueryProperty(nameof(Sites), nameof(Sites))]
    [QueryProperty(nameof(SelectedSiteIndex), nameof(SelectedSiteIndex))]
    [QueryProperty(nameof(SqlRecords), nameof(SqlRecords))]
    [QueryProperty(nameof(SelectedSite), nameof(SelectedSite))]

    public partial class DayDetailsViewModel : BaseViewModel
    {
        [ObservableProperty]
        public ObservableCollection<SiteName> sites = new();
        [ObservableProperty]
        private int selectedSiteIndex;
        [ObservableProperty]
        private SiteName selectedSite = new();
        

        [ObservableProperty]
        private List<SqlRecord> sqlRecords = new();

        [ObservableProperty]
        private List<SqlRecord> selectedMonthRecords = new();

        [ObservableProperty]
        private DateTime currentDateTime = new();

        [ObservableProperty]
        private bool isBusy;
         [ObservableProperty]
        private bool isNotBusy;


        public DayDetailsViewModel(DatabaseContext databaseContext)
        {
            this.Title = "SINGLE DAY SUMMERY";
            CurrentDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        }

        public void UpdateSite()
        {
            if (!(SqlRecords.Any()))
                return;
            

            CurrentDateTime = new DateTime(CurrentDateTime.Year, CurrentDateTime.Month, 1);
            var monthRecords = SqlRecords
                .Where(
                p =>
                p.Date.Year == CurrentDateTime.Date.Year &&
                p.Date.Month == CurrentDateTime.Month &&
                p.ID == Sites[SelectedSiteIndex].ID
                ).ToList();

            SelectedMonthRecords = monthRecords;
            
        }
        public void UpdateDate()
        {
            if (!(SqlRecords.Any()))
                return;

            
            CurrentDateTime = new DateTime(CurrentDateTime.Year, CurrentDateTime.Month, 1);
            var monthRecords = SqlRecords
                .Where(
                p =>
                p.Date.Year == CurrentDateTime.Date.Year &&
                p.ID == Sites[SelectedSiteIndex].ID &&
                p.Date.Month == CurrentDateTime.Month
                ).ToList();

            SelectedMonthRecords = monthRecords;
            
        }
    }
}
