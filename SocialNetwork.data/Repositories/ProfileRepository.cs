using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.core.Entity;
using SocialNetwork.data.DataContext;
using SocialNetwork.data.Interfces;

namespace SocialNetwork.data.Repository
{
    public class ProfilesRepositoryAsync : IRepositoryAsync<Profiles>
    {
        private SocialNetworkDataContext _dataContext;

        public ProfilesRepositoryAsync()
        {
            _dataContext = new SocialNetworkDataContext();
        }

        public async Task CreateAsync(Profiles profile)
        {
            _dataContext.Profiles.Add(profile);
            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Profiles profile = _dataContext.Profiles.Find(id);
            if (profile != null)
            {
                _dataContext.Profiles.Remove(profile);
                await _dataContext.SaveChangesAsync();
            }
        }

        public async Task<ICollection<Profiles>> GetAllAsync()
        {
            List<Profiles> profiles = await _dataContext.Profiles.ToListAsync();
            return profiles;
        }

        public async Task<Profiles> GetByIDAsync(int id)
        {
            return await _dataContext.Profiles.FindAsync(id);
        }

        public async Task<Profiles> GetByIDAccountAsync(string AccountID)
        {
            return await _dataContext.Profiles.Where(p => p.AccountId == AccountID).Include(p => p.Posts).SingleOrDefaultAsync();
        }

        public async Task UpdateAsync(Profiles profile)
        {
            try
            {
                _dataContext.Entry(profile).State = EntityState.Modified;
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool ProfileExists(int id)
        {
            return _dataContext.Profiles.Count(profile => profile.Id == id) > 0;
        }

       
    }

}
