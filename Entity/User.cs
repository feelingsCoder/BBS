using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBS_1_
{
     public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string UserVip { get; set; }
        public DateTime UserCreatedTime { get; set; }
    }
}
