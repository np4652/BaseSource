using BaseSource.AppCode.Helper;
using BaseSource.AppCode.Service;
using BaseSource.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BaseSource.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDapper _dapper;
        private readonly string APIKey;
        private readonly Dictionary<string, string> APIs = new Dictionary<string, string>(){
            {"VerifyContact", "https://waba.360dialog.io/v1/contacts"},
            {"Message", "https://waba.360dialog.io/v1/messages/"}
        };
        public HomeController(ILogger<HomeController> logger, IDapper dapper)
        {
            _logger = logger;
            _dapper = dapper;
            APIKey = "zoyiIqxWpGHuiv0HPGE6yPnXAK";
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("initialform2")]
        public IActionResult initialform2()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> BBPSOutlets(int lastCount = 0)
        {
            var response = new Response<List<BBPSOutlet>>();
            try
            {
                var sqlQuery = @"select Top 1000 _Id id,_AgentName AgentName,_ShopName ShopName,_Mobile Mobile,_Address Address,_PinCode PinCode,_City City,_State State from tbl_NearestBBPOutlet(nolock)";
                var result = await _dapper.GetAll<BBPSOutlet>(sqlQuery, new DynamicParameters(), System.Data.CommandType.Text);
                if (result != null)
                {
                    response = new Response<List<BBPSOutlet>>
                    {
                        StatusCode = 1,
                        Data = result.ToList()
                    };
                }
            }
            catch (Exception ex)
            {
                response = new Response<List<BBPSOutlet>>
                {
                    StatusCode = 1,
                    Status = ex.Message
                };
            }
            return PartialView(response);
        }

        public IActionResult InitialForm()
        {
            return PartialView();
        }

        public async Task<IActionResult> PostDataAsync(string _namespace, string name, List<string> head, List<string> body, string apiKey)
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
            return Json(result);
        }

        public async Task<IActionResult> PostAlternateDataAsync(string _namespace, string name, List<string> head, List<string> body, string apiKey)
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
            return Json(result);
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
            string reponse = await APIRequest.O.PostJsonData(APIs["VerifyContact"], contactRequestParam, headers).ConfigureAwait(false);
            var contactResponse = JsonConvert.DeserializeObject<ContactRequestResponse>(reponse);
            if (contactResponse != null && contactResponse.contacts.Count > 0)
            {
                result = contactResponse.contacts.FirstOrDefault().status.Equals("valid", StringComparison.OrdinalIgnoreCase);

            }
            return result;
        }

        private async Task<MessageResponse> SendMessageAsync(List<string> head, List<string> body, string contact, string _namespace, string name, string apiKey = "")
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
            var response = JsonConvert.DeserializeObject<MessageResponse>(reponse);
            return response;
        }
    }
}
