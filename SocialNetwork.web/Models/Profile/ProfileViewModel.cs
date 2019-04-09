using System;
using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.web.Models.Profile
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name ="Nome")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Sobrenome")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Data de nascimento")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Imagem de perfil")]
        public string PictureUrl { get; set; }

        public string AccountId { get; set; }

    }
}