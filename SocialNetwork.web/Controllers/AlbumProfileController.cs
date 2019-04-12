using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using SocialNetwork.web.Models.Profile;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using SocialNetwork.core.Models;

namespace SocialNetwork.web.Controllers
{
    public class AlbumProfileController : Controller
    {
        readonly Uri UriAccount = new Uri("http://localhost:2001/");

        // GET: Album
        public async Task<ActionResult> Index()
        {
            try
            {
                string acess_token = Session["access_token"]?.ToString();
                if (string.IsNullOrEmpty(acess_token))
                {
                    return RedirectToAction("Login", "Account");
                }
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = UriAccount;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                    var response = await client.GetAsync("api/Profiles/Album/getAlbumByAccount");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        Albuns albuns = JsonConvert.DeserializeObject<Albuns>(responseContent);
                        AlbumViewModel albumViewModel = new AlbumViewModel()
                        {
                            Id = albuns.Id,
                            Description = albuns.Description
                        };

                        foreach(Picture picture in albuns.Pictures)
                        {
                            albumViewModel.Pictures.Add(new PictureViewModel {
                                Id = picture.Id,
                                Description = picture.Description,
                                PictureUrl = picture.PictureUrl
                            });
                        }

                        return View(albumViewModel);
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    return RedirectToAction("Error");
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return RedirectToAction("Error");
            }

        }

        // GET: Album/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            try
            {
                string acess_token = Session["access_token"]?.ToString();
                if (string.IsNullOrEmpty(acess_token))
                {
                    return RedirectToAction("Login", "Account");
                }

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = UriAccount;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                    var response = await client.GetAsync(string.Format("api/Profiles/Album/getAlbum/{0}", id));
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        Albuns albuns = JsonConvert.DeserializeObject<Albuns>(responseContent);
                        AlbumViewModel albumViewModel = new AlbumViewModel()
                        {
                            Id = albuns.Id,
                            Description = albuns.Description
                        };

                        foreach (Picture picture in albuns.Pictures)
                        {
                            albumViewModel.Pictures.Add(new PictureViewModel
                            {
                                Id = picture.Id,
                                Description = picture.Description,
                                PictureUrl = picture.PictureUrl
                            });
                        }

                        return View(albumViewModel);
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    return RedirectToAction("Error");
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return RedirectToAction("Error");
            }
            
        }

        // GET: Album/Create
        [HttpGet]
        public ActionResult Create()
        {
            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AlbumViewModel model)
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

                    var response = await client.PostAsync("api/Profiles/Album/Create", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "AlbumProfile");
                    }
                    else
                    {
                        return RedirectToAction("Error");
                    }

                }
            }
        }

        // GET: Album/Update/5
        [HttpGet]
        public ActionResult Update(AlbumViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(model);
        }

        // POST: Album/Update/5
        [HttpPost, ActionName("Update")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateAction(AlbumViewModel model)
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

                    var response = await client.PutAsync("api/Profiles/Album/Update", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "AlbumProfile");
                    }
                    else
                    {
                        return RedirectToAction("Error");
                    }
                }
            }
        }

        // GET: Album/Delete/5
        public ActionResult Delete(AlbumViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(model);
        }

        // POST: Album/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(AlbumViewModel model)
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

                    var response = await client.PutAsync("api/Profiles/Album/Delete", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "AlbumProfile");
                    }
                    else
                    {
                        return RedirectToAction("Error");
                    }
                }
            }

        }


    }
}
