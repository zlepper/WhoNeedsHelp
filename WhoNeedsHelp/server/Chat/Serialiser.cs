namespace WhoNeedsHelp.Server.Chat
{
    /*public static class Serialiser
    {
        public static string SerialiseList(List<User> list)
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

        public static List<User> DesiraliseGuidStringList(string s)
        {
            if (String.IsNullOrWhiteSpace(s)) return new List<int>();
            return s.Split(',').ToList().Select(Int32.Parse).ToList();
        }

        public static string SerialiseDictionary(Dictionary<int, int> dict)
        {
            if (dict.Count <= 0) return "";
            String s = dict.Keys.ElementAt(0) + ":" + dict.Values.ElementAt(0);
            /*foreach (var pair in dict)
                s = s + ("," + pair.Key + ":" + pair.Value);
            for (int i = 1; i < dict.Count; i++)
            {
                s += "," + dict.Keys.ElementAt(i) + ":" + dict.Values.ElementAt(i);
            }
            Debug.WriteLine(s);
            return s;
        }

        public static Dictionary<int, int> DesiraliseGuidStringDictionary(string s)
        {
            if(String.IsNullOrWhiteSpace(s)) return new Dictionary<int, int>();
            List<string> sl = s.Split(',').ToList();
            return sl.Select(str => str.Split(':')).ToDictionary(sd => Int32.Parse(sd[0]), sd => Int32.Parse(sd[1]));
        } 
    }*/
}