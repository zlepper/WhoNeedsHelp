using System;
using System.Collections.Generic;

namespace WhoNeedsHelp.Models
{
    public class CleanupAlarm
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public virtual User User { get; set; }

        public int? ChannelId { get; set; }
        public virtual Channel Channel { get; set; }

        public DateTime Time { get; set; }

        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }

        public List<int> GetDays()
        {
            var days = new List<int>();
            if(Sunday) days.Add(0);
            if(Monday) days.Add(1);
            if(Tuesday) days.Add(2);
            if(Wednesday) days.Add(3);
            if(Thursday) days.Add(4);
            if(Friday) days.Add(5);
            if(Saturday) days.Add(6);
            return days;
        }

        public void SetDays(List<int> days)
        {
            Sunday = days.Contains(0);
            Monday = days.Contains(1);
            Tuesday = days.Contains(2);
            Wednesday = days.Contains(3);
            Thursday = days.Contains(4);
            Friday = days.Contains(5);
            Saturday = days.Contains(6);
        }

    }
}
