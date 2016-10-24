using System.Collections.Generic;
using BBS.Emtity;
using BBS.Service.Interface;
using BBS.DAO.Interface;
using BBS.DAO.Implement;

namespace BBS.Service.Implement
{
    public class UserService : IUserService
    {
        public bool AddUser(User user)
        {
            IUserDAO dao = new UserDAO();
            
            return dao.Add(user);
        }

        public User GetUserById(int id)
        {
            IUserDAO dao = new UserDAO();
            List<User> list = dao.Query(new User() { UserId = id });

            return list == null ? null : list[0];
        }

        public int Login(string username, string password)
        {
            IUserDAO dao = new UserDAO();
            List<User> list = dao.Query(new User() { UserName = username, UserPassword = password });
            return list == null ? -1 : (int)list[0].UserId;
        }

        public bool UpdateUser(User user)
        {
            IUserDAO dao = new UserDAO();
            return dao.Update(user, user.UserId);
        }
    }
}
