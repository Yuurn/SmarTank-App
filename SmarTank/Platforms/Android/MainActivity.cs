using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Android.Runtime;

namespace SmarTank
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestBluetoothPermissions();

        }

        private void RequestBluetoothPermissions()
        {
            // For Android 12+ (API 31+), Bluetooth permissions need to be requested separately
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                // Bluetooth and Location permissions
                if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.BluetoothScan) != Permission.Granted ||
                    ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.BluetoothConnect) != Permission.Granted ||
                    ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    // Request the required permissions
                    ActivityCompat.RequestPermissions(this, new string[]
                    {
                        Android.Manifest.Permission.BluetoothScan,
                        Android.Manifest.Permission.BluetoothConnect,
                        Android.Manifest.Permission.AccessFineLocation
                    }, 0);
                }
            }
            else
            {
                // For Android below API level 31, just request the legacy Bluetooth permissions
                if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Bluetooth) != Permission.Granted ||
                    ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.BluetoothAdmin) != Permission.Granted ||
                    ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new string[]
                    {
                        Android.Manifest.Permission.Bluetooth,
                        Android.Manifest.Permission.BluetoothAdmin,
                        Android.Manifest.Permission.AccessFineLocation
                    }, 0);
                }
            }
        }

        // Handle the permission request result
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == 0)
            {
                bool allPermissionsGranted = true;
                foreach (var result in grantResults)
                {
                    if (result != Permission.Granted)
                    {
                        allPermissionsGranted = false;
                        break;
                    }
                }

                if (allPermissionsGranted)
                {
                    // Permissions granted, proceed with Bluetooth operations
                    Console.WriteLine("All permissions granted. Proceed with Bluetooth operations.");
                }
                else
                {
                    // Permissions not granted, notify the user and handle appropriately
                    Console.WriteLine("Permissions denied. Unable to perform Bluetooth operations.");
                    // Optionally, show an alert or provide guidance to the user.
                    // For example: DisplayAlert("Bluetooth Permissions", "Permissions are required to access Bluetooth functionality.", "OK");
                }
            }
        }
        //Need to write something that stops/pauses the bluetooth when the page is left so it can reconnect..... not sure how yet

    }
}
