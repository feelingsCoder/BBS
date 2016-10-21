using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBS.DAO
{
    public interface IBaseDAO<T>
    {
        /// <summary>
        /// 获取或设置数据库连接字符串, 默认值为配置文件中名为 Database 的项对应的数据库连接字符串
        /// </summary>
        string ConnectionString { set; get; }

        /// <summary>
        /// 基础查询模板
        /// </summary>
        /// <typeparam name="R">返回值类型</typeparam>
        /// <param name="sql">sql语句或存储过程名</param>
        /// <param name="QueryFunction">查询方法</param>
        /// <param name="parameters">参数数组</param>
        /// <param name="commandType">sql类型</param>
        /// <returns></returns>
        R Execute<R>(string sql, Func<R> QueryFunction, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text);

        #region 执行SQL语句方法

        /// <summary>
        /// 执行T-SQL语句或存储过程并返回受影响行数 查询失败返回-1
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="parameters">T-SQL语句或存储过程中对应的参数的数组</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>受影响行数</returns>
        int ExecuteNonQuery(string sql, SqlParameter[] parameters, CommandType commandType);

        /// <summary>
        /// 执行T-SQL语句或存储过程，并返回结果集的第一行第一列
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="parameters">T-SQL语句或存储过程中对应的参数的数组</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>结果集的第一行第一列</returns>
        object ExecuteScalar(string sql, SqlParameter[] parameters, CommandType commandType);

        /// <summary>
        /// 执行T-SQL语句或存储过程，并返回一个 T 类型的对象集合
        /// <para>使用该方法要求T-SQL语句中的列名必须在 T 类型中有对应的属性，且该属性名应与列名或列的别名一致</para>
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="parameters">T-SQL语句或存储过程中对应的参数的数组</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>一个集合</returns>
        List<T> ExecuteReader(string sql, SqlParameter[] parameters, CommandType commandType);

        /// <summary>
        /// 执行T-SQL语句或存储过程，并返回一个 DataSet
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="parameters">T-SQL语句或存储过程中对应的参数的数组</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>DataSet</returns>
        DataSet ExecuteDataSet(string sql, SqlParameter[] parameters, CommandType commandType);

        #region ExecuteNonQuery() 重载

        /// <summary>
        /// 执行无参数的T-SQL语句或存储过程并返回受影响行数 查询失败返回-1
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>受影响行数</returns>
        int ExecuteNonQuery(string sql, CommandType commandType);

        /// <summary>
        /// 执行带参数的T-SQL语句并返回受影响行数 查询失败返回-1
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <param name="parameters">T-SQL语句对应的参数的数组</param>
        /// <returns>受影响行数</returns>
        int ExecuteNonQuery(string sql, SqlParameter[] parameters);

        /// <summary>
        /// 执行无参数的T-SQL语句并返回受影响行数 查询失败返回-1
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <returns>受影响行数</returns>
        int ExecuteNonQuery(string sql);

        #endregion

        #region ExecuteScalar() 重载
        
        /// <summary>
        /// 执行无参数的T-SQL语句或存储过程，并返回结果集的第一行第一列
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>结果集的第一行第一列</returns>
        object ExecuteScalar(string sql, CommandType commandType);

        /// <summary>
        /// 执行带参数的T-SQL语句，并返回结果集的第一行第一列
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <param name="parameters">T-SQL语句对应的参数的数组</param>
        /// <returns>结果集的第一行第一列</returns>
        object ExecuteScalar(string sql, SqlParameter[] parameters);

        /// <summary>
        /// 执行无参数的T-SQL语句，并返回结果集的第一行第一列
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <returns>结果集的第一行第一列</returns>
        object ExecuteScalar(string sql);

        #endregion

        #region ExecuteReader() 重载
        
        /// <summary>
        /// 执行无参数的T-SQL语句或存储过程，并返回一个 T 类型的对象集合
        /// <para>使用该方法要求T-SQL语句中的列名必须在 T 类型中有对应的属性，且该属性名应与列名或列的别名一致</para>
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>一个集合</returns>
        List<T> ExecuteReader(string sql, CommandType commandType);

        /// <summary>
        /// 执行带参数的T-SQL语句，并返回一个 T 类型的对象集合
        /// <para>使用该方法要求T-SQL语句中的列名必须在 T 类型中有对应的属性，且该属性名应与列名或列的别名一致</para>
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <param name="parameters">T-SQL语句中对应的参数的数组</param>
        /// <returns>一个集合</returns>
        List<T> ExecuteReader(string sql, SqlParameter[] parameters);

        /// <summary>
        /// 执行无参数的T-SQL语句，并返回一个 T 类型的对象集合
        /// <para>使用该方法要求T-SQL语句中的列名必须在 T 类型中有对应的属性，且该属性名应与列名或列的别名一致</para>
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <returns>一个集合</returns>
        List<T> ExecuteReader(string sql);

        #endregion

        #region ExecuteDataSet() 重载
        
        /// <summary>
        /// 执行无参数的T-SQL语句或存储过程，并返回一个 DataSet
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>DataSet</returns>
        DataSet ExecuteDataSet(string sql, CommandType commandType);
        /// <summary>
        /// 执行带参数的T-SQL语句，并返回一个 DataSet
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <param name="parameters">T-SQL语句中对应的参数的数组</param>
        /// <returns>DataSet</returns>
        DataSet ExecuteDataSet(string sql, SqlParameter[] parameters);
        /// <summary>
        /// 执行无参数的T-SQL语句，并返回一个 DataSet
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <returns>DataSet</returns>
        DataSet ExecuteDataSet(string sql);

        #endregion

        #endregion

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
