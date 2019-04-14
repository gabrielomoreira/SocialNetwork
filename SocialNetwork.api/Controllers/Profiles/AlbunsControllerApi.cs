using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using SocialNetwork.core.AlbumEntity;
using SocialNetwork.data.AlbumRepository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace SocialNetwork.api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Albunss")]
    public class AlbunsController : ApiController
    {
        private readonly AlbumRepositoryAsync _repository;
        private AlbunsController()
        {
            try
            {
                _repository = new AlbumRepositoryAsync();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        [HttpGet]
        [Route("getAlbunsByAccount")]
        public async Task<IHttpActionResult> GetAlbunsByIdAsync()
        {
            try
            {
                ICollection<Albuns> albuns = await _repository.GetByIDAccountAsync(User.Identity.GetUserId());
                return Ok(albuns);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        [HttpGet]
        [Route("getAllAlbuns")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetAlbunsAsync()
        {
            try
            {
                ICollection<Albuns> albuns = await _repository.GetAllAsync();
                return Ok(albuns);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        [HttpGet]
        [Route("getAlbum/{id:int}")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetAlbuns(int id)
        {
            try
            {
                Albuns album = await _repository.GetByIDAsync(id);
                return Ok(album);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        /*
        [HttpPost]
        [Route("Create")]
        [ResponseType(typeof(Albuns))]
        public async Task<IHttpActionResult> CreateAlbunsAsync()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest();
            }

            try
            {
                var result = await Request.Content.ReadAsMultipartAsync();

                var requestJson = await result.Contents[0].ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<Albuns>(requestJson);

                if (result.Contents.Count > 1)
                {
                    model.PictureProfileUrl = await CreateBlobPictureAlbunsAsync(result.Contents[1]);
                }
                var accountId = User.Identity.GetUserId();
                var album = new Albuns()
                {
                    Name = model.Name,
                    Profile = model.Profile,
                    Pictures = model.Pictures
                };

                await _repository.CreateAsync(album);

                return Ok(album);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return StatusCode(HttpStatusCode.InternalServerError);
            }


        }

        [HttpPut]
        [Route("Update")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> UpdateAlbunsAsync()
        {
            Albuns album = null;
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest();
                }
                var result = await Request.Content.ReadAsMultipartAsync();

                var requestJson = await result.Contents[0].ReadAsStringAsync();
                album = JsonConvert.DeserializeObject<Albuns>(requestJson);

                if (result.Contents.Count > 1)
                {
                    album.PictureProfileUrl = await CreateBlobPictureAlbunsAsync(result.Contents[1]);
                }
                await _repository.UpdateAsync(album);

                return Ok(album);
            }
            catch (DbUpdateConcurrencyException e)
            {
                Console.Write(e.Message);
                if ((album != null) && _repository.ProfileExists(album.Id))
                {
                    return StatusCode(HttpStatusCode.Conflict);
                }
                return NotFound();
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }
        */
        [HttpDelete]
        [Route("delete")]
        [ResponseType(typeof(Albuns))]
        public async Task<IHttpActionResult> DeleteAlbunsAsync(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        /*
        #region controlFriends
        [HttpGet]
        [Route("AddPicture")]
        [ResponseType(typeof(Albuns))]
        public async Task<IHttpActionResult> AddPictureAsync()
        {
            try
            {
                // Pega o perfil da conta
                Albuns album = await _repository.GetByIDAccountAsync(User.Identity.GetUserId());


                // Atraves do request, pega os dados do amigo
                Albuns friend = await _repository.GetByIDAsync(id);

                // Adiciona o amigo
                album.Following.Add(friend);
                await _repository.UpdateAsync(album);


                //Retorno se OK
                return Ok(album);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }

        [HttpGet]
        [Route("RemoveFriend/{id:int}")]
        [ResponseType(typeof(Albuns))]
        public async Task<IHttpActionResult> RemoveFriendAsync(int id)
        {
            try
            {
                // Pega o perfil da conta
                Albuns album = await _repository.GetByIDAccountAsync(User.Identity.GetUserId());


                // Atraves do request, pega os dados do amigo
                Albuns friend = await _repository.GetByIDAsync(id);

                // Adiciona o amigo
                album.Following.Remove(friend);
                await _repository.UpdateAsync(album);


                //Retorno se OK
                return Ok(album);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }
        #endregion
        
        #region Helpers*/
        private IHttpActionResult InternalError(Exception e)
        {
            Console.Write(e.Message);
            return StatusCode(HttpStatusCode.InternalServerError);
        }
        /*
        private async Task<string> CreateBlobPictureAlbunsAsync(HttpContent httpContent)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var blobContainerName = "sn-picturealbums";
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

        private async Task<string> CreateBlobPicturesAsync(HttpContent httpContent)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var blobContainerName = "sn-pictures";
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
        */
    }
}