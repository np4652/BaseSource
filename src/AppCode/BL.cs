using BaseSource.AppCode.Helper;
using BaseSource.AppCode.Service;
using BaseSource.Models;
using Dapper;
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
        private readonly IDapper _dapper;
        private readonly Dictionary<string, string> APIs = new Dictionary<string, string>(){
            {"VerifyContact", "https://waba.360dialog.io/v1/contacts"},
            {"Message", "https://waba.360dialog.io/v1/messages/"}
        };
        public BL(IDapper dapper)
        {
            _dapper = dapper;
            APIKey = "zoyiIqxWpGHuiv0HPGE6yPnXAK";
        }
        public async Task<List<MessageResponseList>> PostDataAsync(string _namespace, string name, List<string> head, List<string> body, string apiKey)
        {
            var result = new List<MessageResponseList>();
            var sqlQuery = @"IF (Select count(1) from tbl_NearestBBPOutlet(nolock) where ISNULL(_IsSent,0) = 0) = 0 
                                    update tbl_NearestBBPOutlet set _IsSent = 0
                             select Top 1000 _Id id,_AgentName AgentName,_ShopName ShopName,_Mobile Mobile,_Address Address,_PinCode PinCode,_City City,_State State from tbl_NearestBBPOutlet(nolock) where ISNULL(_IsSent,0) = 0";
            var outlets = await _dapper.GetAll<BBPSOutlet>(sqlQuery, new DynamicParameters(), System.Data.CommandType.Text);
            sqlQuery = @"update top (1000) tbl_NearestBBPOutlet set _IsSent = 1 where ISNULL(_IsSent,0) = 0";
            _dapper.Update<int>(sqlQuery, new DynamicParameters(), System.Data.CommandType.Text);
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
            var sqlQuery = @"IF (Select count(1) from tbl_BBPOutletMobiles(nolock) where ISNULL(_IsSent,0) = 0) = 0 
                             	update tbl_BBPOutletMobiles set _IsSent = 0
                             select Top 1000 _Id id,_Mobile Mobile from tbl_BBPOutletMobiles(nolock) where ISNULL(_IsSent,0) = 0";
            var outlets = await _dapper.GetAll<BBPSOutlet>(sqlQuery, new DynamicParameters(), System.Data.CommandType.Text);
            sqlQuery = @"update top (1000) tbl_BBPOutletMobiles set _IsSent = 1 where ISNULL(_IsSent,0) = 0";
            _dapper.Update<int>(sqlQuery, new DynamicParameters(), System.Data.CommandType.Text);
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
            //var sqlQuery = @"insert into log_Wab360dialog_VerifyResponse(_response) values(@response)";
            //_ = _dapper.Update<int>(sqlQuery, new { response }, System.Data.CommandType.Text);
            createLog(response);
            var contactResponse = JsonConvert.DeserializeObject<ContactRequestResponse>(response);
            if (contactResponse != null && contactResponse.contacts?.Count > 0)
            {
                result = contactResponse.contacts.FirstOrDefault().status.Equals("valid", StringComparison.OrdinalIgnoreCase);

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
                if (head != null)
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
                if (body != null)
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
                string reponse = await APIRequest.O.PostJsonData(APIs["Message"], contactRequestParam, headers).ConfigureAwait(false);
                response = JsonConvert.DeserializeObject<MessageResponse>(reponse);
            }
            catch (Exception ex)
            {
                createLog($"Error : {ex.Message}");
            }
            return response;
        }

        private async Task createLog(string response)
        {
            var sqlQuery = @"insert into log_Wab360dialog_VerifyResponse(_response) values(@response)";
            _ = await _dapper.Update<int>(sqlQuery, new { response }, System.Data.CommandType.Text);
        }
    }
}
