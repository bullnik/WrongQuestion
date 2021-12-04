namespace WrongQuestion
{
    public class RedmineUser
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public RedmineUser(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}