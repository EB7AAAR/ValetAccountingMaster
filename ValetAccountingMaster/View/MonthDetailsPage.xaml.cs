using ValetAccountingMaster.ViewModel;

namespace ValetAccountingMaster.View;

public partial class MonthDetailsPage : ContentPage
{
    private readonly MonthDetailsViewModel _monthDetailsViewModel;
    public MonthDetailsPage(MonthDetailsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		_monthDetailsViewModel = viewModel;
	}
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        //if (!_monthDetailsViewModel.IsAlreadyCreated)
        //{
        //    _monthDetailsViewModel.IsAlreadyCreated = true;
        //}
        //await _monthDetailsViewModel.UpdateCurrentViewMonthRecord();
    }
    private async void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_monthDetailsViewModel != null )
        {
            await _monthDetailsViewModel.UpdateCurrentViewMonthRecord();
        }
    }

    private async void DatePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_monthDetailsViewModel != null )
        {
            await _monthDetailsViewModel.UpdateCurrentViewMonthRecord();
        }
    }

    
}