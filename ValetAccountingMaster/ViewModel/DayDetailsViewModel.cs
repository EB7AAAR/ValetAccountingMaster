using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System.Collections.ObjectModel;
using ValetAccountingMaster.Model;
using ValetAccountingMaster.Data;
using CommunityToolkit.Mvvm.Input;
using System.Data;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Sheets.v4.Data;
using Photos;
using EventKit;
using Foundation;
using System.Text;
using System;

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

        private async Task<string> ToCsvStringAsync(DataTable dataTable)
        {
            var builder = new StringBuilder();

            // Add headers
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                builder.Append($"{dataTable.Columns[i].ColumnName},");
            }
            builder.AppendLine();

            // Add rows
            foreach (DataRow row in dataTable.Rows)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    builder.Append($"{row[i]},");
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }

        [RelayCommand]
        async Task Export()
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

            var csvString = await ToCsvStringAsync(dt);

            // Create temporary file
            var tempFile = Path.Combine(FileSystem.Current.CacheDirectory, $"{Guid.NewGuid()}.csv");
            await File.WriteAllTextAsync(tempFile, csvString);

            // Create content for sharing
            //var shareContent = new ShareContent
            //{
            //Title = "DataTable Export",
            //Text = "Sharing DataTable data as CSV",
            //FilePaths = new string[] { tempFile }
            //};

            await Clipboard.Default.SetTextAsync(csvString);


            await Share.RequestAsync(new ShareFileRequest
            {
                Title = Title,
                File = new ShareFile(tempFile)
            });

            // Show share sheet
            //await Share.RequestAsync(shareContent);

            #region MyRegion
            //try
            //{
            //    string[] scopes = {SheetsService.Scope.Spreadsheets };
            //    var service = new SheetsService(new BaseClientService.Initializer()
            //    { HttpClientInitializer = GoogleWebAuthorizationBroker.
            //    AuthorizeAsync(new ClientSecrets {
            //        ClientId = "870346727529-kjf1ljm7adbe4dt2vgqe4t2uaupmv4rv.apps.googleusercontent.com",
            //        ClientSecret = "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",
            //    }, scopes, "user", CancellationToken.None, new FileDataStore("MyAppsToken")).Result,
            //        ApplicationName = "google sheet api test", });

            //    SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum valueInputOption  = 
            //        SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            //    SpreadsheetsResource.ValuesResource.GetRequest getRequest = service.Spreadsheets.Values.Get(
            //        "1hMlZiNVT7H8MzMginSFgg5wd3abCKc1arc4Im1QLe_Y", "Records!A:F");

            //    System.Net.ServicePointManager.ServerCertificateValidationCallback = 
            //        delegate ( object sender2, X509Certificate certificate,
            //        X509Chain chain,System.Net.Security.SslPolicyErrors sslPolicyErrors)
            //        { return true; };


            //    foreach (var rec in selectedMonthRecords)
            //    {
            //        ValueRange getResponse = getRequest.Execute();
            //        IList<IList<object>> values = getResponse.Values;
            //        var range = $"{"Records"}!A" + (values.Count + 1) + ":F" + values.Count + 1;
            //        var valueRange = new ValueRange();
            //        valueRange.Values = new List<IList<object>> { new  List<object>()
            //    {rec.ID,rec.Income,rec.DailyExp,rec.Tip,rec.DailyNet,rec.Workers }};

            //        var updateRequest = service.Spreadsheets.Values.Update(valueRange,
            //            "1hMlZiNVT7H8MzMginSFgg5wd3abCKc1arc4Im1QLe_Y", range);
            //        updateRequest.ValueInputOption = valueInputOption;

            //        var updateResponse = updateRequest.Execute();
            //    }

            //}
            //catch (Exception)
            //{

            //    throw;
            //}


            #endregion

            //try
            //{
            //    DataTable dt = new DataTable();
            //    dt.Columns.Add("Day");
            //    dt.Columns.Add("Inc");
            //    dt.Columns.Add("Exp");
            //    dt.Columns.Add("Tip");
            //    dt.Columns.Add("Net");
            //    dt.Columns.Add("Wrks");

            //    foreach (SqlRecord s in SelectedMonthRecords)
            //    {
            //        DataRow dr = dt.NewRow();
            //        dr[0] = s.Date;
            //        dr[1] = Convert.ToDouble(s.Income);
            //        dr[2] = Convert.ToDouble(s.DailyExp);
            //        dr[3] = Convert.ToDouble(s.Tip);
            //        dr[4] = Convert.ToDouble(s.DailyNet);
            //        dr[5] = Convert.ToDouble(s.Workers);
            //        dt.Rows.Add(dr);
            //    }

            //    dt.AcceptChanges();

            //    string filename = (DateTime.Now.Year.ToString() +
            //        DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() +
            //        DateTime.Now.Hour.ToString()+ DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString());
            //    //string filePath = Path.Combine(FileSystem.AppDataDirectory, filename);

            //    XLWorkbook wb = new XLWorkbook();

            //    var ws = wb.Worksheets.Add(dt, "Sheet 1");

            //    //if (filename.Contains("."))
            //    //{
            //        //int IndexOfLastFullStop = filename.LastIndexOf('.');
            //        //filename = filename.Substring(0, IndexOfLastFullStop) + ".xlsx";
            //    //}
            //    //filePath = filePath + ".xlsx";

            //    CancellationToken ct = new CancellationToken();
            //    var Pth = await PickFolderAsync(ct);
            //    //Pth = Pth + "/" + filename + ".xlsx";
            //    filename = filename + ".xlsx";
            //    var path = Path.Combine(Pth, filename);
            //    wb.SaveAs(path);


            //    //wb.SaveAs(filePath);
            //}
            //catch (Exception e)
            //{

            //    throw;
            //}

            //async Task<string> PickFolderAsync(CancellationToken cancellationToken)
            //{
            //    var result = await FolderPicker.Default.PickAsync(cancellationToken);
            //    if (result.IsSuccessful)
            //    {
            //        //await Toast.Make($"The folder was picked: Name - {result.Folder.Name}, Path - {result.Folder.Path}", ToastDuration.Long).Show(cancellationToken);
            //    }
            //    else
            //    {
            //        //await Toast.Make($"The folder was not picked with error: {result.Exception.Message}").Show(cancellationToken);
            //    }
            //    return result.Folder.Path;
            //}

        }
        
    }
}
