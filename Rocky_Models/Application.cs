using System;
using System.ComponentModel.DataAnnotations;

namespace Rocky_Models
{
    public class Application
    {

        [Key]
        public int Id { get; set; }


        //Validation: No Numerical values in name

        [RegularExpression("^[a-zA-Z]*", ErrorMessage = "No Numerical Values")]

        [Required]
        public String Name { get; set; }

    }
}
