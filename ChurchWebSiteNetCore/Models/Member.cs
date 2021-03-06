﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchWebSiteNetCore.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Required]
        public int OrganizationId { get; set; }
        
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
        
        public string FamilyName { get; set; }
    }
}
