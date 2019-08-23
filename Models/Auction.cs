using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// using System.Linq;
// using belt.Models


namespace belt.Models
    {
        public class NoPastDates : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if ((DateTime)value <= DateTime.Now)
                {
                    return new ValidationResult("No Past Dates Allowed!");
                }
                return ValidationResult.Success;
            }
        }
        public class NoUnder13b : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (((DateTime)value).AddYears(13) > DateTime.Now)
                {
                    return new ValidationResult("No One Under 13 Allowed!");
                }
                return ValidationResult.Success;
            }
        }

        public class Auction
        {
            [Key]
            [Column("id")]
            public int AuctionId {get;set;}

            [Required]
            [Display(Name = "Product Name")]
            [MinLength(4, ErrorMessage="Product name must be greater than 3 in length")]
            // [Column("product_name", TypeName="VARCHAR(255)")]
            public string ProductName {get;set;}

            [Required]
            [MinLength(11, ErrorMessage="Product description must be greater than 10 in length")]
            [Display(Name = "Description")]
            // [Column("description", TypeName="VARCHAR(255)")]
            public string Description {get;set;}

            [Required]
            [Display(Name = "Starting Bid")]
            [Range(0.0,100000000.0, ErrorMessage="Starting bid must be greater than zero")]
            // [Column("address", TypeName="VARCHAR(45)")]
            public double HighBid {get;set;}

            [Required]
            [NoPastDates]
            [Display(Name = "End Date")]
            [DataType(DataType.DateTime)]
            public DateTime Date {get;set;}

            // [Required]
            public int CreatorId {get;set;}
            public User Creator {get;set;}

            // [Required]
            public int HighBidderId {get;set;}
            public User HighBidder {get;set;}

        }
    }