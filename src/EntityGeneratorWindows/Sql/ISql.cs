using EntityGeneratorWindows.Model;
using System.Collections.Generic;
using System.Data;

namespace EntityGeneratorWindows.Sql
{
    internal interface ISql
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        IDbConnection GetConnection(string dbName);

        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <returns></returns>
        IEnumerable<DataBaseInfo> GetDBList();

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        IEnumerable<ClassInfo> GetTableList(string dbName);

        /// <summary>
        /// 获取表中所有字段
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="classinfo"></param>
        /// <returns></returns>
        IEnumerable<FieldInfo> GetTableFieldList(string dbName, ClassInfo classinfo);

    }
}
