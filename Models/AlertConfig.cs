using Microsoft.WindowsAzure.Storage.Table;

namespace tempaast.alerting.models
{
    public class AlertConfigEntity : TableEntity {
        public string firstName {get;set;}
        public string lastName {get;set;}
        public string phoneNumber {get;set;}
       
    }

    public class AlertConfig {
        public string firstName {get;set;}
        public string lastName {get;set;}
        public string phoneNumber {get;set;}
    }
}
