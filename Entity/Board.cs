using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBS.Emtity
{
    public class Board
    {
        public int ? BoardId { get; set; }
        public string BoardName { get; set; }
        public string BoardMaster { get; set;}
        public string BoardMemo { get; set; }

    }
}
