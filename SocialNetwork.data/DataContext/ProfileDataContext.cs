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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Profile>()
                .HasMany(m => m.Followers)
                .WithMany(p => p.Following)
                .Map(w => 
                w.ToTable("Profile_Follow").MapLeftKey("ProfileId").MapRightKey("FollowerID"));

            base.OnModelCreating(modelBuilder);
        }
    }
}
