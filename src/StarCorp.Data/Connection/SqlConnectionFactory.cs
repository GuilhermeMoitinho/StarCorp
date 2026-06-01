using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;

namespace StarCorp.Data.Connection;

public sealed class SqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    static SqlConnectionFactory() => SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

    public DbConnection Create() => new SqlConnection(connectionString);
}
