using SocialNetwork.core.PictureEntity;
using SocialNetwork.core.ProfileEntity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.core.AlbumEntity
{
    public class Albuns
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual Profiles Profile { get; set; }
        public virtual ICollection<Pictures> Pictures { get; set; }
    }
}
