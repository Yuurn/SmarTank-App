using Plugin.BluetoothClassic.Abstractions;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
namespace SmarTank
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private async void ContNewTank(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Tutorial());
        }

        private async void ContCurTank(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Cycle());
        }
        private async void CheckBT(object sender, EventArgs e)
        {
            // Request Location Permission first (Bluetooth scanning requires this)
            var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            // Request Bluetooth permissions for Android 12 and above (this will be handled by Maui)
            var bluetoothStatus = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();
            if (bluetoothStatus != PermissionStatus.Granted)
            {
                bluetoothStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();
            }

            if (locationStatus == PermissionStatus.Granted && bluetoothStatus == PermissionStatus.Granted)
            {
                // Permissions granted, proceed with Bluetooth operations
                await Navigation.PushAsync(new NewPage1());
            }
            else
            {
                // Permissions denied, inform the user
                await DisplayAlert("Permission Error", "Bluetooth and Location permissions are required.", "OK");
            }
        }
    }
}