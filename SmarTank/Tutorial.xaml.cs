namespace SmarTank;

public partial class Tutorial : ContentPage
{
	public Tutorial()
	{
		InitializeComponent();
	}
    private async void NextTutorial(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SmarTank_Setup());

    }
}