using System.ComponentModel.DataAnnotations;

namespace Clinic.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(50, ErrorMessage = "Category name cannot be longer than 50 characters.")]
        public string CategoryName { get; set; }
    }
}
