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
        private const string IotHubConnectionString = "HostName=geckocooker.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4fsJLkA4pSyUej5KHpoEP1LV/KpRv/G7Dp8ovLzh+wg=";
        private const string DeviceId = "geckocooker-sensor01";

        private static RegistryManager registryManager;

        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(IotHubConnectionString);
            RegisterDevice().Wait();
            Console.ReadLine();
        }

        private static async Task RegisterDevice()
        {
            Console.WriteLine($"Registering {DeviceId} in Azure IoT Hub...");
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(DeviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(DeviceId);
            }
            Console.WriteLine($"Device key for {DeviceId}:\r\n{device.Authentication.SymmetricKey.PrimaryKey}");
        }
    }
}
