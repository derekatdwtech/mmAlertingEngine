using System;
using System.Collections.Generic;
using tempaast.alerting.helpers;
using tempaast.alerting.models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;

namespace tempaast.alerting
{
    public class mmAlertingEngine
    {
        [FunctionName("mmAlertingEngine")]
        public void Run([QueueTrigger("alerts", Connection = "meatmonitorqueue_STORAGE")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"Received {myQueueItem}");
            MessageResult temp = JsonConvert.DeserializeObject<MessageResult>(myQueueItem);
            string probeId = temp.probe_id;
            var alertConfig = GetAlertConfig(log, probeId);
            var probeConfig = GetProbeConfig(log, probeId)[0];
            TwilioHelper _twilio = new TwilioHelper(log);


            foreach (var config in alertConfig)
            {
                string message = $"Your probe {probeConfig.nickname} is reading at {temp.temperature.c}C/{temp.temperature.f}F. This is out of bounds of your defined threshold of {probeConfig.tempThresholdInCelcius}C. ";
                _twilio.SendMessage(config.phoneNumber, message);
            }

        }

        private List<AlertConfigEntity> GetAlertConfig(ILogger log, string probeId)
        {
            AzureTableStorage<AlertConfigEntity> _client = new AzureTableStorage<AlertConfigEntity>(Environment.GetEnvironmentVariable("AlertConfigTable"), Environment.GetEnvironmentVariable("meatmonitorqueue_STORAGE"));
            var query = new TableQuery<AlertConfigEntity>().Where(
                    TableQuery.GenerateFilterCondition("probe_id", QueryComparisons.Equal, probeId)
            );

            return _client.GetMany(query).Result;
        }

        private List<ProbeConfig> GetProbeConfig(ILogger log, string probeId)
        {
            AzureTableStorage<ProbeConfig> _client = new AzureTableStorage<ProbeConfig>(Environment.GetEnvironmentVariable("ProbeConfigTable"), Environment.GetEnvironmentVariable("meatmonitorqueue_STORAGE"));
            var query = new TableQuery<ProbeConfig>().Where(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, probeId)
            );

            return _client.GetMany(query).Result;
        }
    }
}
