﻿using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net;
using SocialNetwork.web.Models.Profile;
using SocialNetwork.core.Models;
using System.Collections.Generic;

namespace SocialNetwork.web.Controllers
{

    public class ProfilesController : Controller
    {
        readonly Uri UriAccount = new Uri("http://localhost:2001/");

        // GET: Profiles
        [HttpGet]
        [Route(Name ="Index")]
        public async Task<ActionResult> Index()
        {
            try
            {
                string acess_token = Session["access_token"]?.ToString();
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = UriAccount;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                    var response = await client.GetAsync("api/Profiles/getProfileByAccount");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent == "null")
                        {
                            return RedirectToAction("Create", "Profiles");
                        }

                        Profile profile = JsonConvert.DeserializeObject<Profile>(responseContent);
                        ProfileViewModel profileView = new ProfileViewModel()
                        {
                            Id = profile.Id,
                            FirstName = profile.FirstName,
                            LastName = profile.LastName,
                            BirthDate  = profile.BirthDate,
                            PictureUrl = profile.PictureUrl,
                            AccountId = profile.AccountId
                        };

                        return View(profileView);
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Account");
                    }
                    

                    return View("Error");
                }
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
                return View("Error");
            }
            
        }

        [HttpGet]
        [Route(Name = "Details")]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = UriAccount;

                    var response = await client.GetAsync(string.Format("api/Profiles/getProfile/{0}", id));
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent == "null")
                        {
                            return RedirectToAction("NotFound", "Profiles");
                        }

                        Profile profile = JsonConvert.DeserializeObject<Profile>(responseContent);
                        ProfileViewModel profileView = new ProfileViewModel()
                        {
                            Id = profile.Id,
                            FirstName = profile.FirstName,
                            LastName = profile.LastName,
                            BirthDate = profile.BirthDate,
                            PictureUrl = profile.PictureUrl,
                            AccountId = profile.AccountId,
                            IsFriend = false
                        };
                        /*
                        if (profile.Friends != null)
                        { 
                            foreach (Profile item in profile.Friends)
                            {
                                profileView.Friends.Add(new ProfileViewModel()
                                {
                                    Id = item.Id,
                                    FirstName = item.FirstName,
                                    LastName = item.LastName,
                                    BirthDate = item.BirthDate,
                                    PictureUrl = item.PictureUrl,
                                    AccountId = item.AccountId
                                });
                            }
                            profileView.IsFriend = (profileView.Friends.Where(friend => friend.Id == id) != null);
                        }

                        
                    */
                        return View(profileView);
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Account");
                    }


                    return View("Error");
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return View("Error");
            }

        }

        [HttpGet]
        [Route(Name = "ListProfiles")]
        public async Task<ActionResult> ListProfiles()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = UriAccount;

                    var response = await client.GetAsync("api/Profiles/listProfiles");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent == "null")
                        {
                            return RedirectToAction("Error");
                        }

                        ICollection<Profile> profiles = JsonConvert.DeserializeObject<ICollection<Profile>>(responseContent);
                        ICollection<ProfileViewModel> profileViewModel = new List<ProfileViewModel>();
                        foreach(Profile profile in profiles)
                        {
                            ProfileViewModel profileVM = new ProfileViewModel()
                            {
                                Id = profile.Id,
                                AccountId = profile.AccountId,
                                FirstName = profile.FirstName,
                                LastName = profile.LastName,
                                BirthDate = profile.BirthDate,
                                PictureUrl = profile.PictureUrl

                            };

                            profileViewModel.Add(profileVM);
                        }

                        return View(profileViewModel);
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Account");
                    }


                    return View("Error");
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return View("Error");
            }

        }



        [HttpGet]
        public ActionResult NotFound()
        {
            return View("NotFound");
        }

        // GET: Profiles/Create
        [HttpGet]
        public ActionResult Create()
        {
            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Register", "Account");
            }
            return View();
        }

        // POST: Profiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Acount");
            }

            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    client.BaseAddress = UriAccount;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                    content.Add(new StringContent(JsonConvert.SerializeObject(model)));

                    if (Request.Files.Count > 0)
                    {
                        byte[] fileBytes;

                        using (var inputStream = Request.Files[0].InputStream)
                        {
                            var memoryStream = inputStream as MemoryStream;

                            if (memoryStream == null)
                            {
                                memoryStream = new MemoryStream();
                                inputStream.CopyTo(memoryStream);
                            }

                            fileBytes = memoryStream.ToArray();
                        }
                        var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = Request.Files[0].FileName.Split('\\').Last()
                        };

                        content.Add(fileContent);
                        
                    }

                    var response = await client.PostAsync("api/Profiles/Create", content);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    ProfileViewModel profile = JsonConvert.DeserializeObject<ProfileViewModel>(responseContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "Profiles");
                    }
                    else
                    {
                        return View("Error");
                    }

                }
            }

        }

        // GET: Profiles/Edit/5
        [HttpGet]
        public ActionResult Update(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(model);
        }

        // POST: Profiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Update")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateAction(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Acount");
            }

            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    client.BaseAddress = UriAccount;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                    content.Add(new StringContent(JsonConvert.SerializeObject(model)));

                    if (Request.Files.Count > 0)
                    {
                        byte[] fileBytes;

                        using (var inputStream = Request.Files[0].InputStream)
                        {
                            var memoryStream = inputStream as MemoryStream;

                            if (memoryStream == null)
                            {
                                memoryStream = new MemoryStream();
                                inputStream.CopyTo(memoryStream);
                            }

                            fileBytes = memoryStream.ToArray();
                        }
                        var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = Request.Files[0].FileName.Split('\\').Last()
                        };

                        content.Add(fileContent);

                    }

                    var response = await client.PutAsync("api/Profiles/Update", content);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    ProfileViewModel profile = JsonConvert.DeserializeObject<ProfileViewModel>(responseContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "Profiles");
                    }
                    else
                    {
                        return View("Error");
                    }
                }
            }
        }

        // GET: Profiles/Delete/5
        public ActionResult Delete(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(model);
        }

        // POST: Profiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAction(int id)
        {
            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Acount");
            }

            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    client.BaseAddress = UriAccount;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                    var response = await client.DeleteAsync("api/Profiles/Delete");

                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Logout", "Account");
                    }
                    else
                    {
                        return View("Error");
                    }

                }
            }
        }

        [HttpGet, ActionName("AddFriend")]
        public ActionResult AddFriendGetAction()
        {
            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }


        [HttpPost, ActionName("AddFriend")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddFriendAction(ProfileViewModel model)
        {
            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Account");
            }

            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    client.BaseAddress = UriAccount;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                    content.Add(new StringContent(JsonConvert.SerializeObject(model)));

                    var response = await client.PostAsync("api/Profiles/AddFriend", content);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    ProfileViewModel profile = JsonConvert.DeserializeObject<ProfileViewModel>(responseContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Details", "Profiles");
                    }
                    else
                    {
                        return View("Error");
                    }

                }
            }
            
        }

        [HttpGet, ActionName("RemoveFriend")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveFriendAction(ProfileViewModel model)
        {
            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Acount");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = UriAccount;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                var response = await client.GetAsync("api/Profiles/RemoveFriend");
                if (response.IsSuccessStatusCode)
                {
                    return View("Details", "Profiles");
                }
                else
                {
                    return View("Error");
                }
            }
        }

        /*protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        */
    }
}
