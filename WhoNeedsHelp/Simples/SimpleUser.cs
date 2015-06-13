namespace WhoNeedsHelp.Simples
{
    public class SimpleUser
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public SimpleUser(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
