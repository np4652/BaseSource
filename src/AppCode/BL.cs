using BaseSource.AppCode.Helper;
using BaseSource.AppCode.Service;
using BaseSource.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseSource.AppCode
{
    public class BL : IBL
    {
        private readonly string APIKey;
        private readonly IDbContext _context;
        private readonly Dictionary<string, string> APIs = new Dictionary<string, string>(){
            {"VerifyContact", "https://waba.360dialog.io/v1/contacts"},
            {"Message", "https://waba.360dialog.io/v1/messages/"},
            {"wappsms","https://alerthub.in/wapi/v1/Send/wappsms" }
        };
        public BL(IDbContext context)
        {
            APIKey = "zoyiIqxWpGHuiv0HPGE6yPnXAK";
            _context = context;
        }

        public async Task<List<BBPSOutlet>> BBPSOutletDetail(int top)
        {
            List<BBPSOutlet> result = new List<BBPSOutlet>();
            result = await _context.BBPSOutletDetail(top);
            return result ?? new List<BBPSOutlet>();
        }
        public async Task<List<MessageResponseList>> PostDataAsync(string _namespace, string name, List<string> head, List<string> body, string apiKey)
        {
            var result = new List<MessageResponseList>();
            var outlets = await _context.BBPSOutletDetail();
            if (outlets != null)
            {
                foreach (var o in outlets)
                {
                    var IsVeryfy = await VerifyContactAsync(o.Mobile);
                    if (IsVeryfy)
                    {
                        var response = await SendMessageAsync(head, body, o.Mobile, _namespace, name);
                        result.Add(new MessageResponseList
                        {
                            contact = o.Mobile,
                            MessageResponse = response
                        });
                    }
                }
            }
            return result ?? new List<MessageResponseList>();
        }

        public async Task<List<MessageResponseList>> PostAlternateDataAsync(string _namespace, string name, List<string> head, List<string> body, string apiKey)
        {
            var result = new List<MessageResponseList>();
            var outlets = await _context.BBPOutletMobiles();
            if (outlets != null)
            {
                foreach (var o in outlets)
                {
                    var IsVeryfy = await VerifyContactAsync(o.Mobile);
                    if (IsVeryfy)
                    {
                        var response = await SendMessageAsync(head, body, o.Mobile, _namespace, name, apiKey);
                        result.Add(new MessageResponseList
                        {
                            contact = o.Mobile,
                            MessageResponse = response
                        });
                    }
                }
            }
            return result ?? new List<MessageResponseList>();
        }

        private async Task<bool> VerifyContactAsync(string contact)
        {
            bool result = false;
            try
            {
                var contactRequestParam = new ContactRequestParam
                {
                    blocking = "wait",
                    contacts = new List<string> { contact },
                    force_check = true
                };
                var headers = new Dictionary<string, string>
                {
                    {"D360-API-KEY",APIKey}
                };

                string response = await APIRequest.O.PostJsonData(APIs["VerifyContact"], contactRequestParam, headers).ConfigureAwait(false);
                _context.createLog(response);
                var contactResponse = JsonConvert.DeserializeObject<ContactRequestResponse>(response);
                if (contactResponse != null && contactResponse.contacts?.Count > 0)
                {
                    result = contactResponse.contacts.FirstOrDefault().status.Equals("valid", StringComparison.OrdinalIgnoreCase);

                }
            }
            catch (Exception ex)
            {
                _context.LogError(ex.Message, "BL-->VerifyContactAsync");
            }
            return result;
        }

        private async Task<MessageResponse> SendMessageAsync(List<string> head, List<string> body, string contact, string _namespace, string name, string apiKey = "")
        {
            var response = new MessageResponse();
            try
            {
                apiKey = !string.IsNullOrEmpty(apiKey) ? apiKey : APIKey;
                if (contact.Length == 10)
                {
                    contact = string.Concat("91", contact);
                }
                var components = new List<Component>();
                var parameters = new List<Parameter>();
                if (head != null && head.Count > 0)
                {
                    foreach (string str in head)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            parameters.Add(new Parameter
                            {
                                type = "image",
                                image = new image
                                {
                                    link = str
                                }
                            });
                        }
                    }
                    components.Add(new Component
                    {
                        type = "header",
                        parameters = parameters
                    });
                }
                if (body != null && body.Count > 0)
                {
                    parameters = new List<Parameter>();
                    foreach (string str in body)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            parameters.Add(new Parameter
                            {
                                type = "text",
                                text = str
                            });
                        }
                    }
                    components.Add(new Component
                    {
                        type = "body",
                        parameters = parameters
                    });
                }
                var contactRequestParam = new MessageRequest
                {
                    to = contact,
                    preview_url = true,
                    type = "template",
                    template = new Template
                    {
                        @namespace = _namespace,
                        language = new Language
                        {
                            policy = "deterministic",
                            code = "en"
                        },
                        name = name,
                        components = components
                    },

                };
                var headers = new Dictionary<string, string>
                {
                    {"D360-API-KEY",apiKey}
                };

                string param = JsonConvert.SerializeObject(contactRequestParam);
                string headerParam = JsonConvert.SerializeObject(headers);
                _context.createLog(string.Concat(APIs["Message"], "param :", param, "header : ", headerParam), "SendMessageAsync", "request");
                string reponse = await APIRequest.O.PostJsonData(APIs["Message"], contactRequestParam, headers).ConfigureAwait(false);
                _context.createLog(reponse, "SendMessageAsync", "response");
                response = JsonConvert.DeserializeObject<MessageResponse>(reponse);
            }
            catch (Exception ex)
            {
                _context.LogError(ex.Message, "BL-->SendMessageAsync");
            }
            return response;
        }


        public async Task<string> PostInitialForm3DataAsync(InitialForm3 request)
        {
            var result = string.Empty;
            var outlets = await _context.BBPOutletMobiles();
            if (outlets != null)
            {
                foreach (var o in outlets)
                {
                    if (o.Mobile.Length == 10)
                    {
                        o.Mobile = string.Concat("91", o.Mobile);
                    }
                    request.jid = o.Mobile;
                    string param = JsonConvert.SerializeObject(request);
                    _context.createLog(string.Concat(string.Concat(APIs["wappsms"]), " | params : ", param), "PostInitialForm3DataAsync", "request");
                    var reponse = await APIRequest.O.PostJsonData(APIs["wappsms"], request).ConfigureAwait(false);
                    await Task.Delay(5000);
                    _context.createLog(reponse, "PostInitialForm3DataAsync", "response");
                }
            }
            return result;
        }
    }
}
