using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SocialNetwork.web.Models
{
    public class ProfileModelView
    {
        [Required]
        [Display(Name ="Nome")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Sobrenome")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Data de nascimento")]
        public DateTime BirthDate { get; set; }

        //[Required]
        [Display(Name = "Imagem de perfil")]
        public string PictureUrl { get; set; }

        [Required]
        public string AccountId { get; set; }

    }
}