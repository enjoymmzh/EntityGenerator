using EntityGeneratorWindows.Common;
using EntityGeneratorWindows.Model;
using EntityGeneratorWindows.Sql;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace EntityGeneratorWindows.Generator
{
    public class DBGenerate
    {
        #region 公共变量

        /// <summary>
        /// 当前项目路径
        /// </summary>
        private string curProjectPath { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        /// <summary>
        /// 类首字母大写
        /// </summary>
        public static bool ClasstoUpper = false;

        /// <summary>
        /// 字段首字母大写
        /// </summary>
        public static bool FieldtoUpper = false;

        #endregion

        private DBGenerate() { }
        private static DBGenerate instance = null;
        public static DBGenerate Get()
        {
            if (instance is null)
            {
                instance = new DBGenerate();
            }
            return instance;
        }

        /// <summary>
        /// 创建sln
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectName"></param>
        public void CreateSln(string modelId, string dataId, string projName, string path)
        {
            var projId = Guid.NewGuid().ToString();
            var templatePath = curProjectPath + "Template\\sln.txt";

            string project = string.Format("Project(\"{0}\") = \"{1}\", \"{1}\\{1}.csproj\", \"{2}\"\r\nEndProject\r\n",
                    "{" + projId.ToUpper() + "}", Global.modelName, "{" + modelId.ToUpper() + "}");
            string postSolution = string.Format("{0}.Debug|Any CPU.ActiveCfg = Debug|Any CPU\r\n", "{" + modelId.ToUpper() + "}");
            postSolution += string.Format("{0}.Debug|Any CPU.Build.0 = Debug|Any CPU\r\n", "{" + modelId.ToUpper() + "}");
            postSolution += string.Format("{0}.Release|Any CPU.ActiveCfg = Release|Any CPU\r\n", "{" + modelId.ToUpper() + "}");
            postSolution += string.Format("{0}.Release|Any CPU.Build.0 = Release|Any CPU\r\n", "{" + modelId.ToUpper() + "}");
            if (!string.IsNullOrWhiteSpace(dataId))
            {
                project += string.Format("Project(\"{0}\") = \"{1}\", \"{1}\\{1}.csproj\", \"{2}\"\r\nEndProject\r\n",
                    "{" + projId.ToUpper() + "}", Global.dataName, "{" + dataId.ToUpper() + "}");
                postSolution += string.Format("{0}.Debug|Any CPU.ActiveCfg = Debug|Any CPU\r\n", "{" + dataId.ToUpper() + "}");
                postSolution += string.Format("{0}.Debug|Any CPU.Build.0 = Debug|Any CPU\r\n", "{" + dataId.ToUpper() + "}");
                postSolution += string.Format("{0}.Release|Any CPU.ActiveCfg = Release|Any CPU\r\n", "{" + dataId.ToUpper() + "}");
                postSolution += string.Format("{0}.Release|Any CPU.Build.0 = Release|Any CPU\r\n", "{" + dataId.ToUpper() + "}");
            }
            string appendText = System.IO.File.ReadAllText(templatePath)
                .Replace("@project", project)
                .Replace("@postSolution", postSolution)
                .Replace("@guid", "{" + Guid.NewGuid().ToString().ToUpper() + "}");

            FileHelper.CreateFile(path + "\\" + projName + ".sln", appendText, System.Text.Encoding.UTF8);
        }


        #region 创建数据层项目

        /// <summary>
        /// 创建DbHelper
        /// </summary>
        /// <param name="className"></param>
        /// <param name="classComment"></param>
        /// <param name="dataList"></param>
        public void CreateDbHelper(string basePath)
        {
            #region 内容

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"using " + GetRef() + ";\r\n");
            stringBuilder.Append($"using System;\r\n");
            stringBuilder.Append($"using System.Data;\r\n \r\n");
            stringBuilder.Append($"namespace {Global.dataName}.Common\r\n");
            stringBuilder.Append($"{{\r\n");
            stringBuilder.Append($"    public class Db \r\n    {{\r\n");
            stringBuilder.Append($"        public static IDbConnection GetConnection()\r\n        {{\r\n");
            stringBuilder.Append($"            try \r\n");
            stringBuilder.Append($"            {{\r\n");
            stringBuilder.Append($"                var connectionString = \"" + GetStr() + "\";\r\n");
            stringBuilder.Append($"                var con = new " + GetInterface() + "(connectionString);\r\n");
            stringBuilder.Append($"                con.Open();\r\n");
            stringBuilder.Append($"                return con;\r\n");
            stringBuilder.Append($"            }}\r\n");
            stringBuilder.Append($"            catch (Exception ex)\r\n");
            stringBuilder.Append($"            {{\r\n");
            stringBuilder.Append($"                throw ex;\r\n");
            stringBuilder.Append($"            }}\r\n");
            stringBuilder.Append($"        }}\r\n");
            stringBuilder.Append($"    }} \r\n");
            stringBuilder.Append($"}}\r\n");
            #endregion

            CreateFile(basePath + "\\Common\\Db.cs", stringBuilder);
        }

        /// <summary>
        /// 创建基类dal
        /// </summary>
        /// <param name="className"></param>
        /// <param name="classComment"></param>
        /// <param name="dataList"></param>
        public void CreateBaseDal(string basePath)
        {
            #region 内容

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"using System;\r\n");
            stringBuilder.Append($"using System.Threading.Tasks;\r\n");
            stringBuilder.Append($"using System.Linq;\r\n");
            stringBuilder.Append($"using System.Collections.Generic;\r\n");
            stringBuilder.Append($"using Dapper.Contrib.Extensions;\r\n\r\n");
            stringBuilder.Append($"namespace {Global.dataName}.Common\r\n");
            stringBuilder.Append($"{{\r\n");
            stringBuilder.Append(@"    /// <summary>
    /// 基类
    /// </summary>
    /// <typeparam name=""T""></typeparam>
    public class BaseDal<T> where T : class
        {
            /// <summary>
            /// 增加
            /// </summary>
            /// <param name=""model""></param>
            /// <returns></returns>
            public long Insert(T model)
            {
                try
                {
                    using (var con = Db.GetConnection())
                    {
                        var res = con.Insert(model);
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }

            /// <summary>
            /// 增加
            /// 异步
            /// </summary>
            /// <param name=""model""></param>
            /// <returns></returns>
            public async Task<long> InsertAsync(T model)
            {
                try
                {
                    using (var con = Db.GetConnection())
                    {
                        var res = await con.InsertAsync(model);
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }

            /// <summary>
            /// 更新
            /// </summary>
            /// <param name=""model""></param>
            /// <returns></returns>
            public bool Update(T model)
            {
                try
                {
                    using (var con = Db.GetConnection())
                    {
                        return con.Update(model);
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            /// <summary>
            /// 更新
            /// 异步
            /// </summary>
            /// <param name=""model""></param>
            /// <returns></returns>
            public async Task<bool> UpdateAsync(T model)
            {
                try
                {
                    using (var con = Db.GetConnection())
                    {
                        return await con.UpdateAsync(model);
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            /// <summary>
            /// 删除
            /// </summary>
            /// <param name=""model""></param>
            /// <returns></returns>
            public bool Delete(T model)
            {
                try
                {
                    using (var con = Db.GetConnection())
                    {
                        return con.Delete(model);
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            /// <summary>
            /// 删除
            /// 异步
            /// </summary>
            /// <param name=""model""></param>
            /// <returns></returns>
            public async Task<bool> DeleteAsync(T model)
            {
                try
                {
                    using (var con = Db.GetConnection())
                    {
                        return await con.DeleteAsync(model);
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            /// <summary>
            /// 从id获取数据
            /// </summary>
            /// <param name=""model""></param>
            /// <returns></returns>
            public T GetById(long id)
            {
                try
                {
                    using (var con = Db.GetConnection())
                    {
                        var res = con.Get<T>(id);
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    return default(T);
                }
            }

            /// <summary>
            /// 从id获取数据
            /// 异步
            /// </summary>
            /// <param name=""model""></param>
            /// <returns></returns>
            public async Task<T> GetByIdAsync(long id)
            {
                try
                {
                    using (var con = Db.GetConnection())
                    {
                        var res = await con.GetAsync<T>(id);
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    return default(T);
                }
            }

            /// <summary>
            /// 获取所有数据
            /// </summary>
            /// <param name=""model""></param>
            /// <returns></returns>
            public List<T> GetAll()
            {
                try
                {
                    using (var con = Db.GetConnection())
                    {
                        return con.GetAll<T>().ToList();
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            /// <summary>
            /// 获取所有数据
            /// 异步
            /// </summary>
            /// <param name=""model""></param>
            /// <returns></returns>
            public async Task<List<T>> GetAllAsync()
            {
                try
                {
                    using (var con = Db.GetConnection())
                    {
                        var res = await con.GetAllAsync<T>();
                        return res.ToList();
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }");
            stringBuilder.Append($"}}\r\n");

            #endregion

            CreateFile(basePath + "\\Common\\BaseDal.cs", stringBuilder);
        }

        /// <summary>
        /// 创建数据类
        /// </summary>
        /// <param name="sqlType"></param>
        /// <param name="basePath"></param>
        public void CreateModelDal(string basePath)
        {
            if (Global.tables.Count() > 0)
            { 
                var table = Global.tables.FirstOrDefault();
                var con = SqlType.Instance().Get(Global.sqlType);
                var fields = con.GetTableFieldList(Global.tableName, table);
                if (fields.Count() > 0)
                {
                    #region 内容

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"using Dapper;\r\n");
                    stringBuilder.Append($"using System;\r\n");
                    stringBuilder.Append($"using System.Linq;\r\n");
                    stringBuilder.Append($"using {Global.modelName};\r\n");
                    stringBuilder.Append($"using {Global.modelName}.Common;\r\n");
                    stringBuilder.Append($"using {Global.dataName}.Common;\r\n \r\n");
                    stringBuilder.Append($"namespace {Global.dataName}\r\n" + "{" + "\r\n");
                    stringBuilder.Append($"    /// <summary>\r\n");
                    stringBuilder.Append($"    /// {table.classComment} \r\n");
                    stringBuilder.Append($"    /// </summary>\r\n");
                    stringBuilder.Append($"    public class {ClassToTitleCase(table.className) + Global.dataName.Replace(Global.projName, "")} : BaseDal<{ClassToTitleCase(table.className)}> \r\n    {{\r\n");
                    stringBuilder.Append($"        /// <summary>\r\n");
                    stringBuilder.Append($"        /// 分页\r\n");
                    stringBuilder.Append($"        /// <param name=\"query\"></param>\r\n");
                    stringBuilder.Append($"        /// </summary>\r\n");
                    stringBuilder.Append($"        /// <returns></returns>\r\n");
                    stringBuilder.Append($"        public Page<{ClassToTitleCase(table.className)}> GetPageList(Query query)\r\n");
                    stringBuilder.Append($"        {{\r\n");
                    stringBuilder.Append($"            var sql = @\"SELECT * FROM {table.className} WHERE 1 = 1 \";\r\n");
                    stringBuilder.Append($"            var count = @\"SELECT COUNT(0) FROM {table.className} WHERE 1 = 1 \";\r\n\r\n");
                    stringBuilder.Append($"            var dp = new DynamicParameters();\r\n");
                    stringBuilder.Append($"            sql += \" Limit @PageIndex, @PageSize; \";\r\n");
                    stringBuilder.Append($"            dp.Add(\"PageIndex\", (query.{FieldToTitleCase("page")} - 1) * query.{FieldToTitleCase("size")});\r\n");
                    stringBuilder.Append($"            dp.Add(\"PageSize\", query.{FieldToTitleCase("size")});\r\n\r\n");
                    stringBuilder.Append($"            using (var con = Db.GetConnection())\r\n");
                    stringBuilder.Append($"            {{\r\n");
                    stringBuilder.Append($"                try\r\n");
                    stringBuilder.Append($"                {{\r\n");
                    stringBuilder.Append($"                    var result = new Page<{ClassToTitleCase(table.className)}>();\r\n");
                    stringBuilder.Append($"                    result.{FieldToTitleCase("items")} = con.Query<{ClassToTitleCase(table.className)}>(sql, dp).ToList();\r\n");
                    stringBuilder.Append($"                    result.{FieldToTitleCase("total")} = con.ExecuteScalar<int>(count, dp);\r\n");
                    stringBuilder.Append($"                    return result;\r\n");
                    stringBuilder.Append($"                }}\r\n");
                    stringBuilder.Append($"                catch (Exception ex)\r\n");
                    stringBuilder.Append($"                {{\r\n");
                    stringBuilder.Append($"                    return new Page<{ClassToTitleCase(table.className)}>();\r\n");
                    stringBuilder.Append($"                }}\r\n");
                    stringBuilder.Append($"            }}\r\n");
                    stringBuilder.Append($"        }}\r\n");
                    stringBuilder.Append($"    }} \r\n");
                    stringBuilder.Append($"}}\r\n");
                    #endregion

                    CreateFile(basePath + "\\" + ClassToTitleCase(table.className) + Global.dataName.Replace(Global.projName, "") + ".cs", stringBuilder);
                }
            }
        }

        /// <summary>
        /// 创建Csproj
        /// </summary>
        /// <param name="className"></param>
        /// <param name="classComment"></param>
        /// <param name="dataList"></param>
        public string CreateDalCsproj(string basePath)
        {
            #region 内容

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"<Project Sdk=\"Microsoft.NET.Sdk\">\r\n");
            stringBuilder.Append($"  <PropertyGroup>\r\n");
            if (Global.pversion.Contains("standard"))
            {
                stringBuilder.Append($"	  <TargetFrameworks>{Global.pversion}</TargetFrameworks>\r\n");
            }
            else
            {
                stringBuilder.Append($"	  <TargetFramework>{Global.pversion}</TargetFramework>\r\n");
            }
            stringBuilder.Append($"  </PropertyGroup>\r\n");
            stringBuilder.Append($"  <ItemGroup>\r\n");
            stringBuilder.Append($"    <PackageReference Include=\"Dapper\" Version=\"2.0.90\" />\r\n");
            stringBuilder.Append($"    <PackageReference Include=\"Dapper.Contrib\" Version=\"2.0.78\" />\r\n");
            stringBuilder.Append($"    " + GetPackageReference());
            stringBuilder.Append($"  </ItemGroup>\r\n");
            stringBuilder.Append($"  <ItemGroup>\r\n");
            stringBuilder.Append($"    <ProjectReference Include=\"..\\{Global.modelName}\\{Global.modelName}.csproj\" />\r\n");
            stringBuilder.Append($"  </ItemGroup>\r\n");
            stringBuilder.Append($"</Project>\r\n");
            #endregion

            CreateFile(basePath + "\\" + Global.dataName + ".csproj", stringBuilder);

            return Guid.NewGuid().ToString();
        }

        #endregion

        #region 创建实体层项目

        /// <summary>
        /// 创建Page
        /// </summary>
        /// <param name="className"></param>
        /// <param name="classComment"></param>
        /// <param name="dataList"></param>
        public void CreatePage(string basePath)
        {
            #region 内容

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"using System;\r\n");
            stringBuilder.Append($"using System.Collections.Generic;\r\n \r\n");
            stringBuilder.Append($"namespace {Global.modelName}.Common\r\n");
            stringBuilder.Append($"{{\r\n");
            stringBuilder.Append($"    /// <summary>\r\n");
            stringBuilder.Append($"    /// 分页\r\n");
            stringBuilder.Append($"    /// </summary>\r\n");
            stringBuilder.Append($"    /// <typeparam name=\"T\"></typeparam>\r\n");
            stringBuilder.Append($"    [Serializable]\r\n");
            stringBuilder.Append($"    public class Page<T> \r\n    {{\r\n");
            stringBuilder.Append($"        /// <summary>\r\n");
            stringBuilder.Append($"        /// 总数\r\n");
            stringBuilder.Append($"        /// </summary>\r\n");
            stringBuilder.Append($"        public int {FieldToTitleCase("total")} {{ get; set; }}\r\n");
            stringBuilder.Append($"        /// <summary>\r\n");
            stringBuilder.Append($"        /// 数据列表\r\n");
            stringBuilder.Append($"        /// </summary>\r\n");
            stringBuilder.Append($"        public List<T> {FieldToTitleCase("items")} {{ get; set; }}\r\n");
            stringBuilder.Append($"    }} \r\n");
            stringBuilder.Append($"}}\r\n");
            #endregion

            CreateFile(basePath + "\\Common\\Page.cs", stringBuilder);
        }

        /// <summary>
        /// 创建Query
        /// </summary>
        /// <param name="className"></param>
        /// <param name="classComment"></param>
        /// <param name="dataList"></param>
        public void CreateQuery(string basePath)
        {
            #region 内容

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"using System;\r\n\r\n");
            stringBuilder.Append($"namespace {Global.modelName}.Common\r\n");
            stringBuilder.Append($"{{\r\n");
            stringBuilder.Append($"    public class Query \r\n    {{\r\n");
            stringBuilder.Append($"        /// <summary>\r\n");
            stringBuilder.Append($"        /// 页数\r\n");
            stringBuilder.Append($"        /// </summary>\r\n");
            stringBuilder.Append($"        public int {FieldToTitleCase("page")} {{ get; set; }}\r\n");
            stringBuilder.Append($"        /// <summary>\r\n");
            stringBuilder.Append($"        /// 每页数据量\r\n");
            stringBuilder.Append($"        /// </summary>\r\n");
            stringBuilder.Append($"        public int {FieldToTitleCase("size")} {{ get; set; }}\r\n");
            stringBuilder.Append($"        /// <summary>\r\n");
            stringBuilder.Append($"        /// 开始时间\r\n");
            stringBuilder.Append($"        /// </summary>\r\n");
            stringBuilder.Append($"        public DateTime? {FieldToTitleCase("startTime")} {{ get; set; }}\r\n");
            stringBuilder.Append($"        /// <summary>\r\n");
            stringBuilder.Append($"        /// 结束时间\r\n");
            stringBuilder.Append($"        /// </summary>\r\n");
            stringBuilder.Append($"        public DateTime? {FieldToTitleCase("endTime")} {{ get; set; }}\r\n");
            stringBuilder.Append($"    }} \r\n");
            stringBuilder.Append($"}}\r\n");
            #endregion

            CreateFile(basePath + "\\Common\\Query.cs", stringBuilder);
        }

        /// <summary>
        /// 创建Result
        /// </summary>
        /// <param name="className"></param>
        /// <param name="classComment"></param>
        /// <param name="dataList"></param>
        public void CreateResult(string basePath)
        {
            #region 内容

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"using System;\r\n");
            stringBuilder.Append($"using System.Net;\r\n\r\n");
            stringBuilder.Append($"namespace {Global.modelName}.Common\r\n");
            stringBuilder.Append($"{{\r\n");
            stringBuilder.Append($"    /// <summary>\r\n");
            stringBuilder.Append($"    /// 返回值实体，可以根据实际情况修改，这里是范例\r\n");
            stringBuilder.Append($"    /// </summary>\r\n");
            stringBuilder.Append($"    [Serializable]\r\n");
            stringBuilder.Append($"    public class Result \r\n    {{\r\n");
            stringBuilder.Append($"        /// <summary>\r\n");
            stringBuilder.Append($"        /// 返回代码\r\n");
            stringBuilder.Append($"        /// </summary>\r\n");
            stringBuilder.Append($"        public HttpStatusCode {FieldToTitleCase("code")} {{ get; set; }}\r\n");
            stringBuilder.Append($"        /// <summary>\r\n");
            stringBuilder.Append($"        /// 信息\r\n");
            stringBuilder.Append($"        /// </summary>\r\n");
            stringBuilder.Append($"        public string {FieldToTitleCase("msg")} {{ get; set; }}\r\n");
            stringBuilder.Append($"        /// <summary>\r\n");
            stringBuilder.Append($"        /// 数据\r\n");
            stringBuilder.Append($"        /// </summary>\r\n");
            stringBuilder.Append($"        public object {FieldToTitleCase("data")} {{ get; set; }}\r\n");
            stringBuilder.Append($"        /// <summary>\r\n");
            stringBuilder.Append($"        /// 是否成功\r\n");
            stringBuilder.Append($"        /// </summary>\r\n");
            stringBuilder.Append($"        public bool {FieldToTitleCase("isSuccess")} => {FieldToTitleCase("code")} is HttpStatusCode.OK || {FieldToTitleCase("code")} is HttpStatusCode.Created;\r\n");
            stringBuilder.Append($"    }} \r\n");
            stringBuilder.Append($"}}\r\n");
            #endregion

            CreateFile(basePath + "\\Common\\Result.cs", stringBuilder);
        }

        /// <summary>
        /// 创建实体
        /// </summary>
        /// <param name="className"></param>
        /// <param name="classComment"></param>
        /// <param name="dataList"></param>
        public void CreateModel(string basePath)
        {
            var con = SqlType.Instance().Get(Global.sqlType);
            for (int index = 0; index < Global.tables.Count; index++)
            {
                var table = Global.tables[index];
                var fields = con.GetTableFieldList(Global.tableName, table);
                if (fields.Count() > 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"using System;\r\n");
                    stringBuilder.Append($"using System.Collections.Generic;\r\n");
                    stringBuilder.Append($"using System.Linq;\r\n");
                    stringBuilder.Append($"using System.Text;\r\n");
                    stringBuilder.Append($"using System.Threading.Tasks;\r\n");
                    stringBuilder.Append($"using Dapper.Contrib.Extensions;\r\n");
                    stringBuilder.Append($"using TableAttribute = Dapper.Contrib.Extensions.TableAttribute;\r\n");
                    stringBuilder.Append($"using KeyAttribute = Dapper.Contrib.Extensions.KeyAttribute;\r\n \r\n");
                    stringBuilder.Append($"namespace {Global.modelName}\r\n");
                    stringBuilder.Append($"{{\r\n");
                    stringBuilder.Append($"    /// <summary>\r\n");
                    stringBuilder.Append($"    /// {table.classComment} \r\n");
                    stringBuilder.Append($"    /// 外链字段使用[Computed]标记\r\n");
                    stringBuilder.Append($"    /// </summary>\r\n");
                    var classname = ClassToTitleCase(table.className);
                    stringBuilder.Append($"    [Table(\"{table.className}\")] \r\n");
                    stringBuilder.Append($"    public class {classname} \r\n    {{\r\n");

                    foreach (var field in fields)
                    {
                        stringBuilder.Append($"        /// <summary>\r\n");
                        if (field.isNull)
                        {
                            stringBuilder.Append($"        /// 必填，不能为空\r\n");
                        }
                        stringBuilder.Append($"        /// {field.filedComment}\r\n");
                        stringBuilder.Append($"        /// </summary>\r\n");
                        if (field.isKey)
                        {
                            stringBuilder.Append($"        [Key]\r\n");
                        }
                        stringBuilder.Append($"        public {field.filedType} {FieldToTitleCase(field.filedName)} {{ get; set; }}\r\n");
                    }
                    stringBuilder.Append("    } \r\n");
                    stringBuilder.Append($"}}\r\n");

                    CreateFile(basePath + "\\" + classname + ".cs", stringBuilder);

                    TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs($"表 {classname} 生成实体完成（{index + 1}/{Global.tables.Count})"));
                }
            }
        }

        /// <summary>
        /// 创建实体预览
        /// </summary>
        /// <param name="className"></param>
        /// <param name="classComment"></param>
        /// <param name="dataList"></param>
        public string CreatePreview(ClassInfo info)
        {
            var con = SqlType.Instance().Get(Global.sqlType);
            var fields = con.GetTableFieldList(Global.tableName, info);
            if (fields.Count() > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append($"/// <summary>\r\n");
                stringBuilder.Append($"/// {info.classComment} \r\n");
                stringBuilder.Append($"/// </summary>\r\n");
                var classname = ClassToTitleCase(info.className);
                stringBuilder.Append($"public class {classname} \r\n    {{\r\n");

                foreach (var field in fields)
                {
                    stringBuilder.Append($"    /// <summary>\r\n");
                    if (field.isNull)
                    {
                        stringBuilder.Append($"    /// 必填，不能为空\r\n");
                    }
                    stringBuilder.Append($"    /// {field.filedComment}\r\n");
                    stringBuilder.Append($"    /// </summary>\r\n");
                    stringBuilder.Append($"    public {field.filedType} {FieldToTitleCase(field.filedName)} {{ get; set; }}\r\n");
                }
                stringBuilder.Append("} \r\n");

                return stringBuilder.ToString();
            }
            return "未能生成实体预览";
        }

        /// <summary>
        /// 创建Csproj
        /// </summary>
        /// <param name="className"></param>
        /// <param name="classComment"></param>
        /// <param name="dataList"></param>
        public string CreateModelCsproj(string basePath)
        {
            #region 内容

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"<Project Sdk=\"Microsoft.NET.Sdk\">\r\n");
            stringBuilder.Append($"  <PropertyGroup>\r\n");
            if (Global.pversion.Contains("standard"))
            {
                stringBuilder.Append($"	  <TargetFrameworks>{Global.pversion}</TargetFrameworks>\r\n");
            }
            else
            {
                stringBuilder.Append($"	  <TargetFramework>{Global.pversion}</TargetFramework>\r\n");
            }
            stringBuilder.Append($"  </PropertyGroup>\r\n");
            stringBuilder.Append($"  <ItemGroup>\r\n");
            stringBuilder.Append($"    <PackageReference Include=\"Dapper.Contrib\" Version=\"2.0.78\" />\r\n");
            stringBuilder.Append($"  </ItemGroup>\r\n");
            stringBuilder.Append($"</Project>\r\n");
            #endregion

            CreateFile(basePath + "\\" + Global.modelName + ".csproj", stringBuilder);

            return Guid.NewGuid().ToString();
        }

        #endregion

        #region Util

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="info"></param>
        internal static void CreateFile(string path, StringBuilder info)
        {
            StreamWriter sr;
            //是否存在文件夹,不存在则创建
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            //如果该文件存在则追加内容，否则创建文件
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            sr = File.CreateText(path);
            sr.Write(info.ToString());
            sr.Flush();
            sr.Close();
        }

        /// <summary>
        /// 类首字母大写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string ClassToTitleCase(string str)
        {
            if (ClasstoUpper)
            {
                var sb = new StringBuilder(str.Length);
                for (int i = 0; i < str.Length; i++)
                {
                    var c = str[i];
                    if (i.Equals(0))
                    {
                        sb.Append(char.ToUpper(c));
                    }
                    else
                    {
                        var last = str[i - 1];//如果前一个字符非数字或字母，就大写
                        if (!char.IsLetterOrDigit(last))
                        {
                            sb.Append(char.ToUpper(c));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                }
                return sb.ToString();
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// 字段首字母大写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string FieldToTitleCase(string str)
        {
            if (FieldtoUpper)
            {
                var sb = new StringBuilder(str.Length);
                for (int i = 0; i < str.Length; i++)
                {
                    var c = str[i];
                    if (i.Equals(0))
                    {
                        sb.Append(char.ToUpper(c));
                    }
                    else
                    {
                        var last = str[i - 1];//如果前一个字符非数字或字母，就大写
                        if (!char.IsLetterOrDigit(last))
                        {
                            sb.Append(char.ToUpper(c));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                }
                return sb.ToString();
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// 获取数据库引用
        /// </summary>
        /// <returns></returns>
        private string GetRef() => Global.sqlType switch
        {
            "mysql" => "MySql.Data.MySqlClient",
            "postgres" => "Npgsql",
            "sqlserver" => "System.Data.SqlClient",
            _ => "System",
        };

        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <returns></returns>
        private string GetStr() => Global.sqlType switch
        {
            "mysql" => "server=" + Global.ip + ";port=" + Global.port + ";database=" + Global.tableName + ";user id=" + Global.user + ";password=" + Global.password + ";CharacterSet=utf8",
            "postgres" => "PORT=" + Global.port + ";HOST=" + Global.ip + ";PASSWORD=" + Global.password + ";USER ID=" + Global.user + ";DATABASE=" + Global.tableName + ";Pooling = false;",
            "sqlserver" => "Data Source=" + Global.ip + ";Initial Catalog=" + Global.tableName + ";User Id=" + Global.user + ";Password=" + Global.password + ";",
            _ => "",
        };

        /// <summary>
        /// 获取数据库接口
        /// </summary>
        /// <returns></returns>
        private string GetInterface() => Global.sqlType switch
        {
            "mysql" => "MySqlConnection",
            "postgres" => "NpgsqlConnection",
            "sqlserver" => "SqlConnection",
            _ => "",
        };

        /// <summary>
        /// 获取数据库包引用
        /// </summary>
        /// <returns></returns>
        private string GetPackageReference() => Global.sqlType switch
        {
            "mysql" => "<PackageReference Include=\"MySql.Data\" Version=\"8.0.26\" />\r\n",
            "postgres" => "<PackageReference Include=\"Npgsql\" Version=\"4.1.0\" />\r\n",
            "sqlserver" => "<PackageReference Include=\"System.Data.SqlClient\" Version=\"4.8.2\" />\r\n",
            _ => "",
        };

        #endregion
    }
}
