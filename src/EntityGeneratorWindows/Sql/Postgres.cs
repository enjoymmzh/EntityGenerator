using Dapper;
using EntityGeneratorWindows.Common;
using EntityGeneratorWindows.Model;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EntityGeneratorWindows.Sql
{
    /// <summary>
    /// Postgres数据库
    /// </summary>
    internal class Postgres : ISql
    {
        /// <summary>
        /// 所有数据库
        /// </summary>
        private string allDataBase = "select pg_database.datname, pg_database_size(pg_database.datname) AS size from pg_database";

        /// <summary>
        /// 获取所有的表名
        /// </summary>
        private string allTableSql = @"SELECT         
                                                pg_catalog.pg_relation_filenode(c.oid) as ""Filenode"",
                                                relname as tabname,
		                                        CAST(obj_description (relfilenode, 'pg_class' ) AS VARCHAR ) AS comment
                                            FROM
                                                pg_class c
                                                LEFT JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
                                                LEFT JOIN pg_catalog.pg_database d ON d.datname = 'postgres',
                                                pg_catalog.pg_tablespace t
                                            WHERE
                                                relkind IN ('r')
                                                AND n.nspname NOT IN ('pg_catalog', 'information_schema')
                                                AND n.nspname !~ '^pg_toast'
                                                AND t.oid = CASE WHEN reltablespace<> 0 THEN reltablespace   ELSE dattablespace   END
                                            ORDER BY
                                                 relname ";

        /// <summary>
        /// 获取所有的表信息
        /// </summary>
        private string tableInfoSql = @"SELECT
	                                        col_description ( A.attrelid, A.attnum ) AS COMMENT,
	                                        format_type ( A.atttypid, A.atttypmod ) AS TYPE,
	                                        A.attname AS NAME,
	                                        A.attnotnull AS NOTNULL 
                                        FROM
	                                        pg_class AS C,
	                                        pg_attribute AS A 
                                        WHERE
	                                        C.relname = @tablename 
	                                        AND A.attrelid = C.oid 
	                                        AND A.attnum > 0 
	                                        AND format_type ( A.atttypid, A.atttypmod ) != '-' ";

        private string tablekey = @"SELECT
	                                    pg_constraint.conname AS pk_name,
	                                    pg_attribute.attname AS colname,
	                                    pg_type.typname AS typename 
                                    FROM
	                                    pg_constraint
	                                    INNER JOIN pg_class ON pg_constraint.conrelid = pg_class.oid
	                                    INNER JOIN pg_attribute ON pg_attribute.attrelid = pg_class.oid 
	                                    AND pg_attribute.attnum = pg_constraint.conkey [ 1 ]
	                                    INNER JOIN pg_type ON pg_type.oid = pg_attribute.atttypid 
                                    WHERE
	                                    pg_class.relname = @tablename 
	                                    AND pg_constraint.contype = 'p' 
	                                    AND pg_table_is_visible ( pg_class.oid )";

        public IDbConnection GetConnection(string dbName)
        {
            try
            {
                var str = "PORT=" + Global.port + ";HOST=" + Global.ip + ";PASSWORD=" + Global.password + ";USER ID=" + Global.user + ";Pooling = false;";
                if (!string.IsNullOrWhiteSpace(dbName))
                {
                    str += "DATABASE=" + dbName + ";";
                }
                var con = new NpgsqlConnection(str);
                con.Open();
                return con;
            }
            catch (Exception ex)
            {
                TheEvent.ShowMessageBox("無法連接到數據庫", "糟糕，發生了一個錯誤");
                return null;
            }
        }

        /// <summary>
        /// 获取数据库列表
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataBaseInfo> GetDBList()
        {
            using var con = GetConnection("");
            if (con is not null)
            {
                var list = con.Query<(string datname, string size)> (allDataBase).ToList();
                foreach (var item in list)
                {
                    if (!item.datname.ToString().Contains("template"))
                    {
                        var info = new DataBaseInfo()
                        {
                            datname = item.datname,
                            size = item.size
                        };
                        yield return info;
                    }
                }
            }
        }

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ClassInfo> GetTableList(string dbName)
        {
            using var con = GetConnection(dbName);
            if (con is not null)
            {
                var list = con.Query<(string tabname, string comment)> (allTableSql).ToList();
                foreach (var item in list)
                {
                    var info = new ClassInfo()
                    {
                        className = FileHelper.ReplaceChar(item.tabname),
                        classComment = item.comment
                    };
                    yield return info;
                }
            }
        }

        /// <summary>
        /// 获取表中所有字段信息
        /// </summary>
        /// <param name="tableName"></param>
        public IEnumerable<FieldInfo> GetTableFieldList(string dbName, ClassInfo classinfo)
        {
            using var con = GetConnection(dbName);
            if (con is not null)
            {
                var list = con.Query<(string comment, string type, string name, bool notnull)> (tableInfoSql, new { tablename = classinfo.className }).ToList();
                var key = con.Query<(string pk_name, string colname, string typename)>(tablekey, new { tablename = classinfo.className }).FirstOrDefault();
                foreach (var item in list)
                {
                    var info = new FieldInfo()
                    {
                        filedName = item.name,
                        filedComment = item.comment,
                        filedType = GetType(classinfo.className, item.name, item.type),
                        isNull = item.notnull,
                        isKey = item.name.Equals(key.colname)
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
        private string GetType(string name, string namee, string type)
        {
            if (type.Contains("character varying") || type.Equals("text") || type.Equals("json"))
                return "string";
            else if (type.Equals("date") || type.Contains("timestamp"))
                return "DateTime";
            else if (type.Equals("smallint") || type.Contains("character"))
                return "short";
            else if (type.Equals("bigint"))
                return "long";
            else if (type.Equals("integer"))
                return "int";
            else if (type.Equals("bool"))
                return "bool";
            else if (type.Equals("bytea"))
                return "byte[]";
            else if (type.Equals("real"))
                return "float";
            else if (type.Contains("double"))
                return "double";
            else throw new Exception("无此类型");
        }
    }
}
