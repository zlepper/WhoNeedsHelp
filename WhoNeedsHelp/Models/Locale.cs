using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp.Models
{
    public class Locale
    {
        public int Id { get; set; }
        public string Language { get; set; }
        public string LanguageId { get; set; }
        public virtual ICollection<Translation> Translations { get; set; }
        public virtual ICollection<User> Users { get; set; } 

        public Locale()
        {
            Translations = new List<Translation>();
            Users = new List<User>();
        }

        public Locale(string language, string languageId) : this()
        {
            Language = language;
            LanguageId = languageId;
        }


        public class Translation
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }

            public int LocaleId { get; set; }
            public virtual Locale Locale { get; set; }

            public Translation()
            {
            }

            public Translation(string key, string value, Locale locale)
            {
                Key = key;
                Value = value;
                Locale = locale;
            }
        }
    }
}