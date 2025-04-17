using Plugin.BluetoothClassic.Abstractions;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Microsoft.Maui.Layouts;

namespace SmarTank;


public partial class Parameters : ContentPage
{
    private BluetoothClient _bluetoothClient;
    private BluetoothDeviceInfo _deviceInfo;
    private System.IO.Stream _stream;
    private IDispatcherTimer _connectionTimer;
    private int _reconnectAttempts = 0;
    private bool _isReadingData = false; // Flag to control reading loop
    private Timer _timer;

    bool[] TempRange = new bool[2];
    bool[] pHRange = new bool[2];
    bool TDSRange = false;
    bool ConductivityRange = false;
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
        _mode = mode;
        DisplayModeInfo();
        ConnectToBluetoothAsync();
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
        DisconnectBluetooth();
    }
    private async Task ConnectToBluetoothAsync()
    {
        try
        {
            _bluetoothClient = new BluetoothClient();
            var devices = _bluetoothClient.DiscoverDevices();
            _deviceInfo = devices.FirstOrDefault(d => d.DeviceName == "DSD TECH HC-05");

            if (_deviceInfo != null)
            {
                await DisplayAlert("Bluetooth Connected", "HC-05 device found", "OK");

                bool isConnected = await Task.Run(() => ConnectToDeviceWithTimeout(_deviceInfo));

                if (isConnected)
                {
                    _stream = _bluetoothClient.GetStream();
                    StartReadingData();
                    KeepConnectionAlive();
                }
                else
                {
                    await DisplayAlert("Error", "Unable to connect to the device in time.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "HC-05 device not found.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to connect to Bluetooth device: {ex.Message}", "OK");
            Console.WriteLine($"Error in ConnectToBluetoothAsync: {ex.Message}");
        }
    }

    private bool ConnectToDeviceWithTimeout(BluetoothDeviceInfo deviceInfo)
    {
        try
        {
            var deviceAddress = InTheHand.Net.BluetoothAddress.Parse(deviceInfo.DeviceAddress.ToString());
            _bluetoothClient.Connect(deviceAddress, BluetoothService.SerialPort);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
            return false;
        }
    }

    private void KeepConnectionAlive()
    {
        _connectionTimer = Dispatcher.CreateTimer();
        _connectionTimer.Interval = TimeSpan.FromSeconds(10); // Increased interval to reduce frequency
        _connectionTimer.Tick += async (sender, e) =>
        {
            if (_bluetoothClient.Connected)
            {
                Console.WriteLine("Connection is alive");
                _reconnectAttempts = 0;
            }
            else
            {
                if (_reconnectAttempts < 3)
                {
                    Console.WriteLine("Attempting to reconnect...");
                    await ConnectToBluetoothAsync();
                    _reconnectAttempts++;
                }
                else
                {
                    Console.WriteLine("Reconnection attempts exceeded. Giving up.");
                    _connectionTimer.Stop();
                }
            }
        };
        _connectionTimer.Start();
    }

    private async void StartReadingData()
    {
        try
        {
            byte[] buffer = new byte[256];
            int bytesRead;
            StringBuilder completeData = new StringBuilder();
            _isReadingData = true; // Set flag to true when starting to read data

            while (_bluetoothClient?.Connected == true && _isReadingData)
            {
                try
                {
                    if (_stream != null && _stream.CanRead)
                    {
                        bytesRead = await Task.Run(() => _stream.Read(buffer, 0, buffer.Length));
                        if (bytesRead > 0)
                        {
                            var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            completeData.Append(data);

                            Console.WriteLine($"Raw data received: {data}");

                            if (completeData.ToString().EndsWith("\n"))
                            {
                                var finalData = completeData.ToString().Trim();
                                Console.WriteLine($"Complete data: {finalData}");

                                Dispatcher.Dispatch(() =>
                                {
                                    UpdateSensorValues(finalData);
                                });

                                completeData.Clear();
                            }
                        }
                        else
                        {
                            Console.WriteLine("Stream read returned 0 bytes. Closing the connection.");
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Stream is not readable or is closed.");
                        break;
                    }

                    // Add a small delay to prevent the loop from blocking the UI thread
                    await Task.Delay(100); // Add delay to prevent tight looping and allow UI updates

                }
                catch (IOException ex)
                {
                    Console.WriteLine($"IOException: Bluetooth connection lost: {ex.Message}");
                    Dispatcher.Dispatch(async () =>
                    {
                        await DisplayAlert("Left Parameters Page", $"Bluetooth disconnected: {ex.Message}", "OK");//probs remove this
                    });
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error: Failed to read data: {ex.Message}");
                    Dispatcher.Dispatch(async () =>
                    {
                        await DisplayAlert("Error", $"Failed to read data: {ex.Message}", "OK");
                    });
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in StartReadingData: {ex.Message}");
        }
        finally
        {
            // Dispose of the stream and client when done
            _stream?.Dispose();
            _stream = null; // Set to null after disposing
            _bluetoothClient?.Dispose();
            _bluetoothClient = null; // Set to null after disposing
        }
    }


    private void DisconnectBluetooth()
    {
        try
        {
            _isReadingData = false; // Set flag to false to stop reading data
            _connectionTimer?.Stop();
            _stream?.Dispose();
            _stream = null; // Set to null after disposing
            _bluetoothClient?.Close();
            _bluetoothClient?.Dispose();
            _bluetoothClient = null; // Set to null after disposing
            Console.WriteLine("Bluetooth connection closed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DisconnectBluetooth: {ex.Message}");
        }
    }

    private void UpdateSensorValues(string data)
    {
        Console.WriteLine($"start values");
        var values = data.Split(',');

        if (values.Length >= 4)
        {
            TemperatureLabel.Text = values[0] + " °F";
            PHLabel.Text = values[1];
            TDSLabel.Text = values[2] + " ppm";
            ConductivityLabel.Text = values[3] + " μS/cm";
        }
        else
        {
            TemperatureLabel.Text = "Error";
            PHLabel.Text = "Error";
            TDSLabel.Text = "Error";
            ConductivityLabel.Text = "Error";
        }



        if (double.TryParse(values[0], out double TempValue)) //Checking Temp values
        {
            if (TempValue < 60)
            {
                //Temp is less than 60
                TempRange[0] = true;
                TempRange[1] = false;
                tempWarning.IsVisible = true;
                tempWarningLabel.IsVisible = true;

            }
            else if (TempValue > 85)
            {
                // Temp is greater than 85
                TempRange[1] = true;
                TempRange[0] = false;
                tempWarning.IsVisible = true;
                tempWarningLabel.IsVisible = true;
            }
            else    //reset booleans for Temp values
            {
                TempRange[0] = false;
                TempRange[1] = false;
                tempWarning.IsVisible = false;
                tempWarningLabel.IsVisible = false;
            }
        }

        if (double.TryParse(values[1], out double phValue))
        {
            if (phValue < 6.5)
            {
                Console.WriteLine("less than 6.5");
                pHRange[0] = true;
                pHRange[1] = false;
                phWarning.IsVisible = true;
                phWarningLabel.IsVisible = true;
            }
            else if (phValue > 8)
            {
                Console.WriteLine("greater than 8.0");
                pHRange[1] = false;
                pHRange[0] = true;
                phWarning.IsVisible = true;
                phWarningLabel.IsVisible = true;
            }
            else
            {
                pHRange[0] = false;
                pHRange[1] = false;
                phWarning.IsVisible = false;
                phWarningLabel.IsVisible = false;
            }
        }

        if (double.TryParse(values[2], out double TDSValue)) //Checking TDS values
        {
            if (TDSValue > 450)
            {
                //TDS is greater than 450
                TDSRange = true;
                tdsWarning.IsVisible = true;
                tdsWarningLabel.IsVisible = true;
            }
           
            else    //reset booleans for TDS values
            {
                TDSRange = false;
                tdsWarning.IsVisible = false;
                tdsWarningLabel.IsVisible = false;
            }
        }
        if (double.TryParse(values[2], out double CondValue)) //Checking TDS values
        {
            if (CondValue > 500)
            {
                //TDS is greater than 450
                ConductivityRange = true;
                condWarning.IsVisible = true;
                condWarningLabel.IsVisible = true;
            }

            else    //reset booleans for TDS values
            {
                ConductivityRange = false;
                condWarning.IsVisible = false;
                condWarningLabel.IsVisible = false;
            }
        }
    }
    private async void WarningPHTapped(object sender, EventArgs e)
    {
        Console.WriteLine("WarningPHTapped triggered");

        if (pHRange[0]) // pH is too low
        {
            if (_mode == Mode.Maintenance)
            {
                await DisplayAlert("Warning", "pH is too low! Perform a 25% water change.\n\nNatural Solution: Add driftwood or peat moss.\n\nTip: Check the pH of your tap water.", "OK");
            }
            else if (_mode == Mode.Cycling)
            {
                await DisplayAlert("Cycling Tip", "pH is slightly low — this can happen during cycling.\n\nMonitor regularly using the chemical test kit.", "OK");
            }
        }
        else if (pHRange[1]) // pH is too high
        {
            if (_mode == Mode.Maintenance)
            {
                await DisplayAlert("Warning", "pH is too high! Perform a 25% water change.\n\nNatural Solution: Add crushed coral and increase aeration.\n\nTip: Check the pH of your tap water.", "OK");
            }
            else if (_mode == Mode.Cycling)
            {
                await DisplayAlert("Cycling Tip", "pH is slightly high — this can happen during cycling.\n\nMonitor regularly using the chemical test kit.", "OK");
            }
        }
    }
    private async void WarningTDSTapped(object sender, EventArgs e)
    {
        Console.WriteLine("WarningTDSapped triggered");

        if (TDSRange) // tds is too high
        {
            if (_mode == Mode.Maintenance)
            {
                await DisplayAlert("Warning", "Total Dissolved Solids is too high! Check Chemicals and perform a 25% water change.\n\nTip: If tank has been recently cleaned, reduce amount of food given.", "OK");
            }
            else if (_mode == Mode.Cycling)
            {
                await DisplayAlert("Cycling Tip", "Total Dissolved Solids is above the normal range — this can happen during cycling especially when ammonia and nitrites are spiking.\n\nMonitor regularly using the chemical test kit.", "OK");
            }
        }
    }
    private async void WarningCondTapped(object sender, EventArgs e)
    {
        Console.WriteLine("WarningCondTapped triggered");

        if (ConductivityRange) // tds is too high
        {
            if (_mode == Mode.Maintenance)
            {
                await DisplayAlert("Warning", "Electrical conductivity of the water is too high! Check Chemicals and perform a 25% water change.\n\nTip: If tank has been recently cleaned, reduce amount of food given.", "OK");
            }
            else if (_mode == Mode.Cycling)
            {
                await DisplayAlert("Cycling Tip", "Electrical conductivity of the water is above the normal range — this can happen during cycling especially when ammonia and nitrites are spiking.\n\nMonitor regularly using the chemical test kit.", "OK");
            }
        }
    }
    private async void WarningTempTapped(object sender, EventArgs e)
    {
        Console.WriteLine("WarningTempTapped triggered");

        if (TempRange[0]) // temperature is too low
        {
            await DisplayAlert("Warning", "Water temperature is too low! Check heater is functioning and make sure tank is not in a drafty location.", "OK");
        }
        else if (TempRange[1]) // temperature is too high
        {
            await DisplayAlert("Warning", "Water temperature is too high! Check heater is functioning correctly and make sure tank is not in direct sunlight.", "OK");
        }
        
    }



    private void DisplayModeInfo()
    {
        string message = _mode switch
        {
            //Mode.Cycling => "Cycling mode is active.",
            Mode.Maintenance => "Maintenance mode is active.",
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
        else if (_mode == Mode.Maintenance)
        {
            if ((DateTime.Now - LastPopupTime).TotalDays >= 21)
            {
                DisplayAlert("Reminder", "It's time to do a 25% water change.", "OK");
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

}