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

        [Required(ErrorMessage = "Have to supply Question 1")]
        public int Question1 { get; set; }
        [Required(ErrorMessage = "Have to supply Answer 1")]
        public string Answer1 { get; set; }
        [Required(ErrorMessage = "Have to supply Question 2")]
        public int Question2 { get; set; }
        [Required(ErrorMessage = "Have to supply Answer 2")]
        public string Answer2 { get; set; }
        [Required(ErrorMessage = "Have to supply Question 3")]
        public int Question3 { get; set; }
        [Required(ErrorMessage = "Have to supply Answer 3")]
        public string Answer3 { get; set; }

        [Required(ErrorMessage = "Have to supply Organization name")]
        public string OrganizationName {get; set;}

        [Required(ErrorMessage = "Have to supply Industry")]
        public int IndustryId { get; set; }

        public string OrgEmail { get; set; }
        public string OrgPhone { get; set; }
    }

    public class IndustryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class QuestionsModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
