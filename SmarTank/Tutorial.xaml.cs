namespace SmarTank;

public partial class Tutorial : ContentPage
{
	public Tutorial()
	{
		InitializeComponent();
	}
    private async void NextTutorial(object sender, EventArgs e)
    {
        //  count++;
        
        //if (count == 1)
        //     CounterBtn.Text = $"Clicked {count} time";
        // else
        //    CounterBtn.Text = $"Clicked {count} times";

        //SemanticScreenReader.Announce(CounterBtn.Text);

        await Navigation.PushAsync(new SmarTank_Setup());

    }
}