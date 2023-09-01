using ValetAccountingMaster.ViewModel;

namespace ValetAccountingMaster.View;

public partial class DayDetailsPage : ContentPage
{
    private readonly DayDetailsViewModel _dayDetailsViewModel;
    public DayDetailsPage(DayDetailsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		_dayDetailsViewModel = viewModel;
	}

    protected override  void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
    private  void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_dayDetailsViewModel == null)
        {
            return;
        }
        _dayDetailsViewModel.IsBusy = true;
        _dayDetailsViewModel.IsNotBusy = false;
        _dayDetailsViewModel.UpdateSite();
        _dayDetailsViewModel.IsBusy = false;
        _dayDetailsViewModel.IsNotBusy = true;
    }

    private  void DatePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if(_dayDetailsViewModel == null) {
            return;
        }
        _dayDetailsViewModel.IsBusy = true;
        _dayDetailsViewModel.IsNotBusy = false;
        _dayDetailsViewModel.UpdateSite();
        _dayDetailsViewModel.IsBusy = false;
        _dayDetailsViewModel.IsNotBusy = true;
    }
}