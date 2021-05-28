using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchWebSiteNetCore.Models
{
    public class Transaction
    {
        public int? Id { get; set; }
        
        public int Type { get; set; }

        public int MemberId { get; set; }

        public string TransactionName { get; set; }
        
        [Required]
        public int AccountId { get; set; }
        
        [Required]
        public string Category { get; set; }
        
        [Required]
        public int TransactionMode { get; set; }
        
        [Required]
        public int TransactionType { get; set; }
        
        [Required]
        [Range(0.01, 99999999.99, ErrorMessage = "Amount should be greater than 0")]
        public decimal Amount { get; set; }
        
        public string CheckNumber { get; set; }
        
        [Required]
        public DateTime? TransactionDate { get; set; }
        
        public string Note { get; set; }
    }
}
