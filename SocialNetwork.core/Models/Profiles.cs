using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.core.Models
{
    public class Profile
    {
        public Profile()
        {
            Followers = new HashSet<Profile>();
            Following = new HashSet<Profile>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string PictureUrl { get; set; }

        public string AccountId { get; set; }

        public virtual ICollection<Profile> Followers { get; set; }
        public virtual ICollection<Profile> Following { get; set; }

        public virtual ICollection<Albuns> Albuns { get; set; }

        public virtual ICollection<Publications> Publication { get; set; }
    }
}
