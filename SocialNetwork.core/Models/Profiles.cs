using System;
using System.Data.Entity;

namespace SocialNetwork.core.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string PictureUrl { get; set; }

        public string AccountId { get; set; }

    }
}
