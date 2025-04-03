
namespace SmarTank;

public partial class Cycle : ContentPage
{
    private const string CurrentModeKey = "CurrentMode";
    private const string StartTimeKey = "StartTime";
    private const string LastPopupTimeKey = "LastPopupTime";
    public Cycle()
	{
        InitializeComponent();
	}

    public Mode CurrentMode
    {
        get => (Mode)Preferences.Get(CurrentModeKey, (int)Mode.Maintenance);
        set => Preferences.Set(CurrentModeKey, (int)value);
    }

    private DateTime StartTime
    {
        get => Preferences.Get(StartTimeKey, DateTime.MinValue);
        set => Preferences.Set(StartTimeKey, value);
    }

    private DateTime LastPopupTime
    {
        get => Preferences.Get(LastPopupTimeKey, DateTime.MinValue);
        set => Preferences.Set(LastPopupTimeKey, value);
    }


    private async void OnCyclingModeClicked(object sender, EventArgs e)
    {
        CurrentMode = Mode.Cycling;
        StartTime = DateTime.Now;
        LastPopupTime = DateTime.Now;
        ShowPopup();
        await Navigation.PushAsync(new Parameters(CurrentMode));
    }

    private async void OnMaintenanceModeClicked(object sender, EventArgs e)
    {
        CurrentMode = Mode.Maintenance;
        ShowPopup();
        await Navigation.PushAsync(new Parameters(CurrentMode));
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        if (CurrentMode == Mode.Cycling)
        {
            await DisplayAlert("Mode Information", "Continuing cycling mode. Ensure all sensors are functioning properly.", "OK");
            await Navigation.PushAsync(new Parameters(CurrentMode));
        }
        else
        {
            await DisplayAlert("Error", "Cycling mode is not active. Please select Cycling Mode first.", "OK");
        }
    }


    public void ShowPopup()
    {
        string message = CurrentMode switch
        {
            Mode.Cycling => "Cycling mode is active. Add 2-3 drops of pure Ammonia per 10 gallons. Chemical test kit should indicate ammonia levels of 2-4 ppm. Add beneficial bacteria per bottle instructions.",
            Mode.Maintenance => "Maintenance mode is active. Please ensure sensors are properly placed in aquarium.",
            _ => "Unknown mode."
        };

        DisplayAlert("Mode Information", message, "OK");
    }



     private async void toCycleTutorial(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CycleTutorial());

    }
}