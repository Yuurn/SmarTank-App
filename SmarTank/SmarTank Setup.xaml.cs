namespace SmarTank;

public partial class SmarTank_Setup : ContentPage
{
	public SmarTank_Setup()
	{
		InitializeComponent();
	}

    private async void ToParams(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Parameters());

    }
}