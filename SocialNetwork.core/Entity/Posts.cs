using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.core.Entity
{
    public class Posts
    {
        public Posts()
        {
            Responses = new HashSet<Posts>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string TextPost { get; set; }
        public string PictureUrl { get; set; }
        public DateTime DatePost { get; set; }

        public Pictures Picture { get; set; } 
        public ICollection<Posts> Responses { get; set; }

        public Profiles ProfileAuthor { get; set; }
        public Profiles ProfileOwner { get; set; }
    }
}
