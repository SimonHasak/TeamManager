using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TeamManager.Models
{
    public class Player
    {
        public int PlayerId { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(300)]
        public string Description { get; set; }
        public ICollection<TeamAssignment> TeamAssignments { get; set; }
    }
}
