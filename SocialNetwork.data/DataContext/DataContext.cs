using SocialNetwork.core.AlbumEntity;
using SocialNetwork.core.PictureEntity;
using SocialNetwork.core.ProfileEntity;
using System.Data.Entity;

namespace SocialNetwork.data.DataContext
{
    public class SocialNetworkDataContext : DbContext
    {
        public SocialNetworkDataContext() : base("SocialNetwork") { }

        public DbSet<Profiles> Profiles { get; set; }
        public DbSet<Albuns> Albuns { get; set; }
        public DbSet<Pictures> Pictures { get; set; }

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

            modelBuilder.Entity<Albuns>()
                .HasMany(m => m.Pictures)
                .WithMany(p => p.Albuns)
                .Map(w => w.ToTable("Albuns_Pictures").MapLeftKey("AlbumID").MapRightKey("PictureID"));

            base.OnModelCreating(modelBuilder);
        }

        
    }
}
