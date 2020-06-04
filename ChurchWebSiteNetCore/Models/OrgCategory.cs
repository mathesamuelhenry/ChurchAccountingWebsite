using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchWebSiteNetCore.Models
{
    public class OrgCategory
    {
        public int OrganizationCategoryId { get; set; }
        
        [Required]
        public int OrganizationId { get; set; }
        
        [Required]
        public string CategoryName { get; set; }
    }
}
