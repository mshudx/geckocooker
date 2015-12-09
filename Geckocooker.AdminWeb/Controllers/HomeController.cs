using Microsoft.Azure.Devices;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Geckocooker.AdminWeb.Controllers
{
    public class HomeController : Controller
    {
        private const string AzureStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=geckocookerdata;AccountKey=lpPxAe+4zn+Q97guAXE0UVrwBNIshYK2sDLVduIKuWG4pB+JxWU/Yrv8YuPAsrhzIsomxN6eRew1oVP8p/qXgQ==;BlobEndpoint=https://geckocookerdata.blob.core.windows.net/;TableEndpoint=https://geckocookerdata.table.core.windows.net/;QueueEndpoint=https://geckocookerdata.queue.core.windows.net/;FileEndpoint=https://geckocookerdata.file.core.windows.net/";
        private const string IotHubConnectionString = "HostName=geckocooker.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4fsJLkA4pSyUej5KHpoEP1LV/KpRv/G7Dp8ovLzh+wg=";
        private const string DeviceId = "geckocooker-sensor01";

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(string targetTemperature)
        {
            // just to make sure we don't send crap and fail here instead
            double.Parse(targetTemperature, CultureInfo.InvariantCulture);

            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(IotHubConnectionString);
            var message = new Message(Encoding.UTF8.GetBytes(targetTemperature));
            await serviceClient.SendAsync(DeviceId, message);

            ViewBag.Message = "Message sent: " + targetTemperature;
            return View();
        }

        public ActionResult ClearTables()
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(AzureStorageConnectionString);
            var tableClient = cloudStorageAccount.CreateCloudTableClient();
            var rawTable = tableClient.GetTableReference("geckocookerraw");
            rawTable.DeleteIfExists();
            var processedTable = tableClient.GetTableReference("geckocookerprocessed");
            processedTable.DeleteIfExists();

            ViewBag.Message = "Tables deleted. Wait 1-2 minutes and restart streaming jobs.";
            return View("Index");
        }
    }
}