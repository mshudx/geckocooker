using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geckocooker.DeviceRegistrator
{
    class Program
    {
        private static RegistryManager registryManager;
        private const string iotHubConnectionString = "HostName=geckocooker.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4fsJLkA4pSyUej5KHpoEP1LV/KpRv/G7Dp8ovLzh+wg=";
        private const string deviceId = "geckocooker-sensor01";

        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            RegisterDevice().Wait();
            Console.ReadLine();
        }

        private static async Task RegisterDevice()
        {
            Console.WriteLine($"Registering {deviceId} in Azure IoT Hub...");
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            Console.WriteLine($"Device key for {deviceId}:\r\n{device.Authentication.SymmetricKey.PrimaryKey}");
        }
    }
}
