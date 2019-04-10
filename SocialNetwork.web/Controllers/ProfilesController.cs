using System;
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
        // Dados da própria conta
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
                    

                    return RedirectToAction("Error");
                }
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
                return RedirectToAction("Error");
            }
            
        }

        // dados da conta de terceiros
        [HttpGet]
        [Route(Name = "Details")]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                // Verifica se existe um token (porque está logado)
                string acess_token = Session["access_token"]?.ToString();

                ProfileViewModel profileView = null;
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
                            profileView = new ProfileViewModel()
                            {
                                Id = profile.Id,
                                FirstName = profile.FirstName,
                                LastName = profile.LastName,
                                BirthDate = profile.BirthDate,
                                PictureUrl = profile.PictureUrl,
                                AccountId = profile.AccountId,
                                IsFriend = false // padrão
                            };
                        }
                    }
                    else
                    {
                        // não precisa de autorização para verificar dados do amigo
                        return RedirectToAction("Error");
                    }

                    // Se o profile aqui estiver nulo retorna "NotFound"
                    if (profileView == null)
                    {
                        return RedirectToAction("NotFound", "Profiles");
                    }
                    
                    // Se não tem um token, retorna a view do jeito que está
                    if (string.IsNullOrEmpty(acess_token) && profileView != null)
                    {
                        return View(profileView);
                    }
                }

                // Dessa parte em diante precisa estar logado para pegar informações sensíveis
                using (var client = new HttpClient())
                {
                    /** Este processo é pessado, verificar uma forma de otimizar a busca de amigos */

                    //Se chegar aqui é porque existe um token, portanto, uma conta logada. Logo retorna essa conta
                    client.BaseAddress = UriAccount;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                    var responseAccountProfile = await client.GetAsync("api/Profiles/getProfileByAccount");
                    if (responseAccountProfile.IsSuccessStatusCode)
                    {
                        var responseContentAccountProfile = await responseAccountProfile.Content.ReadAsStringAsync();

                        Profile profile = JsonConvert.DeserializeObject<Profile>(responseContentAccountProfile);
                        ProfileViewModel profileAccountProfile = new ProfileViewModel()
                        {
                            Id = profile.Id,
                            FirstName = profile.FirstName,
                            LastName = profile.LastName,
                            BirthDate = profile.BirthDate,
                            PictureUrl = profile.PictureUrl
                        };
                        profileAccountProfile.Friends = new List<ProfileViewModel>();

                        // Se o perfil da busca é o mesmo do perfil da conta, redireciona para index
                        if (profile.Id == profileView.Id)
                        {
                            return RedirectToAction("Index", "Profiles");
                        }

                        // Adiciona os amigos para a conta
                        ICollection<Profile> friendsAccount = new List<Profile>();
                        foreach(Profile friend in profile.Friends)
                        {
                            ProfileViewModel friendView = new ProfileViewModel()
                            {
                                Id = friend.Id,
                                FirstName = friend.FirstName,
                                LastName = friend.LastName,
                                BirthDate = friend.BirthDate,
                                PictureUrl = friend.PictureUrl
                            };

                            // popula a lista de amigos
                            profileAccountProfile.Friends.Add(friendView);
                        }

                        // verifica se existe um amigo da lista de amigos para retornar se é ou não amigo
                        profileView.IsFriend = profileAccountProfile.Friends.Where(f => f.Id == profileView.Id).Count() == 1;


                    }
                    // se não tiver sucesso no retorno da conta de perfil, deve verificar a causa de retorno não experado, por isso lança uma excessão
                    if (!responseAccountProfile.IsSuccessStatusCode)
                    {
                        throw new Exception(responseAccountProfile.StatusCode.ToString());
                    }

                    return View(profileView);
                }
            }
            catch (Exception e)
            {
                // Em casos de erro!
                Console.Write(e.Message);
                return RedirectToAction("Error");
            }

        }

        [HttpGet]
        [Route(Name = "ListProfiles")]
        public async Task<ActionResult> ListProfiles()
        {
            try
            {
                ICollection<ProfileViewModel> profilesViewModel = new List<ProfileViewModel>();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = UriAccount;

                    var response = await client.GetAsync("api/Profiles/listProfiles");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent == "null")
                        {
                            return View(profilesViewModel);
                        }

                        ICollection<Profile> profiles = JsonConvert.DeserializeObject<ICollection<Profile>>(responseContent);
                        
                        foreach(Profile profile in profiles)
                        {
                            ProfileViewModel profileVM = new ProfileViewModel()
                            {
                                Id = profile.Id,
                                AccountId = profile.AccountId,
                                FirstName = profile.FirstName,
                                LastName = profile.LastName,
                                BirthDate = profile.BirthDate,
                                PictureUrl = profile.PictureUrl,
                                IsFriend = false
                            };

                            profilesViewModel.Add(profileVM);
                        }
                        
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }

                // verifica se existe um token, caso não, retorna todos os amigos
                string acess_token = Session["access_token"]?.ToString();
                if (string.IsNullOrEmpty(acess_token))
                {
                    return View(profilesViewModel);
                }

                //senão...
                //Remove perfil da conta da lista (caso esteja logado)
                using (var client = new HttpClient())
                {
                    /** Este processo é pessado, verificar uma forma de otimizar a busca de amigos */
                    //Se chegar aqui é porque existe um token, portanto, uma conta logada. Logo retorna essa conta
                    client.BaseAddress = UriAccount;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{acess_token}");

                    // pega o perfil da conta
                    var responseAccountProfile = await client.GetAsync("api/Profiles/getProfileByAccount");
                    if (responseAccountProfile.IsSuccessStatusCode)
                    {
                        var responseContentAccountProfile = await responseAccountProfile.Content.ReadAsStringAsync();

                        // Verifica se existe perfil da conta (nesse ponte, não é experado isso)
                        if (responseContentAccountProfile == "null")
                        {
                            throw new Exception("Perfil da conta não encontrada");
                        }

                        Profile profileAcc = JsonConvert.DeserializeObject<Profile>(responseContentAccountProfile);

                        // Remove o perfil da lista, para não aparecer na view
                        profilesViewModel = profilesViewModel.Where(x => x.Id != profileAcc.Id).ToList();

                        ProfileViewModel profileAccount = new ProfileViewModel()
                        {
                            Id = profileAcc.Id,
                            FirstName = profileAcc.FirstName,
                            LastName = profileAcc.LastName,
                            BirthDate = profileAcc.BirthDate,
                            PictureUrl = profileAcc.PictureUrl,
                            AccountId = profileAcc.AccountId
                        };

                        foreach (ProfileViewModel friend in profilesViewModel)
                        {
                            friend.IsFriend = profileAcc.Friends.Any(x => x.Id == friend.Id);
                        }
                        
                        
                    }

                    return View(profilesViewModel);
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return RedirectToAction("Error");
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
                        return RedirectToAction("Error");
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
                        return RedirectToAction("Error");
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
                        return RedirectToAction("Error");
                    }

                }
            }
        }


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
