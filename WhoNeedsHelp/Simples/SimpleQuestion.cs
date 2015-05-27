namespace WhoNeedsHelp.Simples
{
    public class SimpleQuestion
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public SimpleUser User { get; set; }

        public SimpleQuestion(int id, string text, SimpleUser user)
        {
            Id = id;
            Text = text;
            User = user;
        }
    }
}