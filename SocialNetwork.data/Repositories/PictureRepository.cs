using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.core.PictureEntity;
using SocialNetwork.data.DataContext;
using SocialNetwork.data.Interfces;

namespace SocialNetwork.data.PictureRepository
{
    public class PicturesRepositoryAsync : IRepositoryAsync<Pictures>
    {
        private SocialNetworkDataContext _dataContext;

        public PicturesRepositoryAsync()
        {
            _dataContext = new SocialNetworkDataContext();
        }

        public async Task CreateAsync(Pictures picture)
        {
            _dataContext.Pictures.Add(picture);
            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Pictures picture = _dataContext.Pictures.Find(id);
            if (picture != null)
            {
                _dataContext.Pictures.Remove(picture);
                await _dataContext.SaveChangesAsync();
            }
        }

        public async Task<ICollection<Pictures>> GetAllAsync()
        {
            ICollection<Pictures> pictures = await _dataContext.Pictures.ToListAsync();
            if (pictures.Count > 0)
            {
                return pictures;
            }
            return null;
        }

        public async Task<Pictures> GetByIDAsync(int id)
        {
            return await _dataContext.Pictures.FindAsync(id);
        }

        public async Task UpdateAsync(Pictures picture)
        {
            try
            {
                _dataContext.Entry(picture).State = EntityState.Modified;
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool ProfileExists(int id)
        {
            return _dataContext.Pictures.Count(picture => picture.Id == id) > 0;
        }

       
    }

}
