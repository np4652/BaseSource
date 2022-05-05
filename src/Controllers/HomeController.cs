using BaseSource.AppCode;
using BaseSource.AppCode.Helper;
using BaseSource.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BaseSource.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBL _bl;
        public HomeController(IBL bl)
        {
            _bl = bl;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult InitialForm()
        {
            return PartialView();
        }

        [Route("initialform2")]
        public IActionResult initialform2()
        {
            return View();
        }


        [Route("initialform3")]
        public IActionResult initialform3()
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
                var result = await _bl.BBPSOutletDetail(100);
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

        public async Task<IActionResult> PostDataAsync(string _namespace, string name, List<string> head, List<string> body, string apiKey)
        {
            var result = new List<MessageResponseList>();
            result = await _bl.PostDataAsync(_namespace, name, head, body, apiKey);
            return Json(result ?? new List<MessageResponseList>());
        }

        public async Task<IActionResult> PostAlternateDataAsync(string _namespace, string name, List<string> head, List<string> body, string apiKey)
        {
            var result = new List<MessageResponseList>();
            result = await _bl.PostAlternateDataAsync(_namespace, name, head, body, apiKey);
            return Json(result);
        }

        public async Task<IActionResult> PostInitialForm3Async(InitialForm3 request, List<Button> buttons)
        {
            List<dynamic> b = new List<dynamic>();
            buttons.ForEach(x =>
            {
                if (x.btn_type?.ToUpper() == "CALL")
                {
                    b.Add(new
                    {
                        x.btn_type,
                        x.display_txt,
                        x.call
                    });
                }
                else if (x.btn_type?.ToUpper() == "URL")
                {
                    b.Add(new
                    {
                        x.btn_type,
                        x.display_txt,
                        x.url
                    });
                }
                else if (x.btn_type?.ToUpper() == "REPLY")
                {
                    b.Add(new
                    {
                        x.btn_type,
                        x.display_txt
                    });
                }
            });
            request.buttons = b;
            request.messagetype = "BTNMSG";
            request.requestid = AppUtility.O.RandomString(10);
            request.buttons = b;
            var result = await _bl.PostInitialForm3DataAsync(request);
            return Ok(result);
        }
    }
}