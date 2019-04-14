using SocialNetwork.core.AlbumEntity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.core.PictureEntity
{
    public class Pictures
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string PictureUrl { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Albuns> Albuns { get; set; }
    }
}
