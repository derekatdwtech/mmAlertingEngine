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

            if (probeConfig == null)
            {
                log.LogError($"Failed to obtain Configuration for probe {probeId}");
            }

            if (alertConfig.Count > 0)
            {
                List<string> recipients = new List<string>();
                foreach (var config in alertConfig)
                {
                    log.LogInformation($"Sending alert to {config.phoneNumber}");
                    string message = $"Your probe {probeConfig.nickname} is reading at {temp.temperature.c}C/{temp.temperature.f}F. This is out of bounds of your defined threshold of {probeConfig.tempThresholdInCelcius}C. ";

                    // Setup Table Storage for Alert History
                    recipients.Add(config.phoneNumber);
                    try
                    {
                        _twilio.SendMessage(config.phoneNumber, message);
                        log.LogInformation($"Successfully delivered alert to {config.phoneNumber}");
                    }
                    catch (Exception e)
                    {
                        log.LogError($"Failed to deliver message: {e.Message}");
                    }

                }
                // Store Alert History
                Guid rowKey = Guid.NewGuid();

                AlertHistory history = new AlertHistory()
                {
                    PartitionKey = temp.user_id,
                    RowKey = rowKey.ToString(),
                    recipients = JsonConvert.SerializeObject(recipients),
                    details = myQueueItem,
                    sendTime = temp.time
                };
                StoreAlertHistory(history);
            }
            else
            {
                log.LogWarning($"No alert configs were found for probe {probeId}");
            }

        }

        private List<AlertConfigEntity> GetAlertConfig(ILogger log, string probeId)
        {
            AzureTableStorage<AlertConfigEntity> _client = new AzureTableStorage<AlertConfigEntity>(Environment.GetEnvironmentVariable("meatmonitorqueue_STORAGE"), Environment.GetEnvironmentVariable("AlertConfigTable"));
            var query = new TableQuery<AlertConfigEntity>().Where(
                    TableQuery.GenerateFilterCondition("probe_id", QueryComparisons.Equal, probeId)
            );

            return _client.GetMany(query).Result;
        }

        private List<ProbeConfig> GetProbeConfig(ILogger log, string probeId)
        {
            AzureTableStorage<ProbeConfig> _client = new AzureTableStorage<ProbeConfig>(Environment.GetEnvironmentVariable("meatmonitorqueue_STORAGE"), Environment.GetEnvironmentVariable("ProbeConfigTable"));
            var query = new TableQuery<ProbeConfig>().Where(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, probeId)
            );

            return _client.GetMany(query).Result;
        }

        private async void StoreAlertHistory(AlertHistory alertHistory)
        {
            AzureTableStorage<AlertHistory> _client = new AzureTableStorage<AlertHistory>(Environment.GetEnvironmentVariable("meatmonitorqueue_STORAGE"), Environment.GetEnvironmentVariable("AlertHistoryTable"));
            await _client.InsertOrUpdateAsync(alertHistory);
        }
    }
}
