using Plugin.BluetoothClassic.Abstractions;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
namespace SmarTank;


public partial class Parameters : ContentPage
{
    private Timer _timer;
    private DateTime StartTime => Preferences.Get("StartTime", DateTime.MinValue);
    private DateTime LastPopupTime
    {
        get => Preferences.Get("LastPopupTime", DateTime.MinValue);
        set => Preferences.Set("LastPopupTime", value);
    }

    private Mode _mode;

    public Parameters(Mode mode)
	{
		InitializeComponent();
      //  BindingContext = new SensorViewModel();
        _mode = mode;
        DisplayModeInfo();
    }
   

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartTimer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopTimer();
    }



    private void DisplayModeInfo()
    {
        string message = _mode switch
        {
            //Mode.Cycling => "Cycling mode is active.",
          //  Mode.Maintenance => "Maintenance mode is active. Please check and calibrate the sensors.",
            _ => "Unknown mode."
        };

       // DisplayAlert("Mode Information", message, "OK");
    }



    private void CheckAndShowPopup(object state)
    {
        if (_mode == Mode.Cycling)
        {
            var elapsedTime = DateTime.Now - StartTime;

            if (elapsedTime.TotalDays >= 42) // 6 weeks
            {
                DisplayAlert("Reminder", "Cycling mode has completed 6 weeks. You should see an increase in Nitrates. It's time to transition to maintenance mode and add fish to aquarium!", "OK");
                StopTimer();
            }
            else if (elapsedTime.TotalDays >= 21 && elapsedTime.TotalDays < 22) // 3 weeks
            {
                DisplayAlert("Reminder", "Cycling mode has reached 3 weeks. You should see an increase in the Nitrites when checking aquarium chemicals.", "OK");
            }
            else if ((DateTime.Now - LastPopupTime).TotalDays >= 2) // Every other day
            {
                DisplayAlert("Reminder", "It's time to check and log the aquarium chemicals.", "OK");
                LastPopupTime = DateTime.Now;
            }

        }
    }

    //method that starts the time. The time is kept in days
    private void StartTimer()
    {
        _timer = new Timer(CheckAndShowPopup, null, TimeSpan.Zero, TimeSpan.FromHours(1));
    }

    //method to stop the timer.
    private void StopTimer()
    {
        _timer?.Dispose();
    }

    //the next page when the button is pushed is TestingLog
    private async void ToLog(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TestingLog());

    }
}