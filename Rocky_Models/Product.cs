using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rocky_Models
{
    public class Product
    {


        public Product()
        {
            TempSqFT = 1;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public String Name { get; set; }

        public String ShortDesc { get; set; }
        public String Description { get; set; }


        [Range(1, int.MaxValue)]
        public double Price { get; set; }


        public string Image { get; set; }


        [Display(Name = "Category Type")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }


        [Display(Name = "Application Type")]
        public int ApplicationId { get; set; }

        [ForeignKey("ApplicationId")]
        public virtual Application ApplicationType { get; set; }

        [NotMapped]
        [Range(1,10000, ErrorMessage ="Sqft must be greater than zero")]
        public int TempSqFT { get; set; }

    }
}
