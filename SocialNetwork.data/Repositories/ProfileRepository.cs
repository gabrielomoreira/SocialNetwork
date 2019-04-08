
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.core.Models;
using SocialNetwork.data.Interfces;
using SocialNetwork.data.ProfileDataContext;

namespace SocialNetwork.data.Repositories
{
    public class ProfileRepositoryAsync : IRepositoryAsync<Profile>
    {
        private DataContext _dataContextAsync;

        public ProfileRepositoryAsync()
        {
            _dataContextAsync = new DataContext();
        }

        public async Task CreateAsync(Profile profile)
        {
            _dataContextAsync.Profiles.Add(profile);
            await _dataContextAsync.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Profile profile = _dataContextAsync.Profiles.Find(id);
            if (profile != null)
            {
                _dataContextAsync.Profiles.Remove(profile);
                await _dataContextAsync.SaveChangesAsync();
            }
        }

        public async Task<ICollection<Profile>> GetAllAsync()
        {
            List<Profile> profiles = await _dataContextAsync.Profiles.ToListAsync();
            if (profiles.Count > 0)
            {
                return profiles;
            }
            return null;
        }

        public async Task<Profile> GetByIDAsync(int id)
        {
            return await _dataContextAsync.Profiles.FindAsync(id);
        }

        public async Task<Profile> GetByIDAccountAsync(string AccountID)
        {
            return await _dataContextAsync.Profiles.Where(p => p.AccountId == AccountID).SingleOrDefaultAsync();
        }

        public async Task UpdateAsync(Profile profile)
        {
            try
            {
                _dataContextAsync.Entry(profile).State = EntityState.Modified;
                await _dataContextAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        public bool ProfileExists(int id)
        {
            return _dataContextAsync.Profiles.Count(profile => profile.Id == id) > 0;
        }



    }

}
