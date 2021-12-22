using System;
using System.Collections.Generic;
using System.Text;

namespace WrongQuestion
{
    public class RedmineUser
    {
        public long Id { get; private set; }
        public string Name { get; private set; }

        public RedmineUser(long id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
