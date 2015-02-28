using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp.server
{
    public static class Serialiser
    {
        public static string SerialiseList(List<Guid> list)
        {
            if (list.Count <= 0) return "";
            String s = list[0].ToString();
            for (int i = 1; i < list.Count; i++)
            {
                s += "," + list[i];
            }
            Debug.WriteLine(s);
            return s;
        }

        public static List<Guid> DesiraliseGuidStringList(string s)
        {
            if (String.IsNullOrWhiteSpace(s)) return new List<Guid>();
            return s.Split(',').ToList().Select(str => new Guid(str)).ToList();
        }

        public static string SerialiseDictionary(Dictionary<Guid, Guid> dict)
        {
            if (dict.Count <= 0) return "";
            String s = dict.Keys.ElementAt(0) + ":" + dict.Values.ElementAt(0);
            /*foreach (var pair in dict)
                s = s + ("," + pair.Key + ":" + pair.Value);*/
            for (int i = 1; i < dict.Count; i++)
            {
                s += "," + dict.Keys.ElementAt(i) + ":" + dict.Values.ElementAt(i);
            }
            Debug.WriteLine(s);
            return s;
        }

        public static Dictionary<Guid, Guid> DesiraliseGuidStringDictionary(string s)
        {
            if(String.IsNullOrWhiteSpace(s)) return new Dictionary<Guid, Guid>();
            List<string> sl = s.Split(',').ToList();
            return sl.Select(str => str.Split(':')).ToDictionary(sd => new Guid(sd[0]), sd => new Guid(sd[1]));
        } 
    }
}