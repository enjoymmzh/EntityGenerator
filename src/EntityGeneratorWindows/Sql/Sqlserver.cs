using Dapper;
using EntityGeneratorWindows.Common;
using EntityGeneratorWindows.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace EntityGeneratorWindows.Sql
{
    /// <summary>
    /// SqlServer数据库
    /// </summary>
    internal class Sqlserver : ISql
    {
        /// <summary>
        /// 所有数据库
        /// </summary>
        private string allDataBase = "SELECT NAME FROM MASTER.DBO.SYSDATABASES where NAME  !='master' and NAME !='model' and NAME  != 'msdb' and NAME  !='tempdb' ORDER BY NAME ; ";

        /// <summary>
        /// 获取所有的表名
        /// </summary>
        private string allTableSql = @"SELECT DISTINCT
	                                        d.name,
	                                        f.value 
                                        FROM
	                                        syscolumns a
	                                    LEFT JOIN systypes b ON a.xusertype= b.xusertype
	                                    INNER JOIN sysobjects d ON a.id= d.id 
	                                    AND d.xtype= 'U' 
	                                    AND d.name<> 'dtproperties'
	                                    LEFT JOIN syscomments e ON a.cdefault= e.id
	                                    LEFT JOIN sys.extended_properties g ON a.id= G.major_id 
	                                    AND a.colid= g.minor_id
	                                    LEFT JOIN sys.extended_properties f ON d.id= f.major_id 
	                                    AND f.minor_id= 0";

        /// <summary>
        /// 获取所有的表信息
        /// </summary>
        private string tableInfoSql = @"SELECT
	                                        a.name fieldname,
	                                        b.name fieldtype,
                                        CASE
		
		                                        WHEN a.isnullable= 1 THEN
		                                        'true' ELSE 'false' 
	                                        END isnullvalue,
	                                        isnull( g.[value], '' ) comment 
                                        FROM
	                                        syscolumns a
	                                        LEFT JOIN systypes b ON a.xtype= b.xusertype
	                                        INNER JOIN sysobjects d ON a.id= d.id 
	                                        AND d.xtype= 'U' 
	                                        AND d.name<> 'dtproperties'
	                                        LEFT JOIN syscomments e ON a.cdefault= e.id
	                                        LEFT JOIN sys.extended_properties g ON a.id= g.major_id 
	                                        AND a.colid= g.minor_id
	                                        LEFT JOIN sys.extended_properties f ON d.id= f.major_id 
	                                        AND f.minor_id = 0 
                                        WHERE
	                                        d.name= @tablename ";

        public IDbConnection GetConnection(string dbName)
        {
            try
            {
                var str = $"Server={Global.ip};Database=master;Initial Catalog={dbName};Trusted_Connection=True;"; 
                if (!string.IsNullOrWhiteSpace(Global.user) && !string.IsNullOrWhiteSpace(Global.password))
                {
                    str = $"Data Source={Global.ip};User Id={Global.user};Password={Global.password};Initial Catalog={dbName};";
                }
                
                var con = new SqlConnection(str);
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
            if (con is not null)
            {
                var list = con.Query<dynamic> (allDataBase).ToList();
                foreach (var item in list)
                {
                    var info = new DataBaseInfo()
                    {
                        datname = item.NAME,
                        size = ""
                    };
                    yield return info;
                }
            }
        }

        public IEnumerable<ClassInfo> GetTableList(string dbName)
        {
            using var con = GetConnection(dbName);
            if (con is not null)
            {
                var list = con.Query<(string name, string value)>(allTableSql).ToList();
                foreach (var item in list)
                {
                    var info = new ClassInfo()
                    {
                        className = FileHelper.ReplaceChar(item.name),
                        classComment = item.value
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
                var list = con.Query<(string fieldname, string fieldtype, bool isnullvalue, string comment)> (tableInfoSql, new { tablename = classinfo.className, dbname = dbName }).ToList();
                foreach (var item in list)
                {
                    var info = new FieldInfo()
                    {
                        filedName = item.fieldname,
                        filedComment = item.comment,
                        filedType = GetType(classinfo.className, item.fieldtype.ToString()),
                        isNull = !Convert.ToBoolean(item.isnullvalue),
                        isKey = false
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
                || type.Contains("nvarchar"))
                return "string";
            else if (type.Equals("date") || type.Contains("time"))
                return "DateTime";
            else if (type.Equals("smallint") || type.Equals("bit"))
                return "short";
            else if (type.Equals("bigint"))
                return "long";
            else if (type.Equals("int") || type.Equals("mediumint"))
                return "int";
            else if (type.Equals("float"))
                return "float";
            else if (type.Equals("decimal") || type.Equals("money") || type.Equals("numeric"))
                return "decimal";
            else if (type.Equals("double") || type.Equals("float"))
                return "double";
            else if (type.Equals("binary") || type.Equals("image") || type.Equals("varbinary"))
                return "byte[]";
            else if (type.Equals("real") || type.Equals("smallmoney"))
                return "Single";
            else if (type.Equals("tinyint"))
                return "byte";
            else if (type.Equals("sql_variant"))
                return "object";
            else throw new Exception("无此类型");
        }
    }
}
