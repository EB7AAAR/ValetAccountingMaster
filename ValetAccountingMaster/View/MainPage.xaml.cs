using CommunityToolkit.Maui.Core.Platform;
using ValetAccountingMaster.ViewModel;


namespace ValetAccountingMaster.View;

public partial class MainPage : ContentPage
{
    private readonly RecordsViewModel _recordsViewModel;
    public MainPage(RecordsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _recordsViewModel = viewModel;
        
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_recordsViewModel.IsAlreadyCreated)
        {
            await _recordsViewModel.GetSqlAllRecordsAsync();
            await _recordsViewModel.GetFirebaseRecordAsync();
            await _recordsViewModel.MatchRecords();
            _recordsViewModel.IsAlreadyCreated = true;
        }
        await _recordsViewModel.UpdateCurrentViewMonthRecord();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    private async void picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_recordsViewModel != null)
        {
            await _recordsViewModel.UpdateCurrentViewMonthRecord();
        }
    }

    private async void DatePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_recordsViewModel != null )
        {
            await _recordsViewModel.UpdateCurrentViewMonthRecord();
        }
    }
}