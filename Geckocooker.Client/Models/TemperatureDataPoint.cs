using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geckocooker.Client.Models
{
    public class TemperatureDataPoint
    {
        public TemperatureDataPoint()
        {
            RowKey = Guid.NewGuid().ToString();
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public double TemperatureInCelsius { get; set; }
    }
}
