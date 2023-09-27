using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System.Collections.ObjectModel;
using ValetAccountingMaster.Model;
using ValetAccountingMaster.Data;
using CommunityToolkit.Mvvm.Input;
using System.Data;
using ClosedXML.Excel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;

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

        [RelayCommand]
        async Task Export()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Day");
                dt.Columns.Add("Inc");
                dt.Columns.Add("Exp");
                dt.Columns.Add("Tip");
                dt.Columns.Add("Net");
                dt.Columns.Add("Wrks");

                foreach (SqlRecord s in SelectedMonthRecords)
                {
                    DataRow dr = dt.NewRow();
                    dr[0] = s.Date;
                    dr[1] = Convert.ToDouble(s.Income);
                    dr[2] = Convert.ToDouble(s.DailyExp);
                    dr[3] = Convert.ToDouble(s.Tip);
                    dr[4] = Convert.ToDouble(s.DailyNet);
                    dr[5] = Convert.ToDouble(s.Workers);
                    dt.Rows.Add(dr);
                }

                dt.AcceptChanges();

                string filename = CurrentDateTime.Month.ToString() + "-" + CurrentDateTime.Year.ToString() + "-" + SelectedSite.ID;
                string filePath = Path.Combine(FileSystem.AppDataDirectory, filename);

                XLWorkbook wb = new XLWorkbook();

                var ws = wb.Worksheets.Add(dt, "Sheet 1");

                //if (filename.Contains("."))
                //{
                    //int IndexOfLastFullStop = filename.LastIndexOf('.');
                    //filename = filename.Substring(0, IndexOfLastFullStop) + ".xlsx";
                //}
                //filePath = filePath + ".xlsx";

                CancellationToken ct = new CancellationToken();
                var Pth = await PickFolderAsync(ct);
                Pth = Pth + "\\" + filename + ".xlsx";

                wb.SaveAs(Pth);


                //wb.SaveAs(filePath);
            }
            catch (Exception e)
            {

                throw;
            }

            async Task<string> PickFolderAsync(CancellationToken cancellationToken)
            {
                var result = await FolderPicker.Default.PickAsync(cancellationToken);
                if (result.IsSuccessful)
                {
                    await Toast.Make($"The folder was picked: Name - {result.Folder.Name}, Path - {result.Folder.Path}", ToastDuration.Long).Show(cancellationToken);
                }
                else
                {
                    await Toast.Make($"The folder was not picked with error: {result.Exception.Message}").Show(cancellationToken);
                }
                return result.Folder.Path;
            }

        }


    }
}
