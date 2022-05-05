using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace BaseSource.AppCode.Helper
{
    public class APIRequest
    {
        public static APIRequest O { get { return Instance.Value; } }
        private static Lazy<APIRequest> Instance = new Lazy<APIRequest>(() => new APIRequest());
        private APIRequest() { }

        public async Task<string> PostJsonData(string URL, object PostData, IDictionary<string, string> headers = null)
        {
            string result = string.Empty;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
                http.Timeout = 3 * 60 * 1000;
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(PostData));
                http.Method = "POST";
                http.Accept = ContentType.application_json;
                http.ContentType = ContentType.application_json;
                http.MediaType = ContentType.application_json;
                http.ContentLength = data.Length;
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        http.Headers.Add(item.Key, item.Value);
                    }
                }
                using (Stream stream = http.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                WebResponse response = await http.GetResponseAsync().ConfigureAwait(false);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = await sr.ReadToEndAsync().ConfigureAwait(false);
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }


        public async Task<HttpResponseMessage> postAsync(string url,object PostData)
        {
            var res = new HttpResponseMessage();
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(PostData), Encoding.UTF8, "application/json");
                string endpoint = url;
                using (var Response = await client.PostAsync(endpoint, content))
                {
                    res = Response;
                }
            }
            return res;
        }
    }
}
