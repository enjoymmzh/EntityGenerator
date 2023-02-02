using Dapper;
using EntityGeneratorWindows.Common;
using EntityGeneratorWindows.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EntityGeneratorWindows.Sql
{
    /// <summary>
    /// Mysql数据库
    /// </summary>
    internal class Mysql : ISql
    {
        /// <summary>
        /// 所有数据库
        /// </summary>
        private string allDataBase = "SELECT SCHEMA_NAME AS datname FROM INFORMATION_SCHEMA.SCHEMATA; ";

        /// <summary>
        /// 获取所有的表名
        /// </summary>
        //private string allTableSql = @"select TABLE_NAME tabname, TABLE_COMMENT tcomment from information_schema.TABLES where TABLE_SCHEMA=@dbname and TABLE_TYPE = 'base table';";
        private string allTableSql = @"select TABLE_NAME tabname, TABLE_COMMENT tcomment from information_schema.TABLES where TABLE_SCHEMA=@dbname;";

        /// <summary>
        /// 获取所有的表信息
        /// </summary>
        private string tableInfoSql = @"SELECT
	                                        COLUMN_NAME,
	                                        IS_NULLABLE,
	                                        DATA_TYPE,
	                                        COLUMN_COMMENT, 
	                                        COLUMN_KEY 
                                        FROM
	                                        information_schema.`COLUMNS` 
                                        WHERE
	                                        TABLE_NAME =@tablename
	                                    AND TABLE_SCHEMA =@dbname ";

        public IDbConnection GetConnection(string dbName)
        {
            try
            {
                var str = "server=" + Global.ip + ";port=" + Global.port + ";user id=" + Global.user + ";password=" + Global.password + ";CharacterSet=utf8;";
                if (!string.IsNullOrWhiteSpace(dbName))
                {
                    str += "database=" + dbName + ";";
                }
                var con = new MySqlConnection(str);
                con.Open();
                return con;
            }
            catch (Exception ex)
            {
                TheEvent.ShowMessageBox("糟糕，發生了一個錯誤", "無法連接到數據庫");
                return null;
            }
        }

        public IEnumerable<DataBaseInfo> GetDBList()
        {
            using var con = GetConnection("");
            var list = con.Query<dynamic>(allDataBase).ToList();
            foreach (var item in list)
            {
                var info = new DataBaseInfo()
                {
                    datname = item.datname,
                    size = ""
                };
                yield return info;
            }
        }

        public IEnumerable<ClassInfo> GetTableList(string dbName)
        {
            using var con = GetConnection(dbName);
            if (con is not null)
            {
                var list = con.Query<(string tabname, string tcomment)>(allTableSql, new { dbname = dbName }).ToList();
                foreach (var item in list)
                {
                    var info = new ClassInfo()
                    {
                        className = FileHelper.ReplaceChar(item.tabname.ToString()),
                        classComment = item.tcomment
                    };
                    yield return info;
                }
            }
        }

        public IEnumerable<FieldInfo> GetTableFieldList(string dbName, ClassInfo classinfo)
        {
            using var con = GetConnection(dbName);
            if (con is not null)
            {
                var list = con.Query<(string COLUMN_NAME, string IS_NULLABLE, string DATA_TYPE, string COLUMN_COMMENT, string COLUMN_KEY)>(tableInfoSql, new { tablename = classinfo.className, dbname = dbName }).ToList();
                foreach (var item in list)
                {
                    var info = new FieldInfo()
                    {
                        filedName = item.COLUMN_NAME,
                        filedComment = item.COLUMN_COMMENT,
                        filedType = GetType(classinfo.className, item.DATA_TYPE.ToString()),
                        isNull = !item.IS_NULLABLE.Equals("YES"),
                        isKey = !string.IsNullOrWhiteSpace(item.COLUMN_KEY) && item.COLUMN_KEY.Equals("PRI")
                    };
                    yield return info;
                }
            }
        }

        /// <summary>
        /// 获取列的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetType(string name, string type)
        {
            if (type.Contains("varchar") || type.Contains("text") || type.Equals("json") 
                || type.Contains("char") || type.Contains("nchar") || type.Contains("ntext")
                || type.Contains("nvarchar") || type.Contains("enum"))
                return "string";
            else if (type.Equals("date") || type.Contains("time"))
                return "DateTime";
            else if (type.Equals("smallint") || type.Equals("bit"))
                return "short";
            else if (type.Equals("boolean") || type.Equals("bool"))
                return "bool";
            else if (type.Equals("smallint unsigned"))
                return "ushort";
            else if (type.Equals("bigint"))
                return "long";
            else if (type.Equals("bigint unsigned"))
                return "ulong";
            else if (type.Equals("int") || type.Equals("mediumint"))
                return "int";
            else if (type.Equals("int unsigned"))
                return "uint";
            else if (type.Equals("float"))
                return "float";
            else if (type.Equals("decimal") || type.Contains("money") || type.Equals("numeric"))
                return "decimal";
            else if (type.Equals("double"))
                return "double";
            else if (type.Equals("binary") || type.Equals("image") || type.Equals("varbinary") || type.Contains("blob"))
                return "byte[]";
            else if (type.Equals("real"))
                return "Single";
            else if (type.Equals("tinyint"))
                return "byte";
            else if (type.Equals("tinyint unsigned"))
                return "sbyte";
            else if (type.Equals("Variant"))
                return "object";
            else if (type.Equals("guid"))
                return "Guid";
            else return "string";
        }
    }
}
