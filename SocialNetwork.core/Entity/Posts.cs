using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.core.Entity
{
    public class Posts
    {
        public Posts()
        {
            DatePost = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string TextPost { get; set; }
        public DateTime DatePost { get; set; }

        [ForeignKey("PictureOwnerId")]
        public virtual Pictures PictureOwner { get; set; }
        public int? PictureOwnerId { get; set; }

        [ForeignKey("ProfileAuthorId")]
        public virtual Profiles ProfileAuthor { get; set; }
        public int? ProfileAuthorId { get; set; }


    }
}
