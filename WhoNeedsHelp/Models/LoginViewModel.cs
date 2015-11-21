﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool RememberMe { get; set; }
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

        [Compare("Email", ErrorMessage = "Emailadresser stemmer ikke overens.")]
        [Display(Name = "Bekræft email")]
        public string RepeatEmail { get; set; }

        [Required(ErrorMessage = "Du skal indtaste et kodeord.")]
        [DataType(DataType.Password)]
        [Display(Name = "Kodeord")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Du skal gentage dit kodeord, så du er sikker på at have tastet det rigtigt. ")]
        [Compare("Password", ErrorMessage = "Kodeord stemmer ikke overens.")]
        [Display(Name = "Bekræft kodeord")]
        [DataType(DataType.Password)]
        public string RepeatPassword { get; set; }
        
        [Display(Name = "Husk mig")]
        public bool RememberMe { get; set; }
    }

    public class AuthViewModel
    {
        public LoginViewModel LoginViewModel { get; set; }
        public SignupViewModel SignupViewModel { get; set; }

    }

    public class PrincipalSerializeModel
    {
        public int UserId { get; set; }
        public string[] Roles { get; set; }
    }
}