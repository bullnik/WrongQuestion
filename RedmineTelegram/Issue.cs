using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTelegram
{
    public class Issue
    {
        public int Id { get; set; }
        public int AssignedTo { get; set; }
        public bool IsClosed { get; set; }
        public int Status { get; set; }

        public Issue(int id, int assignedTo, bool isClosed, int status)
        {
            Id = id;
            AssignedTo = assignedTo;
            IsClosed = isClosed;
            Status = status;
        }
    }
}
