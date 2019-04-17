using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.web.Models.Profile
{
    public class PostsViewModel
    {

        public PostsViewModel()
        {
            Responses = new HashSet<PostsViewModel>();
        }

        public int Id{ get; set; }

        [Display(Name = "Comment")]
        public string TextPost { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DatePost { get; set; }

        [Display(Name = "Imagem post")]
        public PictureViewModel PictureVM { get; set; }

        public ICollection<PostsViewModel> Responses { get; set; }

        [Required]
        public ProfileViewModel ProfileAuthor { get; set; }
        [Required]
        public ProfileViewModel ProfileOwner { get; set; }

    }
    

}