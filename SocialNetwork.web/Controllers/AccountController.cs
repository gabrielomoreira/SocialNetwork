using Newtonsoft.Json.Linq;
using SocialNetwork.web.Models.Account;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
            if (string.IsNullOrEmpty(Session["access_token"]?.ToString()))
            {
                return View();
            }
            return RedirectToAction("Index", "Profiles");
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
                    return RedirectToAction("Login", "Account");
                }
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (string.IsNullOrEmpty(Session["access_token"]?.ToString()))
            {
                return View();
            }
            return RedirectToAction("Index", "Profiles");
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

                        Session.Add("access_token", tokenData["access_token"]);

                        return RedirectToAction("Index", "Profiles");
                    }
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        return View();
                    }
                    return View("Error");
                }
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Logoff(LoginViewModel model)
        {
            try
            {
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
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Session["access_token"]?.ToString()}");

                    using (var requestContent = new FormUrlEncodedContent(data))
                    {
                        var response = await client.PostAsync("api/Account/Logout", null);

                        if (response.IsSuccessStatusCode)
                        {
                            Session.Remove("access_token");
                            return RedirectToAction("Login", "Account");
                        }
                        return View("Error");
                    }
                }
            }
            catch
            {
                return View("Error");
            }

        }

    }
}