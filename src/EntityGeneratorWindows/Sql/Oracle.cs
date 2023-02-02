using EntityGeneratorWindows.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityGeneratorWindows.Sql
{
    internal class Oracle : ISql
    {
        public IDbConnection GetConnection(string dbName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DataBaseInfo> GetDBList()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FieldInfo> GetTableFieldList(string dbName, ClassInfo classinfo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ClassInfo> GetTableList(string dbName)
        {
            throw new NotImplementedException();
        }
    }
}
