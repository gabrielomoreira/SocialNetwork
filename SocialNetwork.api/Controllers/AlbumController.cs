using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using SocialNetwork.core.Models;
using SocialNetwork.data.Repositories;

namespace SocialNetwork.api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Profiles/Album")]
    public class AlbumController : ApiController
    {
        private AlbumRepositoryAsync _repository;
        private AlbumController()
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

        // GET: api/Profiles
        [HttpGet]
        [Route("getAll")]
        [AllowAnonymous]
        public async Task<ICollection<Albuns>> GetAlbumAsync()
        {
            ICollection<Albuns> profiles = await _repository.GetAllAsync();
            if (profiles == null)
            {
                return null;
            }
            return profiles.ToList();
        }

        [HttpGet]
        [Route("getAlbumByAccount")]
        [AllowAnonymous]
        public async Task<ICollection<Albuns>> GetAlbumByAccountAsync()
        {
            try
            {
                var result = await Request.Content.ReadAsMultipartAsync();

                var requestJson = await result.Contents[0].ReadAsStringAsync();
                Profile profile = JsonConvert.DeserializeObject<Profile>(requestJson);

                ICollection<Albuns> profiles = await _repository.GetAllAlbunsProfileIdAsync(profile);

                if (profiles == null)
                {
                    return null;
                }
                return profiles.ToList();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return null;
            }

        }

        [HttpGet]
        [Route("getAlbunsProfileId/{id:int}")]
        [AllowAnonymous]
        public async Task<ICollection<Albuns>> GetAllAlbunsProfileIdAsync(int id)
        {
            ICollection<Albuns> profiles = await _repository.GetByIDAsync(id);

            if (profiles == null)
            {
                return null;
            }
            return profiles.ToList();

        }
        
        // GET: api/Profiles/getProfile/5
        [HttpGet]
        [Route("getAlbum/{id:int}")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetAlbum(int id)
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

        // PUT: api/Albuns/5
        [HttpPut]
        [Route("Update")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> UpdateAlbumAsync()
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
                
                await _repository.UpdateAsync(album);

                return Ok(album);
            }
            catch (DbUpdateConcurrencyException e)
            {
                Console.Write(e.Message);
                if ((album != null) && _repository.AlbunsExists(album.Id))
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

        // POST: api/Profiles
        [HttpPost]
        [Route("Create")]
        [ResponseType(typeof(Albuns))]
        public async Task<IHttpActionResult> CreateAlbumAsync()
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

                var album = new Albuns()
                {
                    Description = model.Description,
                    Profile = model.Profile
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

        // DELETE: api/Albuns/5
        [ResponseType(typeof(Albuns))]
        [Route(Name = "Delete/{id:int}")]
        public async Task<IHttpActionResult> DeleteAlbumAsync(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return Ok();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return StatusCode(HttpStatusCode.InternalServerError);
            }
            
        }
        
        private IHttpActionResult InternalError(Exception e)
        {
            Console.Write(e.Message);
            return StatusCode(HttpStatusCode.InternalServerError);
        }

    }

}