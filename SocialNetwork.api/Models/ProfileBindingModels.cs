using System;

namespace SocialNetwork.api.Models
{
    public class ProfileBindingModels
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string PictureUrl { get; set; }

        public string AccountId{ get; set; }
    }
}