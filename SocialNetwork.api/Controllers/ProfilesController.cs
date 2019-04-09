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
using SocialNetwork.core.Models;
using SocialNetwork.data.Repositories;

namespace SocialNetwork.api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Profiles")]
    public class ProfilesController : ApiController
    {
        private ProfileRepositoryAsync _repository;
        private ProfilesController()
        {
            try
            {
                _repository = new ProfileRepositoryAsync();
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        // GET: api/Profiles
        [HttpGet]
        [Route("getAll")]
        [AllowAnonymous]
        public async Task<ICollection<Profile>> GetProfilesAsync()
        {
            ICollection<Profile> profiles = await _repository.GetAllAsync();
            if (profiles == null)
            {
                return null;
            }
            return profiles.ToList();
        }

        // GET: api/Profiles/getByIdAccount/5
        [HttpGet]
        [Route("getProfileByAccount")]
        public async Task<IHttpActionResult> GetProfileByIdAsync()
        {
            try
            {
                var accountId = User.Identity.GetUserId();
                Profile profile = await _repository.GetByIDAccountAsync(accountId);

                return Ok(profile);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }

        // POST: api/Profiles
        [HttpPost]
        [Route("Create")]
        [ResponseType(typeof(Profile))]
        public async Task<IHttpActionResult> CreateProfileAsync()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest();
            }

            try
            {
                
                var result = await Request.Content.ReadAsMultipartAsync();

                var requestJson = await result.Contents[0].ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<Profile>(requestJson);

                if (result.Contents.Count > 1)
                {
                    model.PictureUrl = await CreateBlobPictureProfilesAsync(result.Contents[1]);
                }
                var accountId = User.Identity.GetUserId();
                var profile = new Profile()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BirthDate = model.BirthDate,
                    PictureUrl = model.PictureUrl,
                    AccountId = accountId
                };

                await _repository.CreateAsync(profile);

                return Ok(profile);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return StatusCode(HttpStatusCode.InternalServerError);
            }
            
            
        }

        // PUT: api/Profiles/5
        [HttpPut]
        [Route("Update")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> UpdateProfileAsync()
        {
            //var accountId = User.Identity.GetUserId();
            Profile profile = null;
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest();
                }
                var result = await Request.Content.ReadAsMultipartAsync();

                var requestJson = await result.Contents[0].ReadAsStringAsync();
                profile = JsonConvert.DeserializeObject<Profile>(requestJson);

                if (result.Contents.Count > 1)
                {
                    profile.PictureUrl = await CreateBlobPictureProfilesAsync(result.Contents[1]);
                }
                await _repository.UpdateAsync(profile);

                return Ok(profile);
            }
            catch (DbUpdateConcurrencyException e)
            {
                Console.Write(e.Message);
                if ((profile != null) && _repository.ProfileExists(profile.Id))
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

        // DELETE: api/Profiles/5
        [HttpDelete]
        [Route("delete")]
        [ResponseType(typeof(Profile))]
        public async Task<IHttpActionResult> DeleteProfileAsync(int id)
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


        //helpers
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

        /*protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository.Dispose();
            }
            base.Dispose(disposing);
        }*/
    }
}