using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.core.Entity;
using SocialNetwork.data.DataContext;

namespace SocialNetwork.data.Repository
{
    public class PostsRepositoryAsync
    {
        private SocialNetworkDataContext _dataContext;

        public PostsRepositoryAsync()
        {
            _dataContext = new SocialNetworkDataContext();
        }

        public async Task<ICollection<Posts>> GetAllPost()
        {
            return await _dataContext.Posts.ToListAsync();
        }

        public async Task<ICollection<Posts>> GetAllPostByProfile(int idProfile)
        {
            return await _dataContext.Posts.Where(p => p.ProfileOwner.Id == idProfile).ToListAsync();
        }

        public async Task<Posts> GetPost(int id)
        {
            return await _dataContext.Posts.Where(p => p.Id == id).SingleOrDefaultAsync();
        }

        public async Task DeletePost(int id)
        {
            Posts post = _dataContext.Posts.Find(id);
            if (post != null)
            {
                _dataContext.Posts.Remove(post);
                await _dataContext.SaveChangesAsync();
            }
        }
    }

}
