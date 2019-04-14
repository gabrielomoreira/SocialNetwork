using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.web.Models.Profile
{
    public class ProfileViewModel
    {

        public ProfileViewModel()
        {
            Followers = new HashSet<ProfileViewModel>();
            Following = new HashSet<ProfileViewModel>();
        }

        public int Id{ get; set; }

        [Required]
        [Display(Name ="Nome")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Sobrenome")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Data de nascimento")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Imagem de perfil")]
        public string PictureUrl { get; set; }

        public string AccountId { get; set; }

        [Display(Name = "Followers")]
        public virtual ICollection<ProfileViewModel> Followers { get; set; }

        [Display(Name = "Following")]
        public virtual ICollection<ProfileViewModel> Following { get; set; }

        public bool IsFriend { get; set; }

    }

    public class AlbumViewModel
    {
        public AlbumViewModel()
        {
            Pictures = new HashSet<PictureViewModel>();
        }

        public int Id { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Pictures")]
        public virtual ICollection<PictureViewModel> Pictures { get; set; }
    }

    public class PictureViewModel
    {

        public int Id { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Picture")]
        public string PictureUrl { get; set; }
    }

}