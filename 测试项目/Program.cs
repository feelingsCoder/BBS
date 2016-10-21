using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBS.Emtity;
using BBS.Service.Interface;
using BBS.Service.Implement;

namespace 测试项目
{
    class Program
    {
        static void Main(string[] args)
        {
            IUserService s = new UserService();
            User u = s.GetUserById(1);
            Console.WriteLine(u);
        }
    }
}
