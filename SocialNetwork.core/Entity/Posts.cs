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

        public Profiles ProfileAuthor { get; set; }

    }
}
