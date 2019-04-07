using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialNetwork.data.Interfces
{
    public interface IRepositoryAsync<T>
    {
        Task<ICollection<T>> GetAllAsync();
        Task<T> GetByIDAsync(int id);
        Task UpdateAsync(T item);
        Task DeleteAsync(int id);
        Task CreateAsync(T item);
    }

}
