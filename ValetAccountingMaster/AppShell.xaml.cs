using ValetAccountingMaster.View;

namespace ValetAccountingMaster;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(MonthDetailsPage),typeof(MonthDetailsPage));
		Routing.RegisterRoute(nameof(DayDetailsPage),typeof(DayDetailsPage));
	}
}
