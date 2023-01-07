using System.ComponentModel.DataAnnotations;

namespace Chat.Web.ViewModels
{
    public class RoomViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        [RegularExpression(@"^\w+( \w+)*$", ErrorMessage = "Characters allowed: letters, numbers, and one space between words.")]
        public string Name { get; set; }

        public string Admin { get; set; }
    }
}
