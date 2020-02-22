using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ChurchWebSiteNetCore.Models.Auth
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Have to supply first name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Have to supply last name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Have to supply an e-mail address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Have to supply a password")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The repeat password did not seem correct")]
        public string RepeatPassword { get; set; }
    }

    public class QuestionAnswer
    {
        public string QuestionId { get; set; }
        public string Answer { get; set; }
    }
}
