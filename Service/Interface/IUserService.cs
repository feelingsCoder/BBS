using BBS.Emtity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBS.Service.Interface
{
    public interface IUserService
    {

        /// <summary>
        /// 检查用户名密码是否匹配，成功返回用户id，否则返回-1
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>成功返回用户id，否则返回-1</returns>
        int Login(string username, string password);

        /// <summary>
        /// 根据用户id查询用户信息
        /// </summary>
        /// <returns></returns>
        User GetUserById(int id);

        /// <summary>
        /// 添加新用户
        /// </summary>
        /// <param name="user">新用户</param>
        /// <returns>添加是否成功</returns>
        bool AddUser(User user);

        // 暂时不考虑删除用户 因为涉及到级联删除

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="user">新的用户信息</param>
        /// <returns>更新是否成功</returns>
        bool UpdateUser(User user);
    }
}
