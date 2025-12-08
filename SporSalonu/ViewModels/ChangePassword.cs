using System.ComponentModel.DataAnnotations;

namespace SporSalonu.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Eski şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Eski Şifre")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Yeni şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Tekrar")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmNewPassword { get; set; }
    }
}