using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.core.Models
{
    public class Publications
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string PictureUrl { get; set; }
        public DateTime PublicationTime { get; set; }

        public virtual ICollection<Publications> Responses { get; set; }

    }
    /*
    public class PublicationProfiles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string PictureUrl { get; set; }
        public DateTime PublicationTime { get; set; }

        public Profile profile;

        public virtual ICollection<Publications> Responses { get; set; }

    }

    public class PublicationPictures
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string PictureUrl { get; set; }
        public DateTime PublicationTime { get; set; }

        public Picture picture;

        public virtual ICollection<Publications> Responses { get; set; }

    }
    */
}