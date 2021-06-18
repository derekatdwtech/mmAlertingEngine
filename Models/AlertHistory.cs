using Microsoft.WindowsAzure.Storage.Table;

namespace tempaast.alerting.models {
    public class AlertHistory : TableEntity {
        public string recipients {get; set;}
        public string details {get; set;}
        public string sendTime {get; set;}
        
    }
}