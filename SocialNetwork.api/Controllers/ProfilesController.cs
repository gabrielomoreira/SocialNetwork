using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SocialNetwork.core.Models;
using SocialNetwork.data.Repositories;

namespace SocialNetwork.api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class ProfilesController : ApiController
    {
        private ProfileRepositoryAsync _repository;
        //private DataContext db = new DataContext();
        private ProfilesController()
        {
            _repository = new ProfileRepositoryAsync();
        }

        // GET: api/Profiles
        public async Task<List<Profile>> GetProfilesAsync()
        {
            ICollection<Profile> profiles = await _repository.GetAllAsync();
            if (profiles == null)
            {
                return null;
            }
            return profiles.ToList();
        }

        // GET: api/Profiles/5
        [ResponseType(typeof(Profile))]
        public async Task<IHttpActionResult> GetProfile(int id)
        {
            try
            {
                Profile profile = await _repository.GetByIDAsync(id);
                if (profile == null)
                {
                    return NotFound();
                }

                return Ok(profile);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }

        // PUT: api/Profiles/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutProfile(Profile profile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _repository.UpdateAsync(profile);
            }
            catch (DbUpdateConcurrencyException e)
            {
                Console.Write(e.Message);
                if (_repository.ProfileExists(profile.Id))
                {
                    return StatusCode(HttpStatusCode.Conflict);
                }
                return NotFound();
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Profiles
        [ResponseType(typeof(Profile))]
        public async Task<IHttpActionResult> PostProfile(Profile profile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _repository.CreateAsync(profile);
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        // DELETE: api/Profiles/5
        [ResponseType(typeof(Profile))]
        public async Task<IHttpActionResult> DeleteProfile(int id)
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


        //helper
        private IHttpActionResult InternalError(Exception e)
        {
            Console.Write(e.Message);
            return StatusCode(HttpStatusCode.InternalServerError);
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