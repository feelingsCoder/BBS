using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBS.Emtity
{
    public class BBSPost
    {
        public int ? BBSId {get;set;}
        public string BBSTitle { get; set; }
        public string BBSContent { get; set;}
        public string BBSSender { get; set;}
        public DateTime ? BBSSendTime { get; set; }
        public DateTime ? BBSOpTime { get; set; }
        public string BBSIsTop { get; set; }
        public string BBSIdGood { get; set; }
        public string BBSReplayid { get; set; }


    }
}
