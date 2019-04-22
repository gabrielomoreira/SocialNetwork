using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.core.Entity
{
    public class Pictures
    {
        public Pictures()
        {
            Posts = new HashSet<Posts>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string PictureUrl { get; set; }
        public string Description { get; set; }
        public ICollection<Posts> Posts { get; set; }

        [ForeignKey("ProfileOwnerId")]
        public virtual Profiles ProfileOwner { get; set; }
        public int? ProfileOwnerId { get; set; }
        
    }
}
