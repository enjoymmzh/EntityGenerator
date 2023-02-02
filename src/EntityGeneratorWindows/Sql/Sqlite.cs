
using EntityGeneratorWindows.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace EntityGeneratorWindows.Sql
{
    internal class Sqlite : ISql
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
