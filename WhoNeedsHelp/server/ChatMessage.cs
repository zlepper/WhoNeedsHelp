﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }
        public string Text { get; set; }
        public Guid Author { get; set; }
        public Guid Channel { get; set; }

        public ChatMessage() { }

        public ChatMessage(string text, Guid user, Guid channel)
        {
            Text = text;
            Author = user;
            Channel = channel;
            using (var db = new HelpContext())
            {
                Id = db.GenerateNewGuid(HelpContext.Modes.ChatMessage);
            }
        }
    }
}