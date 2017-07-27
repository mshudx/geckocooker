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
        private const string AzureStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=geckocookerdemo;AccountKey=R0Qot/+fbrlJXSpjHReEg/Wb14VfIX93baK6FalpjW7qwrybq50XANdQgMZfCn46Ru+6CLxJxhZHOJr3ekLBnQ==;EndpointSuffix=core.windows.net";
        private const string IotHubConnectionString = "HostName=geckocookerdemo.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=rtdqip0Vr6+UKpBCbmC6qZpLvu/4N/cveH5MsIqj/6Y=";
        private const string DeviceId = "geckocooker-sensor01";

        private const string RawTableName = "geckocookerraw";
        private const string ProcessedTableName = "geckocookerprocessed";

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
            var rawTable = tableClient.GetTableReference(RawTableName);
            rawTable.DeleteIfExists();
            var processedTable = tableClient.GetTableReference(ProcessedTableName);
            processedTable.DeleteIfExists();

            ViewBag.Message = "Tables deleted. Wait 1-2 minutes and restart streaming jobs.";
            return View("Index");
        }
    }
}