using SocialNetwork.core.AlbumEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.core.ProfileEntity
{
    public class Profiles
    {
        public Profiles()
        {
            Followers = new HashSet<Profiles>();
            Following = new HashSet<Profiles>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string AccountId { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string PictureProfileUrl { get; set; }

        public virtual ICollection<Albuns> Albuns { get; set; }
        public virtual ICollection<Profiles> Followers { get; set; }
        public virtual ICollection<Profiles> Following { get; set; }
        
    }
}
