using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.core.PictureEntity;
using SocialNetwork.data.DataContext;

namespace SocialNetwork.data.ProfileRepository
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

    }

}
