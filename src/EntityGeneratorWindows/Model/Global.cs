using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EntityGeneratorWindows.Model
{
    internal class Global
    {
        public static string sqlType { get; set; }
        public static string ip { get; set; }
        public static string port { get; set; }
        public static string user { get; set; }
        public static string password { get; set; }
        public static string path { get; set; }
        /// <summary>
        /// 项目名称
        /// </summary>
        public static string projName { get; set; }
        /// <summary>
        /// 实体层项目名称
        /// </summary>
        public static string modelName { get; set; }
        /// <summary>
        /// 数据层项目名称
        /// </summary>
        public static string dataName { get; set; }
        /// <summary>
        /// 生成类型，0：只生成实体；1：生成实体项目；2：生成实体项目和数据层项目
        /// </summary>
        public static int btype { get; set; }

        public static string tableName { get; set; }
        public static List<ClassInfo> tables { get; set; } = new List<ClassInfo>();

        public static string pversion { get; set; }

        public static Window instance { get; set; }
    }

    internal class Sqlinfo
    {
        public string ip { get; set; }
        public string port { get; set; }
        public string sqlType { get; set; }
        public string user { get; set; }
        public string password { get; set; }
    }
}
