using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace tempaast.alerting.models {
   public class MessageResult
    {
        public string name { get; set; }
        public string probe_id { get; set; }
        public string user_id { get; set; }
        public string time { get; set; }
        public Temperature temperature { get; set; }
    }

    public class Temperature
    {
        public float f { get; set; }
        public float c { get; set; }
    }

    public class TempTableEntity : TableEntity
    {

        public TempTableEntity()
        {

        }
        public string name { get; set; }
        public string time { get; set; }
        public string temp_c { get; set; }
        public string temp_f { get; set; }
        public string user_id { get; set; }
        public string probe_id { get; set; }
    }
}