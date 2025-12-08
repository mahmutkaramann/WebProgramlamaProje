using System.ComponentModel.DataAnnotations;

namespace SporSalonu.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="Ad alanı zorunludur.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(20,MinimumLength = 8, ErrorMessage ="Şifre {0} en az {2} ve en fazla {1} karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage ="Şifreler eşleşmedi.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre tekrar alanı zorunludur.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
