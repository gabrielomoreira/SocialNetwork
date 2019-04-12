
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.core.Models;
using SocialNetwork.data.Interfces;
using SocialNetwork.data.ProfileDataContext;

namespace SocialNetwork.data.Repositories
{
    public class AlbumRepositoryAsync : IRepositoryAsync<Albuns>
    {
        private DataContext _dataContextAsync;

        public AlbumRepositoryAsync()
        {
            _dataContextAsync = new DataContext();
        }

        public async Task CreateAsync(Albuns album)
        {
            _dataContextAsync.Albuns.Add(album);
            await _dataContextAsync.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Albuns album = _dataContextAsync.Albuns.Find(id);
            if (album != null)
            {
                _dataContextAsync.Albuns.Remove(album);
                await _dataContextAsync.SaveChangesAsync();
            }
        }

        public async Task<ICollection<Albuns>> GetAllAlbunsProfileIdAsync(Profile profile)
        {
            ICollection<Albuns> albums = await _dataContextAsync.Albuns.Where(x => x.Profile.Id == profile.Id).ToListAsync();
            if (albums.Count > 0)
            {
                return albums;
            }
            return null;
        }

        public async Task<ICollection<Albuns>> GetAllAsync()
        {
            ICollection<Albuns> albums = await _dataContextAsync.Albuns.ToListAsync();
            if (albums.Count > 0)
            {
                return albums;
            }
            return null;
        }

        public async Task<ICollection<Albuns>> GetByIDAsync(int id)
        {
            return await _dataContextAsync.Albuns.FindAsync(id);
        }

        public async Task UpdateAsync(Albuns album)
        {
            try
            {
                _dataContextAsync.Entry(album).State = EntityState.Modified;
                await _dataContextAsync.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        public bool AlbunsExists(int id)
        {
            return _dataContextAsync.Albuns.Count(album => album.Id == id) > 0;
        }

       
    }

}
