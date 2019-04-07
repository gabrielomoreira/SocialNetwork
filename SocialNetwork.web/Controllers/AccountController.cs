using Newtonsoft.Json.Linq;
using SocialNetwork.web.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SocialNetwork.web.Controllers
{
    public class AccountController : Controller
    {
        readonly Uri UriAccount = new Uri("http://localhost:2001/");

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = UriAccount;

                var response = await client.PostAsJsonAsync("api/Account/Register", model);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login");
                }
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var data = new Dictionary<string, string>
            {
                {"grant_type", "password" },
                {"username", model.Email },
                {"password", model.Password }
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = UriAccount;
                client.DefaultRequestHeaders.Accept.Clear();

                using (var requestContent = new FormUrlEncodedContent(data))
                {
                    var response = await client.PostAsync("Token", requestContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var tokenData = JObject.Parse(responseContent);

                        Session.Add("access_token", tokenData["acess_token"]);

                        return RedirectToAction("Index", "Home");
                    }
                    return View(model);
                }
            }

        }
    }
}