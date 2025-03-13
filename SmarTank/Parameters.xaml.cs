namespace SmarTank;

public partial class Parameters : ContentPage
{
	public Parameters()
	{
		InitializeComponent();
        BindingContext = new SensorViewModel();
	}
    private async void ToLog(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TestingLog());

    }
}