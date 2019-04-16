using SocialNetwork.core.Entity;
using System.Data.Entity;

namespace SocialNetwork.data.DataContext
{
    public class SocialNetworkDataContext : DbContext
    {
        public SocialNetworkDataContext() : base("SocialNetwork") { }

        public DbSet<Profiles> Profiles { get; set; }
        public DbSet<Pictures> Pictures { get; set; }
        public DbSet<Posts> Posts { get; set; }

        public static SocialNetworkDataContext Create()
        {
            return new SocialNetworkDataContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Profiles>()
                .HasMany(m => m.Followers)
                .WithMany(p => p.Following)
                .Map(w => w.ToTable("Profiles_Relationships").MapLeftKey("ProfileID").MapRightKey("FollowerID"));

            base.OnModelCreating(modelBuilder);
        }

    }
}
