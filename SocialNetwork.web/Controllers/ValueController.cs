using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace SocialNetwork.web.Controllers
{
    public class ValueController : Controller
    {
        readonly Uri UriAccount = new Uri("http://localhost:2001/");
        // GET: Value
        public async System.Threading.Tasks.Task<ActionResult> IndexAsync()
        {
            string acess_token = Session["acess_token"]?.ToString();

            if (!string.IsNullOrEmpty(acess_token))
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = UriAccount;
                    client.DefaultRequestHeaders.Accept.Clear();

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Baerer", $"{acess_token}");

                    var response = await client.GetAsync("ApplicationId/Values");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        return RedirectToAction("Index", "Home");
                    }

                    return View("Error");
                }
            }

            return RedirectToAction("Login", "Account", null);
        }
    }
}