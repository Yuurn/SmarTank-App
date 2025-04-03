using Plugin.BluetoothClassic;
using Plugin.BluetoothClassic.Abstractions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.IO;
using System.Collections;

namespace SmarTank
{
    public partial class NewPage1 : ContentPage
    {
        BluetoothClient _bluetoothClient;
        BluetoothDeviceInfo _deviceInfo;
        System.IO.Stream _stream;

        private IDispatcherTimer _connectionTimer;
        private int _reconnectAttempts = 0;

        public NewPage1()
        {
            InitializeComponent();
            ConnectToBluetoothAsync();
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
                    await DisplayAlert("GOOD", "HC-05 device found", "OK");

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
            _connectionTimer.Interval = TimeSpan.FromSeconds(5);
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

                while (_bluetoothClient.Connected)
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
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"IOException: Bluetooth connection lost: {ex.Message}");
                        Dispatcher.Dispatch(async () =>
                        {
                            await DisplayAlert("Error", $"Bluetooth connection lost: {ex.Message}", "OK");
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
                _bluetoothClient?.Dispose();
            }
        }

        private void UpdateSensorValues(string data)
        {
            Console.WriteLine($"start values");
            var values = data.Split(',');

            double ph = 6.5;
            string phS = ph.ToString();
            if (values.Length >= 4)
            {
                TemperatureLabel.Text = values[0] + " °F";
                PHLabel.Text = "90";
                TDSLabel.Text = values[2] + " ppm";
                ConductivityLabel.Text = values[3] + " μS/cm";


                if (String.Compare(values[0], (8).ToString()) > 0)  //greater than 8 for ph
                {
                    DisplayAlert("Warninig", "Too low alert, perform 25% water change. Natural Solution: Add crushed coral and increase aeration. \n"
                                + "Tip: check pH of tap water used in aquarium to ensure it’s about 7.0.", "OK");
                }
                else if (String.Compare(values[0], ph.ToString()) > 0) //less than 6.5 for ph
                {
                    DisplayAlert("Warning", "Too high alert, perform 25% water change. Natural Solution: Add driftwood or peat moss."
                                + "Tip: check pH of tap water used in aquarium to ensure it’s about 7.0.", "OK");
                }










            }
            else
            {
                TemperatureLabel.Text = "Error";
                PHLabel.Text = "Error";
                TDSLabel.Text = "Error";
                ConductivityLabel.Text = "Error";
            }
        }
    }
}