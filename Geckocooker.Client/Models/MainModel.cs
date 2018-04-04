using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Geckocooker.Client.Models
{
    internal class MainModel : INotifyPropertyChanged
    {
        private DeviceClient deviceClient;
        private const string IotHubUri = "geckocookerdemo.azure-devices.net";
        private const string DeviceId = "geckocooker-sensor01";
        private const string DeviceKey = "REDACTED";

        private const double RoomTemperature = 25.0F;
        private const double HeatChangeRatePerSecond = 0.2F;
        private const double HeatUncertaintyRangeFraction = 0.05;

        Random random = new Random();
        private DispatcherTimer timer = new DispatcherTimer();

        private double _actualTemperature = RoomTemperature;
        public double MeasuredTemperature
        {
            get
            {
                double randomPosition = random.NextDouble();
                double positionWithinInterval = HeatUncertaintyRangeFraction * randomPosition;
                return _actualTemperature * (1 + positionWithinInterval - (HeatUncertaintyRangeFraction / 2));
            }
        }

        private double _targetTemperature = RoomTemperature + 3;
        public double TargetTemperature
        {
            get
            {
                return _targetTemperature;
            }
            set
            {
                if (_targetTemperature != value)
                {
                    _targetTemperature = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Status;
        public string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainModel()
        {
            deviceClient = DeviceClient.Create(
                IotHubUri,
                new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey), 
                TransportType.Http1);

            ListenForMessages();

            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void ListenForMessages()
        {
            while (true)
            {
                var message = await deviceClient.ReceiveAsync();
                if (message == null)
                {
                    await Task.Delay(1000);
                    continue;
                }

                try
                {
                    var messageString = Encoding.UTF8.GetString(message.GetBytes());
                    var targetTemperature = double.Parse(messageString, CultureInfo.InvariantCulture);
                    TargetTemperature = targetTemperature;
                }
                catch
                { }
                finally
                {
                    await deviceClient.CompleteAsync(message);
                }
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
            // Simulate heat change
            if (TargetTemperature != _actualTemperature)
            {
                if (Math.Abs(TargetTemperature - _actualTemperature) <= HeatChangeRatePerSecond)
                {
                    _actualTemperature = TargetTemperature;
                    Status = "";
                }
                else
                {
                    if (TargetTemperature > _actualTemperature)
                    {
                        _actualTemperature += HeatChangeRatePerSecond;
                        Status = "Heating";
                    }
                    else
                    {
                        _actualTemperature -= HeatChangeRatePerSecond;
                        Status = "Cooling";
                    }
                }
            }
            else
            {
                Status = "";
            }

            // Notify listeners that the measured temperature has changed
            // Because it is slightly random, we're sure to have a change
            OnPropertyChangedExplicit(nameof(MeasuredTemperature));

            // Push message to IoT Hub (Event Hubs)
            await PushTemperatureToCloud(MeasuredTemperature);
        }

        private async Task PushTemperatureToCloud(double measuredTemperature)
        {
            var messageString = JsonConvert.SerializeObject(new TemperatureDataPoint() { PartitionKey = DeviceId, Timestamp = DateTimeOffset.Now, TemperatureInCelsius = MeasuredTemperature });
            var message = new Message(Encoding.UTF8.GetBytes(messageString));
            try
            {
                await deviceClient.SendEventAsync(message);
            }
            catch { }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string caller = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
        private void OnPropertyChangedExplicit(string caller)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
        #endregion
    }
}
