using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.core.Entity;
using SocialNetwork.data.DataContext;

namespace SocialNetwork.data.Repository
{
    public class PictureRepositoryAsync
    {
        private SocialNetworkDataContext _dataContext;

        public PictureRepositoryAsync()
        {
            _dataContext = new SocialNetworkDataContext();
        }

        public async Task<Pictures> GetImage(int id)
        {
            return await _dataContext.Pictures.Where(p => p.Id == id).SingleOrDefaultAsync();
        }

        public async Task UpdateAsync(Pictures pictures)
        {
            try
            {
                _dataContext.Entry(pictures).State = EntityState.Modified;
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }

}
