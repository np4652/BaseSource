using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseSource.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Language
    {
        public string policy { get; set; }
        public string code { get; set; }
    }

    public class Parameter
    {
        public string type { get; set; }
        public string text { get; set; }
        public image image { get; set; }
    }

    public class image
    {
        public string link { get; set; }
    }

    public class Component
    {
        public string type { get; set; }
        public List<Parameter> parameters { get; set; }
    }

    public class Template
    {
        public string @namespace { get; set; }
        public Language language { get; set; }
        public string name { get; set; }
        public List<Component> components { get; set; }
    }

    public class MessageRequest
    {
        public string to { get; set; }
        public bool preview_url { get; set; }
        public string type { get; set; }
        public Template template { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Message
    {
        public string id { get; set; }
    }

    public class ResponseMeta
    {
        public string api_status { get; set; }
        public string version { get; set; }
    }

    public class MessageResponse
    {
        public List<Message> messages { get; set; }
        public ResponseMeta meta { get; set; }
    }

}
