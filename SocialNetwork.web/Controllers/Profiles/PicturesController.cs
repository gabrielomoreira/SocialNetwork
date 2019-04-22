#region imports
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using SocialNetwork.web.Models.Profile;
using SocialNetwork.core.Entity;
#endregion

namespace SocialNetwork.web.Controllers
{
    public class PicturesController : Controller
    {
        private readonly Uri UriAccount = new Uri("http://localhost:2001/");

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Account");
            }

            ICollection<PictureViewModel> albumViewModel = new List<PictureViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = UriAccount;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                var response = await client.GetAsync("api/AlbumProfile/Index");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    ICollection<Pictures> pictures = JsonConvert.DeserializeObject<ICollection<Pictures>>(responseContent);

                    foreach (Pictures picture in pictures)
                    {
                        albumViewModel.Add(BuildPictureViewModel(picture));
                    }

                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    return RedirectToAction("Error");
                }
                return View(albumViewModel);
            }

        }

        [HttpGet, ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(int id)
        {
            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Account");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = UriAccount;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                var response = await client.GetAsync(string.Format("api/AlbumProfile/SearchByIMG/{0}", id));
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Pictures picture = JsonConvert.DeserializeObject<Pictures>(responseContent);
                    PictureViewModel pictureVM = BuildPictureViewModel(picture);

                    // para verificar se é o perfil da conta
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");
                    var responseAccount = await client.GetAsync("api/Profiles/getProfileByAccount");

                    if (!responseAccount.IsSuccessStatusCode)
                    {
                        throw new Exception(responseAccount.StatusCode.ToString());
                    }

                    var responseContentAccountProfile = await responseAccount.Content.ReadAsStringAsync();

                    Profiles profileAccount = JsonConvert.DeserializeObject<Profiles>(responseContentAccountProfile);

                    pictureVM.PermissionRemove = profileAccount.Album.Where(pic => pic.Id == id).Count() > 0;

                    return View(pictureVM);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }
                return RedirectToAction("Error");

            }

        }

        [HttpGet]
        public async Task<ActionResult> ListPictures(int id)
        {
            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Account");
            }

            ICollection<PictureViewModel> albumViewModel = new List<PictureViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = UriAccount;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                var response = await client.GetAsync(string.Format("api/AlbumProfile/Profile/{0}", id));
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    ICollection<Pictures> pictures = JsonConvert.DeserializeObject<ICollection<Pictures>>(responseContent);
                    
                    foreach (Pictures picture in pictures)
                    {
                        albumViewModel.Add(BuildPictureViewModel(picture));
                    }

                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    return RedirectToAction("Error");
                }

                return View(albumViewModel);

            }
            
        }

        private PictureViewModel BuildPictureViewModel(Pictures picture)
        {
            PictureViewModel pictureVM = new PictureViewModel()
            {
                Id = picture.Id,
                Description = picture.Description,
                PictureUrl = picture.PictureUrl

            };
            foreach(Posts post in picture.Posts)
            {
                pictureVM.Posts.Add(new PostsViewModel()
                {
                    Id = post.Id,
                    DatePost = post.DatePost,
                    TextPost = post.TextPost,
                    ProfileAuthor = new ProfileViewModel()
                    {
                        Id = post.ProfileAuthor.Id,
                        FirstName = post.ProfileAuthor.FirstName,
                        LastName = post.ProfileAuthor.LastName
                    }
                });

            }
            return pictureVM;
        }

        [HttpGet]
        public ActionResult AddPicture()
        {
            if (string.IsNullOrEmpty(Session["access_token"]?.ToString()))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost, ActionName("AddPicture")]
        public async Task<ActionResult> AddPictureAction(PictureViewModel model)
        {
            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Acount");
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
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                    content.Add(new StringContent(JsonConvert.SerializeObject(model)));
                    AddContent(content);

                    var response = await client.PostAsync("api/AlbumProfile/AddPicture", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Error");
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Pictures picture = JsonConvert.DeserializeObject<Pictures>(responseContent);
                    return RedirectToAction("Details", "Pictures", new { picture.Id });
                }
            }
        }
        
        [HttpGet, ActionName("RemovePicture")]
        public async Task<ActionResult> RemovePictureAction(int id)
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

                var response = await client.GetAsync(string.Format("api/AlbumProfile/RemovePicture/{0}", id));
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Pictures");
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
        }
        
        [HttpPost]
        public async Task<ActionResult> AddPost(PictureViewModel model)
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

                    Posts post = new Posts()
                    {
                        TextPost = model.Post.TextPost,
                        DatePost = DateTime.UtcNow
                    };

                    content.Add(new StringContent(JsonConvert.SerializeObject(post)));
                    AddContent(content);

                    var response = await client.PostAsync(string.Format("api/AlbumProfile/AddPostPicture/{0}", model.Id), content);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Error");
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Pictures picture = JsonConvert.DeserializeObject<Pictures>(responseContent);

                    return RedirectToAction("Details", "Pictures", new { picture.Id });
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> RemovePost(int id)
        {
            string acess_token = Session["access_token"]?.ToString();
            if (string.IsNullOrEmpty(acess_token))
            {
                return RedirectToAction("Login", "Acount");
            }

            using (var client = new HttpClient())
            {

                var response = await client.DeleteAsync(string.Format("api/AlbumProfile/RemovePosts/{0}", id));
                if (!response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Error");
                }
                return View();
            }
        }

        #region helpers
        private void AddContent(MultipartFormDataContent content)
        {
            if (Request.Files.Count > 0)
            {
                byte[] fileBytes;
                using (var inputStream = Request.Files[0].InputStream)
                {
                    MemoryStream memoryStream = inputStream as MemoryStream;
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
        }
        #endregion

    }
}
