using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.core.AlbumEntity;
using SocialNetwork.data.DataContext;
using SocialNetwork.data.Interfces;

namespace SocialNetwork.data.AlbumRepository
{
    public class AlbumRepositoryAsync : IRepositoryAsync<Albuns>
    {
        private SocialNetworkDataContext _dataContext;

        public AlbumRepositoryAsync()
        {
            _dataContext = new SocialNetworkDataContext();
        }

        public async Task CreateAsync(Albuns album)
        {
            _dataContext.Albuns.Add(album);
            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Albuns album = _dataContext.Albuns.Find(id);
            if (album != null)
            {
                _dataContext.Albuns.Remove(album);
                await _dataContext.SaveChangesAsync();
            }
        }

        public async Task<ICollection<Albuns>> GetByIDAccountAsync(string AccountId)
        {
            ICollection<Albuns> albuns = await _dataContext.Albuns.Where(album => album.Profile.AccountId == AccountId).ToListAsync();
            if (albuns.Count > 0)
            {
                return albuns;
            }
            return null;
        }

        public async Task<Albuns> GetByIDAsync(int id)
        {
            return await _dataContext.Albuns.FindAsync(id);
        }
        
        public async Task<ICollection<Albuns>> GetAllByProfileAsync(int profileId)
        {
            ICollection<Albuns> albuns = await _dataContext.Albuns.Where(album => album.Id == profileId).ToListAsync();
            if (albuns.Count > 0)
            {
                return albuns;
            }
            return null;
        }

        public async Task<ICollection<Albuns>> GetAllAsync()
        {
            ICollection<Albuns> albuns = await _dataContext.Albuns.ToListAsync();
            if (albuns.Count > 0)
            {
                return albuns;
            }
            return null;
        }

        public async Task UpdateAsync(Albuns album)
        {
            try
            {
                _dataContext.Entry(album).State = EntityState.Modified;
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool AlbumExists(int id)
        {
            return _dataContext.Albuns.Count(album => album.Id == id) > 0;
        }

       
    }

}
