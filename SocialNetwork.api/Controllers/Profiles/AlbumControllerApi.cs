using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using SocialNetwork.core.PictureEntity;
using SocialNetwork.core.ProfileEntity;
using SocialNetwork.data.ProfileRepository;

namespace SocialNetwork.api.Controllers
{
    [Authorize, RoutePrefix("api/AlbumProfile")]
    public class AlbumController : ApiController
    {
        private ProfilesRepositoryAsync _repository;
        private PictureRepositoryAsync _repositoryPictures;
        private AlbumController()
        {
            try
            {
                _repository = new ProfilesRepositoryAsync();
                _repositoryPictures = new PictureRepositoryAsync();
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        [HttpGet, Route("Index")]
        public async Task<IHttpActionResult> Index()
        {
            try
            {
                // Pega o perfil da conta
                Profiles profile = await _repository.GetByIDAccountAsync(User.Identity.GetUserId());
                return Ok(profile.Album);

            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        [HttpGet, Route("Profile/{idProfile:int}")]
        public async Task<IHttpActionResult> ListPicturesProfileAsync(int idProfile)
        {
            try
            {
                // Pega o perfil da conta
                Profiles profile = await _repository.GetByIDAsync(idProfile);

                return Ok(profile.Album);
                
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        [HttpGet, Route("SearchByIMG/{idImg}")]
        public async Task<IHttpActionResult> SearchByIMGAsync(int idImg)
        {
            try
            {
                // Pega o perfil da conta
                Pictures picture = await _repositoryPictures.GetImage(idImg);

                return Ok(picture);

            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }


        [HttpPost, Route("AddPicture")]
        public async Task<IHttpActionResult> AddPictureAsync()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest();
            }
            try
            {
                // Pega o perfil da conta
                Profiles profile = await _repository.GetByIDAccountAsync(User.Identity.GetUserId());

                var result = await Request.Content.ReadAsMultipartAsync();
                var requestJson = await result.Contents[0].ReadAsStringAsync();
                Pictures requestPicture = JsonConvert.DeserializeObject<Pictures>(requestJson);
                if (result.Contents.Count > 1)
                {
                    requestPicture.PictureUrl = await CreateBlobPicturesAlbumAsync(result.Contents[1]);
                }

                profile.Album.Add(requestPicture);
                await _repository.UpdateAsync(profile);


                //Retorno se OK
                return Ok(profile);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }

        [HttpGet, Route("RemovePicture/{id:int}")]
        [ResponseType(typeof(Profiles))]
        public async Task<IHttpActionResult> RemovePictureAsync(int id)
        {
            try
            {
                // Pega o perfil da conta
                Profiles profile = await _repository.GetByIDAccountAsync(User.Identity.GetUserId());

                Pictures picture = profile.Album.ToList().Where(p => p.Id == id).SingleOrDefault();

                // Adiciona o amigo
                profile.Album.Remove(picture);
                await _repository.UpdateAsync(profile);


                //Retorno se OK
                return Ok(profile);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }

        #region Helpers
        private IHttpActionResult InternalError(Exception e)
        {
            Console.Write(e.Message);
            return StatusCode(HttpStatusCode.InternalServerError);
        }
        
        private async Task<string> CreateBlobPicturesAlbumAsync(HttpContent httpContent)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var blobContainerName = "sn-albumpictures";
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(blobContainerName);
            await blobContainer.CreateIfNotExistsAsync();

            await blobContainer.SetPermissionsAsync(
                new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                }
            );

            var fileName = httpContent.Headers.ContentDisposition.FileName;
            var byteArray = await httpContent.ReadAsByteArrayAsync();

            var blob = blobContainer.GetBlockBlobReference(GetRandomBlobName(fileName));
            await blob.UploadFromByteArrayAsync(byteArray, 0, byteArray.Length);

            return blob.Uri.AbsoluteUri;
        }

        private string GetRandomBlobName(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }
        #endregion
    }
}