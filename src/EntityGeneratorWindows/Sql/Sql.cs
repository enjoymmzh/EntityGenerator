using System;
using Unity;

namespace EntityGeneratorWindows.Sql
{
    internal class SqlType
    {
        /// <summary>
        /// 
        /// </summary>
        private static UnityContainer container = null;

        /// <summary>
        /// 
        /// </summary>
        private static SqlType instance = null;
        public static SqlType Instance()
        {
            if (instance == null)
            {
                instance = new SqlType();

                //注冊控制反轉
                container = new UnityContainer();
                container.RegisterType<ISql, Mysql>("mysql");
                container.RegisterType<ISql, Postgres>("postgres");
                container.RegisterType<ISql, Sqlserver>("sqlserver");
                container.RegisterType<ISql, Sqlite>("sqllite");
                container.RegisterType<ISql, Oracle>("oracle");
                container.RegisterType<ISql, MongoDB>("mongodb");
            }
            return instance;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">类名</param>
        /// <returns></returns>
        public ISql Get(string type)
        {
            try
            {
                //獲取反轉類
                return container.Resolve<ISql>(type);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
