using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace WhoNeedsHelp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Du skal indtaste en emailadresse.")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Det indtastede var ikke en emailadresse.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Du skal indtaste et kodeord.")]
        [DataType(DataType.Password)]
        [Display(Name = "Kodeord")]
        public string Password { get; set; }
        
        [Display(Name = "Husk mig")]
        public string RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class SignupViewModel
    {
        [Required(ErrorMessage = "Du skal have et navn.")]
        [Display(Name = "Navn")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Du skal indtaste en emailadresse.")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Det indtastede var ikke en emailaddresse.")]
        public string Email { get; set; }

        [System.ComponentModel.DataAnnotations.Compare("Email", ErrorMessage = "Emailadresser stemmer ikke overens.")]
        [Display(Name = "Bekræft email")]
        public string RepeatEmail { get; set; }

        [Required(ErrorMessage = "Du skal indtaste et kodeord.")]
        [DataType(DataType.Password)]
        [Display(Name = "Kodeord")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Du skal gentage dit kodeord, så du er sikker på at have tastet det rigtigt. ")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Kodeord stemmer ikke overens.")]
        [Display(Name = "Bekræft kodeord")]
        [DataType(DataType.Password)]
        public string RepeatPassword { get; set; }
        
        [Display(Name = "Husk mig")]
        public string RememberMeSignup { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class AuthViewModel
    {
        public LoginViewModel LoginViewModel { get; set; }
        public SignupViewModel SignupViewModel { get; set; }

    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Du skal indtaste en emailadresse")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Den indtastede værdi er ikke en emailadresse. ")]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel2
    {
        [HiddenInput]
        public string ResetKey { get; set; }


        [Required(ErrorMessage = "Du skal indtaste et kodeord.")]
        [DataType(DataType.Password)]
        [Display(Name = "Kodeord")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Du skal gentage dit kodeord, så du er sikker på at have tastet det rigtigt. ")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Kodeord stemmer ikke overens.")]
        [Display(Name = "Bekræft kodeord")]
        [DataType(DataType.Password)]
        public string RepeatPassword { get; set; }

        [HiddenInput]
        public string Email { get; set; }
    }

    public class PrincipalSerializeModel
    {
        public int UserId { get; set; }
        public string[] Roles { get; set; }
    }
}
