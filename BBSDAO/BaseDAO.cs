using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace BBS.DAO
{
    public abstract class BaseDAO<T> : IBaseDAO<T>, IDAO<T>
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["BBS"].ConnectionString;
        private SqlConnection connection = null;
        private SqlCommand command = null;
        private SqlTransaction transaction = null;
        private SqlDataReader reader = null;

        public BaseDAO()
        {

        }

        public BaseDAO(BaseDAO<T> other)
        {
            connection = other.connection;
            command = other.command;
            transaction = other.transaction;
        }

        /// <summary>
        /// 获取或设置数据库连接字符串, 默认值为配置文件中名为 Database 的项对应的数据库连接字符串
        /// </summary>
        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        /// <summary>
        /// 初始化连接
        /// </summary>
        private void InitSqlConnection()
        {
            connection = new SqlConnection(ConnectionString);
            connection.Open();

            command = new SqlCommand();
            command.Connection = connection;
        }

        /// <summary>
        /// 基础查询模板
        /// </summary>
        /// <typeparam name="R">返回值类型</typeparam>
        /// <param name="sql">sql语句或存储过程名</param>
        /// <param name="QueryFunction">查询方法</param>
        /// <param name="parameters">参数数组</param>
        /// <param name="commandType">sql类型</param>
        /// <returns></returns>
        public R Execute<R>(string sql, Func<R> QueryFunction, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            try
            {
                if ((connection == null || connection.State != ConnectionState.Open) && transaction == null)
                {
                    InitSqlConnection();
                }
                //设置T-SQL语句或存储过程
                command.CommandText = sql;
                //设置命令字符串类型（SQL命令或者存储过程，默认为SQL命令）
                command.CommandType = commandType;
                // 清除SqlCommand的参数
                command.Parameters.Clear();
                //设置参数
                if (parameters != null)
                {
                    // 预处理SqlParameter参数数组，将为NULL的参数赋值为DBNull.Value;
                    foreach (SqlParameter parameter in parameters)
                    {
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }
                    }
                    //讲参数数组添加到command对象的参数集合的末尾
                    command.Parameters.AddRange(parameters);
                }

                R result = QueryFunction();

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                //如果没有执行事务操作，则关闭数据库连接
                if (transaction == null && connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        #region 内置增删改查方法

        /// <summary>
        /// 根据对象o的非空属性添加一条记录
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>添加是否成功</returns>
        virtual public bool Add(T o)
        {
            //获取 T 的类型
            Type type = typeof(T);
            //获取 T 类型的所有公共属性
            List<PropertyInfo> properties = new List<PropertyInfo>(type.GetProperties());
            //生成sql语句
            StringBuilder sql = new StringBuilder("insert into [");
            sql.Append(type.Name);
            sql.Append("]([");
            StringBuilder temp = new StringBuilder("]) values(");
            //绑定参数
            List<SqlParameter> parameters = new List<SqlParameter>();
            bool isFirst = true; //确定是否为第一个非空属性
            foreach (PropertyInfo p in properties)
            {
                //寻找非空属性
                if (isEmpty(o, p))
                {
                    //如果不是第一个非空属性 添加分隔符
                    if (!isFirst)
                    {
                        sql.Append("], [");
                        temp.Append(", ");
                    }
                    sql.Append(p.Name);
                    temp.Append("@");
                    temp.Append(p.Name);
                    parameters.Add(new SqlParameter("@" + p.Name, p.GetValue(o)));
                    isFirst = false;
                }
            }
            temp.Append(");");
            //将temp跟sql合成一个字符串
            sql.Append(temp.ToString());

            //执行查询
            return ExecuteNonQuery(sql.ToString(), parameters.ToArray()) == 1;
        }

        /// <summary>
        /// 根据对象o的非空属性删除一条或多条记录
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns></returns>
        virtual public bool Delete(T o)
        {
            //获取 T 的类型
            Type type = typeof(T);
            //获取 T 类型的所有公共属性
            List<PropertyInfo> properties = new List<PropertyInfo>(type.GetProperties());
            //生成sql语句
            StringBuilder sql = new StringBuilder("delete from [");
            sql.Append(type.Name);
            sql.Append("] where [");
            //绑定参数
            List<SqlParameter> parameters = new List<SqlParameter>();
            bool isFirst = true; //确定是否为第一个非空属性
            foreach (PropertyInfo p in properties)
            {
                //寻找非空属性
                if (isEmpty(o,p))
                {
                    //如果不是第一个非空属性 添加分隔符
                    if (!isFirst)
                    {
                        sql.Append(" and [");
                    }
                    //添加列名
                    sql.Append(p.Name);
                    sql.Append("] = @");
                    //添加值
                    sql.Append(p.Name);
                    //绑定参数
                    parameters.Add(new SqlParameter("@" + p.Name, p.GetValue(o)));
                    isFirst = false;
                }
            }

            //执行查询
            return ExecuteNonQuery(sql.ToString(), parameters.ToArray()) > 0;
        }

        /// <summary>
        /// 根据对象o的key更新一条记录
        /// </summary>
        /// <param name="o">对象</param>
        /// <param name="key">主键名</param>
        /// <returns></returns>
        virtual public bool Update(T o, object key)
        {
            //获取 T 的类型
            Type type = typeof(T);
            //获取 T 类型的所有公共属性
            List<PropertyInfo> properties = new List<PropertyInfo>(type.GetProperties());
            // UPDATE 表名称 SET 列名称 = 新值 WHERE 列名称 = 某值
            //生成sql语句
            StringBuilder sql = new StringBuilder("UPDATE [");
            //添加表名
            sql.Append(type.Name);
            sql.Append("] set [");

            StringBuilder temp = new StringBuilder(" where ");
            //绑定参数
            List<SqlParameter> parameters = new List<SqlParameter>();
            bool isFirst = true; //确定是否为第一个非空属性
            foreach (PropertyInfo p in properties)
            {
                //寻找非空且不是主键的属性
                if (isEmpty(o, p))
                {
                    if (p.Name == key.ToString())
                    {
                        //添加列名
                        temp.Append(key);
                        temp.Append(" = @");
                        //添加值
                        temp.Append(p.Name);
                    }
                    else
                    {
                        //如果不是第一个非空属性 添加分隔符
                        if (!isFirst)
                        {
                            sql.Append(", [");
                        }
                        //添加列名
                        sql.Append(p.Name);
                        sql.Append("] = @");
                        //添加值
                        sql.Append(p.Name);
                    }
                    //绑定参数
                    parameters.Add(new SqlParameter("@" + p.Name, p.GetValue(o)));
                    isFirst = false;
                }
            }
            sql.Append(temp.ToString());

            //执行查询
            return ExecuteNonQuery(sql.ToString(), parameters.ToArray()) == 1;
        }

        /// <summary>
        /// 根据对象o的非空属性查询一个对象列表
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns></returns>
        virtual public List<T> Query(T o)
        {
            List<T> list = null;

            //获取 T 的类型
            Type type = typeof(T);
            //获取 T 类型的所有公共属性
            List<PropertyInfo> properties = new List<PropertyInfo>(type.GetProperties());
            //生成sql语句
            StringBuilder sql = new StringBuilder("select * from [");
            sql.Append(type.Name);
            sql.Append("] where [");
            //绑定参数
            List<SqlParameter> parameters = new List<SqlParameter>();
            bool isFirst = true; //确定是否为第一个非空属性
            foreach (PropertyInfo p in properties)
            {
                //寻找非空属性
                if (isEmpty(o, p))
                {
                    //如果不是第一个非空属性 添加分隔符
                    if (!isFirst)
                    {
                        sql.Append(" and [");
                    }
                    //添加列名
                    sql.Append(p.Name);
                    sql.Append("] = @");
                    //添加值
                    sql.Append(p.Name);
                    //绑定参数
                    parameters.Add(new SqlParameter("@" + p.Name, p.GetValue(o)));
                    isFirst = false;
                }
            }
            list = ExecuteReader(sql.ToString(), parameters.ToArray());
            return list;
        }

        /// <summary>
        /// 检查对象o的p属性是否为空
        /// </summary>
        /// <param name="o">对象</param>
        /// <param name="p">属性</param>
        /// <returns></returns>
        private bool isEmpty(T o ,PropertyInfo p)
        {
            return p.GetValue(o) != null && (!p.GetValue(o).GetType().IsValueType || double.Parse(p.GetValue(o).ToString()) != 0);
        }

        #endregion

        #region 公有方法

        /// <summary>
        /// 执行T-SQL语句或存储过程并返回受影响行数 查询失败返回-1
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="parameters">T-SQL语句或存储过程中对应的参数的数组</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>受影响行数</returns>
        public int ExecuteNonQuery(string sql, SqlParameter[] parameters, CommandType commandType)
        {
            return Execute(sql, ExecuteNonQuery, parameters, commandType);
        }

        /// <summary>
        /// 执行T-SQL语句或存储过程，并返回结果集的第一行第一列
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="parameters">T-SQL语句或存储过程中对应的参数的数组</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>结果集的第一行第一列</returns>
        public object ExecuteScalar(string sql, SqlParameter[] parameters, CommandType commandType)
        {
            return Execute(sql, ExecuteScalar, parameters, commandType);
        }

        /// <summary>
        /// 执行T-SQL语句或存储过程，并返回一个 T 类型的对象集合
        /// <para>使用该方法要求T-SQL语句中的列名必须在 T 类型中有对应的属性，且该属性名应与列名或列的别名一致</para>
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="parameters">T-SQL语句或存储过程中对应的参数的数组</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>一个集合</returns>
        public List<T> ExecuteReader(string sql, SqlParameter[] parameters, CommandType commandType)
        {
            return Execute(sql, ExecuteReader, parameters, commandType);
        }

        /// <summary>
        /// 执行T-SQL语句或存储过程，并返回一个 DataSet
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="parameters">T-SQL语句或存储过程中对应的参数的数组</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataSet(string sql, SqlParameter[] parameters, CommandType commandType)
        {
            return Execute(sql, ExecuteDataSet, parameters, commandType);
        }

        #endregion

        #region 重载方法

        //ExecuteNonQuery
        /// <summary>
        /// 执行无参数的T-SQL语句或存储过程并返回受影响行数 查询失败返回-1
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>受影响行数</returns>
        public int ExecuteNonQuery(string sql, CommandType commandType)
        {
            return ExecuteNonQuery(sql, null, commandType);
        }

        /// <summary>
        /// 执行带参数的T-SQL语句并返回受影响行数 查询失败返回-1
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <param name="parameters">T-SQL语句对应的参数的数组</param>
        /// <returns>受影响行数</returns>
        public int ExecuteNonQuery(string sql, SqlParameter[] parameters)
        {
            return ExecuteNonQuery(sql, parameters, CommandType.Text);
        }

        /// <summary>
        /// 执行无参数的T-SQL语句并返回受影响行数 查询失败返回-1
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <returns>受影响行数</returns>
        public int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, null, CommandType.Text);
        }

        //ExecuteScalar
        /// <summary>
        /// 执行无参数的T-SQL语句或存储过程，并返回结果集的第一行第一列
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>结果集的第一行第一列</returns>
        public object ExecuteScalar(string sql, CommandType commandType)
        {
            return ExecuteScalar(sql, null, commandType);
        }

        /// <summary>
        /// 执行带参数的T-SQL语句，并返回结果集的第一行第一列
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <param name="parameters">T-SQL语句对应的参数的数组</param>
        /// <returns>结果集的第一行第一列</returns>
        public object ExecuteScalar(string sql, SqlParameter[] parameters)
        {
            return ExecuteScalar(sql, parameters, CommandType.Text);
        }

        /// <summary>
        /// 执行无参数的T-SQL语句，并返回结果集的第一行第一列
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <returns>结果集的第一行第一列</returns>
        public object ExecuteScalar(string sql)
        {
            return ExecuteScalar(sql, null, CommandType.Text);
        }

        //ExecuteReader
        /// <summary>
        /// 执行无参数的T-SQL语句或存储过程，并返回一个 T 类型的对象集合
        /// <para>使用该方法要求T-SQL语句中的列名必须在 T 类型中有对应的属性，且该属性名应与列名或列的别名一致</para>
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>一个集合</returns>
        public List<T> ExecuteReader(string sql, CommandType commandType)
        {
            return ExecuteReader(sql, null, commandType);
        }

        /// <summary>
        /// 执行带参数的T-SQL语句，并返回一个 T 类型的对象集合
        /// <para>使用该方法要求T-SQL语句中的列名必须在 T 类型中有对应的属性，且该属性名应与列名或列的别名一致</para>
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <param name="parameters">T-SQL语句中对应的参数的数组</param>
        /// <returns>一个集合</returns>
        public List<T> ExecuteReader(string sql, SqlParameter[] parameters)
        {
            return ExecuteReader(sql, parameters, CommandType.Text);
        }

        /// <summary>
        /// 执行无参数的T-SQL语句，并返回一个 T 类型的对象集合
        /// <para>使用该方法要求T-SQL语句中的列名必须在 T 类型中有对应的属性，且该属性名应与列名或列的别名一致</para>
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <returns>一个集合</returns>
        public List<T> ExecuteReader(string sql)
        {
            return ExecuteReader(sql, null, CommandType.Text);
        }

        //ExecuteDataSet
        /// <summary>
        /// 执行无参数的T-SQL语句或存储过程，并返回一个 DataSet
        /// </summary>
        /// <param name="sql">T-SQL语句或存储过程名</param>
        /// <param name="commandType">CommandText的类型</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataSet(string sql, CommandType commandType)
        {
            return ExecuteDataSet(sql, null, commandType);
        }

        /// <summary>
        /// 执行带参数的T-SQL语句，并返回一个 DataSet
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <param name="parameters">T-SQL语句中对应的参数的数组</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataSet(string sql, SqlParameter[] parameters)
        {
            return ExecuteDataSet(sql, parameters, CommandType.Text);
        }

        /// <summary>
        /// 执行无参数的T-SQL语句，并返回一个 DataSet
        /// </summary>
        /// <param name="sql">T-SQL语句</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataSet(string sql)
        {
            return ExecuteDataSet(sql, null, CommandType.Text);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 执行查询并返回受影响行数
        /// </summary>
        /// <returns></returns>
        protected int ExecuteNonQuery()
        {
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行查询并生成一个对象
        /// </summary>
        /// <returns></returns>
        protected object ExecuteScalar()
        {
            return command.ExecuteScalar();
        }

        /// <summary>
        /// 执行查询并生成一个对象集合
        /// </summary>
        /// <returns></returns>
        protected List<T> ExecuteReader()
        {
            //将T-SQL文本发送给SqlConnection，并生成一个 SqlDataReader
            reader = command.ExecuteReader();
            //定义集合 List<T>
            List<T> list = new List<T>();
            //获取 T 的类型
            Type type = typeof(T);
            //获取 T 类型的所有公共属性
            List<PropertyInfo> properties = new List<PropertyInfo>(type.GetProperties());
            //循环读取所有行
            while (reader.Read())
            {
                //用Activator的CreateInstance静态方法，生成新对象 
                T obj = Activator.CreateInstance<T>();
                //遍历所有列
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    //获取与该列对应的属性
                    PropertyInfo p = properties.Find(x => x.Name.Equals( reader.GetName(i), StringComparison.OrdinalIgnoreCase));
                    //将该列的值赋值给改属性
                    if (p != null)
                    {
                        p.SetValue(obj, DBNull.Value.Equals(reader[i]) ? null : reader[i], null);
                    }
                    else throw new Exception($"{reader.GetName(i)}列在{type}中没有对应的属性。");
                }
                //将该对象加入List集合
                list.Add(obj);
            }
            //关闭SqlDataReader对象
            reader.Close();
            //返回list
            return list;
        }

        /// <summary>
        /// 执行查询并生成一个DataSet
        /// </summary>
        /// <returns></returns>
        protected DataSet ExecuteDataSet()
        {
            //使用 command 生成 SqlDataAdapter 对象
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            //实例化一个DataSet用于存储数据表
            DataSet set = new DataSet();
            //执行查询，并在DataSet中添加查询到的数据
            adapter.Fill(set);
            //返回set
            return set;
        }

        #endregion

        #region 事务操作方法

        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTransaction()
        {
            InitSqlConnection();
            transaction = connection.BeginTransaction();
            command.Transaction = transaction;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            transaction.Commit();
            transaction = null;
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            transaction.Rollback();
            transaction = null;
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }

        #endregion
    }
}