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

namespace SocialNetwork.web.Controllers
{

    public class ProfilesController : Controller
    {
        readonly Uri UriAccount = new Uri("http://localhost:2001/");

        // GET: Profiles
        [HttpGet]
        [Route(Name ="Details")]
        public async Task<ActionResult> Details()
        {
            try
            {
                string acess_token = Session["access_token"]?.ToString();
                using (var client = new HttpClient())
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
                            FirstName = profile.FirstName,
                            LastName = profile.LastName,
                            BirthDate  = profile.BirthDate,
                            PictureUrl = profile.PictureUrl
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

        // GET: Profiles/Create
        [HttpGet]
        public ActionResult Create()
        {
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
                        return RedirectToAction("Details", "Profile");
                    }
                    else
                    {
                        return View("Error");
                    }

                }
            }

        }

        // GET: Profiles/Details/5
        /*public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = await db.Profiles.FindAsync(id);
            if (profile == null)
            {
                return HttpNotFound();
            }
            return View(profile);
        }

        

        // GET: Profiles/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = await db.Profiles.FindAsync(id);
            if (profile == null)
            {
                return HttpNotFound();
            }
            return View(profile);
        }

        // POST: Profiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,FirstName,LastName,BirthDate,PictureUrl,AccountId")] Profile profile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(profile).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(profile);
        }

        // GET: Profiles/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = await db.Profiles.FindAsync(id);
            if (profile == null)
            {
                return HttpNotFound();
            }
            return View(profile);
        }

        // POST: Profiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Profile profile = await db.Profiles.FindAsync(id);
            db.Profiles.Remove(profile);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        
        protected override void Dispose(bool disposing)
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
