using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using SocialNetwork.core.Models;
using SocialNetwork.web.Models;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace SocialNetwork.web.Controllers
{
    [Authorize]
    public class ProfilesController : Controller
    {
        readonly Uri UriAccount = new Uri("http://localhost:2001/");

        // GET: Profiles
        [HttpGet]
        public async Task<ActionResult> Index(string AccountID)
        {
            try
            {
                string acess_token = Session["access_token"]?.ToString();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = UriAccount;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Baerer", $"{acess_token}");

                    var response = await client.GetAsync(string.Concat("api/Profiles/getByIdAccount/{0}", AccountID));
                    var resultContent = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(resultContent))
                    {
                        return View("Create", "Profile");
                    }

                    return View(response);
                }
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
                return View("Error");
            }
            
        }

        // GET: Profiles/Create
       /* public ActionResult Create()
        {
            ProfileModelView model = new ProfileModelView();
            model.AccountId = "";

            return View(model);
        }*/

        // POST: Profiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProfileModelView model)
        {
            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Acount", null);
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    client.BaseAddress = UriAccount;
                    client.DefaultRequestHeaders.Accept.Clear();

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Baerer", $"{acess_token}");

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

                        var response = await client.PostAsync("api/Profiles/Create", content);

                        var responseContent = await response.Content.ReadAsStringAsync();
                        ProfileModelView profile = JsonConvert.DeserializeObject<ProfileModelView>(responseContent);

                        if (response.IsSuccessStatusCode)
                        {
                            return RedirectToAction("Index", "Profile", profile);
                        }
                        else
                        {
                            return View("Error");
                        }
                    }

                }
            }

                return View(model);
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
