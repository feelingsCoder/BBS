using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBS.DAO
{
    public interface IDAO<T>
    {
        /// <summary>
        /// 获取或设置数据库连接字符串, 默认值为配置文件中名为 Database 的项对应的数据库连接字符串
        /// </summary>
        string ConnectionString { set; get; }

        #region 事务操作方法

        /// <summary>
        /// 开始事务
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();

        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();

        #endregion

        #region 内置方法

        /// <summary>
        /// 根据对象o的非空属性添加一条记录
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>添加是否成功</returns>
        bool Add(T o);

        /// <summary>
        /// 根据对象o的非空属性删除一条或多条记录
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns></returns>
        bool Delete(T o);

        /// <summary>
        /// 根据对象o的key更新一条记录
        /// </summary>
        /// <param name="o">对象</param>
        /// <param name="key">主键名</param>
        /// <returns></returns>
        bool Update(T o, object key);

        /// <summary>
        /// 根据对象o的非空属性查询一个对象列表
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns></returns>
        List<T> Query(T o);
        #endregion
    }
}
