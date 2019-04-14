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
using SocialNetwork.core.Models;
using System.Collections.Generic;
using SocialNetwork.web.Models.Profile;
#endregion

namespace SocialNetwork.web.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly Uri UriAccount = new Uri("http://localhost:2001/");

        #region CRUD
        [HttpGet]
        public async Task<ActionResult> Index()
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
                    ProfileViewModel profileView = BuildProfileViewModel(profile);

                    return View(profileView);
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            string acess_token = Session["access_token"]?.ToString();
            ProfileViewModel profileViewModel = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = UriAccount;

                var response = await client.GetAsync(string.Format("api/Profiles/getProfile/{0}", id));
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (responseContent != "null")
                    {
                        Profile profile = JsonConvert.DeserializeObject<Profile>(responseContent);

                        if (profile == null || string.IsNullOrEmpty(profile.FirstName))
                        {
                            return RedirectToAction("NotFound", "Profiles");
                        }

                        profileViewModel = BuildProfileViewModel(profile);

                        // Se não tem um token, retorna a view do jeito que está
                        if (string.IsNullOrEmpty(acess_token))
                        {
                            return View(profileViewModel);
                        }
                    }
                }
                else
                {
                    // não precisa de autorização para verificar dados do amigo
                    return RedirectToAction("Error");
                }
            }

            // Dessa parte em diante precisa estar logado para pegar informações sensíveis
            using (var client = new HttpClient())
            {
                //Se chegar aqui é porque existe um token, portanto, uma conta logada. Logo retorna essa conta
                client.BaseAddress = UriAccount;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                var responseAccountProfile = await client.GetAsync("api/Profiles/getProfileByAccount");

                /* se não tiver sucesso no retorno da conta de perfil, 
                    * deve verificar a causa de retorno não experado (porque neste ponto é uma conta com perfil logado)
                    * por isso lança uma excessão.
                */
                if (!responseAccountProfile.IsSuccessStatusCode)
                {
                    throw new Exception(responseAccountProfile.StatusCode.ToString());
                }

                var responseContentAccountProfile = await responseAccountProfile.Content.ReadAsStringAsync();

                Profile profileAccount = JsonConvert.DeserializeObject<Profile>(responseContentAccountProfile);

                // Se o perfil da busca é o mesmo do perfil da conta, redireciona para index
                if (profileAccount.Id == profileViewModel.Id)
                {
                    return RedirectToAction("Index", "Profiles");
                }

                // verifica se existe um amigo da lista de amigos para retornar se é ou não amigo
                profileViewModel.IsFriend = profileViewModel.Followers.Where(f => f.Id == profileAccount.Id).Count() == 1;

                return View(profileViewModel);
            }

        }

        [HttpGet]
        public async Task<ActionResult> ListProfiles()
        {
            string acess_token = Session["access_token"]?.ToString();
            ICollection<ProfileViewModel> profilesViewModel = new List<ProfileViewModel>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = UriAccount;

                var response = await client.GetAsync("api/Profiles/listProfiles");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (responseContent == "null")
                    {
                        return View(profilesViewModel);
                    }

                    ICollection<Profile> profiles = JsonConvert.DeserializeObject<ICollection<Profile>>(responseContent);

                    foreach (Profile profile in profiles)
                    {
                        ProfileViewModel profileVM = BuildProfileViewModel(profile);
                        profilesViewModel.Add(profileVM);
                    }

                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }
                if (!response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Error");
                }
            }

            // verifica se existe um token, para não retornar dados sensiveis
            if (string.IsNullOrEmpty(acess_token))
            {
                return View(profilesViewModel);
            }

            //senão...
            //Remove perfil da conta da lista (caso esteja logado), uma vez que não faz sentiod retorna-lo
            using (var client = new HttpClient())
            {
                //Se chegar aqui é porque existe um token, portanto, uma conta logada. Logo precisa dessa conta para ser removida
                client.BaseAddress = UriAccount;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                // pega o perfil da conta
                var responseAccountProfile = await client.GetAsync("api/Profiles/getProfileByAccount");
                if (responseAccountProfile.IsSuccessStatusCode)
                {
                    var responseContentAccountProfile = await responseAccountProfile.Content.ReadAsStringAsync();

                    Profile profileAcc = JsonConvert.DeserializeObject<Profile>(responseContentAccountProfile);

                    // Remove o perfil da lista, para não aparecer na view
                    profilesViewModel = profilesViewModel.Where(x => x.Id != profileAcc.Id).ToList();

                    foreach (ProfileViewModel friend in profilesViewModel)
                    {
                        friend.IsFriend = false;
                        if (profileAcc.Following.Count() > 0)
                        {
                            friend.IsFriend = profileAcc.Following.Any(x => x.Id == friend.Id);
                        }
                    }
                }

                return View(profilesViewModel);
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            if (string.IsNullOrEmpty(Session["access_token"]?.ToString()))
            {
                return RedirectToAction("Register", "Account");
            }
            return View();
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAction(ProfileViewModel model)
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

                    var response = await client.PostAsync("api/Profiles/Create", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Error");
                    }

                    return RedirectToAction("Index", "Profiles");
                }
            }

        }

        [HttpGet]
        public ActionResult Update(ProfileViewModel model)
        {
            return View(model);
        }

        [HttpPost, ActionName("Update")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateAction(ProfileViewModel model)
        {
            string acess_token = Session["access_token"]?.ToString();
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

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
                    AddContent(content);

                    var response = await client.PutAsync("api/Profiles/Update", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Error");
                    }
                    return RedirectToAction("Index", "Profiles");

                }
            }
        }

        public ActionResult Delete(ProfileViewModel model)
        {
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAction()
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

                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Error");
                    }
                    return RedirectToAction("Logout", "Account");

                }
            }
        }
        #endregion

        #region control friends relationships
        [HttpGet, ActionName("AddFriend")]
        public async Task<ActionResult> AddFriendAction(int id)
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

                    var response = await client.GetAsync(string.Format("api/Profiles/AddFriend/{0}", id));

                    var responseContent = await response.Content.ReadAsStringAsync();
                    ProfileViewModel profile = JsonConvert.DeserializeObject<ProfileViewModel>(responseContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("ListProfiles", "Profiles");
                    }
                    else
                    {
                        return RedirectToAction("Error");
                    }

                }
            }

        }

        [HttpGet, ActionName("RemoveFriend")]
        public async Task<ActionResult> RemoveFriendAction(int id)
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

                var response = await client.GetAsync(string.Format("api/Profiles/RemoveFriend/{0}", id));
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListProfiles", "Profiles");
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
        }
        #endregion

        #region helpers
        private ProfileViewModel BuildProfileViewModel(Profile profile)
        {
            ProfileViewModel profileVM = new ProfileViewModel()
            {
                Id = profile.Id,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                BirthDate = profile.BirthDate,
                PictureUrl = profile.PictureProfileUrl,
                AccountId = profile.AccountId,
                IsFriend = false // padrão
            };

            foreach (Profile follower in profile.Followers)
            {
                profileVM.Followers.Add(new ProfileViewModel()
                {
                    Id = follower.Id,
                    AccountId = follower.AccountId,
                    FirstName = follower.FirstName,
                    LastName = follower.LastName,
                    BirthDate = follower.BirthDate,
                    PictureUrl = follower.PictureProfileUrl
                });
            }

            foreach (Profile following in profile.Following)
            {

                profileVM.Following.Add(new ProfileViewModel()
                {
                    Id = following.Id,
                    AccountId = following.AccountId,
                    FirstName = following.FirstName,
                    LastName = following.LastName,
                    BirthDate = following.BirthDate,
                    PictureUrl = following.PictureProfileUrl
                });
            }

            return profileVM;
        }


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
