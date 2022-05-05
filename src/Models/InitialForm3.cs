using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseSource.Models
{
    public class Button
    {
        public string btn_type { get; set; }
        public string display_txt { get; set; }
        public string call { get; set; }
        public string url { get; set; }
    }

    public class InitialForm3
    {
        public string apiusername { get; set; }
        public string apipassword { get; set; }
        public string requestid { get; set; }
        public string jid { get; set; }
        public string messagetype { get; set; }
        public string content { get; set; }
        public string from { get; set; }
        public string titletype { get; set; }
        public string title { get; set; }
        public string footer { get; set; }
        public dynamic buttons { get; set; }
    }
}
