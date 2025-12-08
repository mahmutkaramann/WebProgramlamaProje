using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SporSalonu.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Email alanı zorunludur.")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Beni hatırla?")]
        public bool RememberMe { get; set; }
    }
}
