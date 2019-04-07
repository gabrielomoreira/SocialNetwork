using SocialNetwork.core.Models;
using System.Data.Entity;

namespace SocialNetwork.data.ProfileDataContext
{
    public class DataContext : DbContext
    {
        public DataContext() : base("SocialNetwork") { }

        public DbSet<Profile> Profiles { get; set; }

        public static DataContext Create()
        {
            return new DataContext();
        }
    }
}
