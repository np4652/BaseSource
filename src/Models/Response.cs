using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseSource.Models
{
    public class Response<T>
    {
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public T Data { get; set; }
    }
}
