using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;

namespace SmarTank
{
    public class SensorViewModel : INotifyPropertyChanged
    {
        private IBluetoothLE _bluetooth;
        private IAdapter _adapter;
        private IDevice _device;
        private ICharacteristic _characteristic;

        private double _phReading;
        public double PHReading
        {
            get => _phReading;
            set { _phReading = value; OnPropertyChanged(); }
        }

        private double _tdsReading;
        public double TDSReading
        {
            get => _tdsReading;
            set { _tdsReading = value; OnPropertyChanged(); }
        }

        private double _temperature;
        public double Temperature
        {
            get => _temperature;
            set { _temperature = value; OnPropertyChanged(); }
        }

        private double _conductivity;
        public double Conductivity
        {
            get => _conductivity;
            set { _conductivity = value; OnPropertyChanged(); }
        }

        private string _color;
        public string ColorSensor
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }

        public SensorViewModel()
        {
            _bluetooth = CrossBluetoothLE.Current;
            _adapter = _bluetooth.Adapter;
            ConnectToArduino();
        }

        private async void ConnectToArduino()
        {
            try
            {
                _device = await _adapter.ConnectToKnownDeviceAsync(Guid.Parse("00001101-0000-1000-8000-00805F9B34FB")); // HC-05/06 GUID
                var service = await _device.GetServiceAsync(Guid.Parse("00001101-0000-1000-8000-00805F9B34FB"));
                _characteristic = await service.GetCharacteristicAsync(Guid.Parse("00001101-0000-1000-8000-00805F9B34FB"));

                if (_characteristic.CanUpdate)
                {
                    _characteristic.ValueUpdated += OnDataReceived;
                    await _characteristic.StartUpdatesAsync();
                }
            }
            catch (DeviceConnectionException ex)
            {
                Console.WriteLine($"Bluetooth Connection Error: {ex.Message}");
            }
        }

        private void OnDataReceived(object sender, CharacteristicUpdatedEventArgs e)
        {
            string receivedData = Encoding.UTF8.GetString(e.Characteristic.Value);
            ParseSensorData(receivedData);
        }

        private void ParseSensorData(string data)
        {
            try
            {
                var values = data.Split(',');
                if (values.Length >= 4)
                {
                    PHReading = double.Parse(values[0]);
                    TDSReading = double.Parse(values[1]);
                    Temperature = double.Parse(values[2]);
                    Conductivity = double.Parse(values[3]);
                    ColorSensor = values[3];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing sensor data: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}