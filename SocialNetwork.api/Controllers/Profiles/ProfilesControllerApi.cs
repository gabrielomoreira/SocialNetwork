using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
using SocialNetwork.core.Entity;
using SocialNetwork.data.Repository;

namespace SocialNetwork.api.Controllers
{
    [Authorize, RoutePrefix("api/Profiles")]
    public class ProfilesController : ApiController
    {
        private ProfilesRepositoryAsync _repositoryProfile;
        private PostsRepositoryAsync _repositoryPost;

        private ProfilesController()
        {
            try
            {
                _repositoryProfile = new ProfilesRepositoryAsync();
                _repositoryPost = new PostsRepositoryAsync();
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        #region AllowAnonymous and/or Authorize
        [AllowAnonymous, HttpGet, Route("getProfile/{id:int}")]
        public async Task<IHttpActionResult> GetProfiles(int id)
        {
            try
            {
                Profiles profile = await _repositoryProfile.GetByIDAsync(id);
                return Ok(profile);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        [AllowAnonymous, HttpGet, Route("ListProfiles")]
        public async Task<IHttpActionResult> GetListProfiles()
        {
            try
            {
                ICollection<Profiles> profiles = await _repositoryProfile.GetAllAsync();

                profiles = profiles.Where(p => p.AccountId != User.Identity.GetUserId()).ToList();
                return Ok(profiles);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }
        #endregion

        #region Authorize ONLY
        [HttpGet, Route("getProfileByAccount")]
        public async Task<IHttpActionResult> GetProfilesByIdAsync()
        {
            try
            {
                Profiles profile = await _repositoryProfile.GetByIDAccountAsync(User.Identity.GetUserId());
                return Ok(profile);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        [HttpPost, Route("Create"), ResponseType(typeof(Profiles))]
        public async Task<IHttpActionResult> CreateProfilesAsync()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest();
            }

            try
            {
                var result = await Request.Content.ReadAsMultipartAsync();
                var requestJson = await result.Contents[0].ReadAsStringAsync();
                Profiles requestProfile = JsonConvert.DeserializeObject<Profiles>(requestJson);
                requestProfile.AccountId = User.Identity.GetUserId();
                if (result.Contents.Count > 1)
                {
                    requestProfile.PictureProfileUrl = await CreateBlobPictureProfilesAsync(result.Contents[1]);
                }

                await _repositoryProfile.CreateAsync(requestProfile);

                return Ok(requestProfile);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut, Route("Update"), ResponseType(typeof(void))]
        public async Task<IHttpActionResult> UpdateProfilesAsync()
        {
            Profiles requestProfile = null;
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest();
                }
                var result = await Request.Content.ReadAsMultipartAsync();

                var requestJson = await result.Contents[0].ReadAsStringAsync();
                requestProfile = JsonConvert.DeserializeObject<Profiles>(requestJson);
                if (result.Contents.Count > 1)
                {
                    requestProfile.PictureProfileUrl = await CreateBlobPictureProfilesAsync(result.Contents[1]);
                }

                await _repositoryProfile.UpdateAsync(requestProfile);

                return Ok(requestProfile);
            }
            catch (DbUpdateConcurrencyException e)
            {
                Console.Write(e.Message);
                if ((requestProfile != null) && _repositoryProfile.ProfileExists(requestProfile.Id))
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

        [HttpDelete, Route("delete")]
        public async Task<IHttpActionResult> DeleteProfilesAsync(int id)
        {
            try
            {
                await _repositoryProfile.DeleteAsync(id);
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }
        #endregion

        #region controlFriends
        [HttpGet, Route("AddFriend/{id:int}"), ResponseType(typeof(Profiles))]
        public async Task<IHttpActionResult> AddFriendAsync(int id)
        {
            try
            {
                // Pega o perfil da conta
                Profiles profile = await _repositoryProfile.GetByIDAccountAsync(User.Identity.GetUserId());
                // Atraves do request, pega os dados do amigo
                Profiles friend = await _repositoryProfile.GetByIDAsync(id);

                // Adiciona o amigo
                profile.Following.Add(friend);
                await _repositoryProfile.UpdateAsync(profile);


                //Retorno se OK
                return Ok(profile);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }

        [HttpGet, Route("RemoveFriend/{id:int}"), ResponseType(typeof(Profiles))]
        public async Task<IHttpActionResult> RemoveFriendAsync(int id)
        {
            try
            {
                // Pega o perfil da conta
                Profiles profile = await _repositoryProfile.GetByIDAccountAsync(User.Identity.GetUserId());
                // Atraves do request, pega os dados do amigo
                Profiles friend = await _repositoryProfile.GetByIDAsync(id);

                // Adiciona o amigo
                profile.Following.Remove(friend);
                await _repositoryProfile.UpdateAsync(profile);
                

                //Retorno se OK
                return Ok(profile);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }
        #endregion

        #region posts
        [HttpPost, Route("AddPosts")]
        public async Task<IHttpActionResult> AddPostsProfileAsync()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest();
            }

            try
            {
                var result = await Request.Content.ReadAsMultipartAsync();
                var requestPost = await result.Contents[0].ReadAsStringAsync();

                Posts post = JsonConvert.DeserializeObject<Posts>(requestPost);


                // Adiciona postagem no proprietário
                Profiles profile = await _repositoryProfile.GetByIDAsync(post.ProfileAuthor.Id);
                profile.Posts.Add(post);

                await _repositoryProfile.UpdateAsync(profile);


                //Retorno se OK
                return Ok();
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }

        /*[HttpGet, Route("AddPostsReply/{id:int}")]
        public async Task<IHttpActionResult> AddPostsReplyAsync(int id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest();
            }

            try
            {
                var result = await Request.Content.ReadAsMultipartAsync();
                var requestPost = await result.Contents[0].ReadAsStringAsync();

                Posts reply = JsonConvert.DeserializeObject<Posts>(requestPost);

                if (result.Contents.Count > 1)
                {
                    reply.PicturePost.PictureUrl = await CreateBlobPostsPicturesAlbumAsync(result.Contents[1]);
                }

                // Adiciona reply no post
                Profiles profile = await _repositoryProfile.GetByIDAsync(reply.ProfileOwner.Id);
                profile.Posts.Where(p => p.Id == id).SingleOrDefault().Responses.Add(reply);

                await _repositoryProfile.UpdateAsync(profile);


                //Retorno se OK
                return Ok();
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }*/

        [HttpDelete, Route("RemovePosts/{id:int}")]
        public async Task<IHttpActionResult> RemovePostsAsync(int id)
        {
            try
            {

                await _repositoryPost.DeletePost(id);
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }
        #endregion

        #region Helpers
        private IHttpActionResult InternalError(Exception e)
        {
            Console.Write(e.Message);
            return StatusCode(HttpStatusCode.InternalServerError);
        }

        private async Task<string> CreateBlobPictureProfilesAsync(HttpContent httpContent)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var blobContainerName = "sn-pictureprofiles";
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

        private async Task<string> CreateBlobPostsPicturesAlbumAsync(HttpContent httpContent)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var blobContainerName = "sn-postpictures";
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