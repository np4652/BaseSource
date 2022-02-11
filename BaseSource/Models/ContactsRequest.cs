using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseSource.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Contact
    {
        public string input { get; set; }
        public string status { get; set; }
        public string wa_id { get; set; }
    }

    public class Meta
    {
        public string api_status { get; set; }
        public string version { get; set; }
    }

    public class ContactRequestResponse
    {
        public List<Contact> contacts { get; set; }
        public Meta meta { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ContactRequestParam
    {
        public string blocking { get; set; }
        public List<string> contacts { get; set; }
        public bool force_check { get; set; }
    }

}
