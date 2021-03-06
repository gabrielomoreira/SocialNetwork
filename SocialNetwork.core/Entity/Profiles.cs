﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.core.Entity
{
    public class Profiles
    {
        public Profiles()
        {
            Followers = new HashSet<Profiles>();
            Following = new HashSet<Profiles>();
            Album = new HashSet<Pictures>();
            Posts = new HashSet<Posts>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string AccountId { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string PictureProfileUrl { get; set; }

        public ICollection<Profiles> Followers { get; set; }
        public ICollection<Profiles> Following { get; set; }

        public ICollection<Pictures> Album { get; set; }
        public ICollection<Posts> Posts { get; set; }
        
    }
}
