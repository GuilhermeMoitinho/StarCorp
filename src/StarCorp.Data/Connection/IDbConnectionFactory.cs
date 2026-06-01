using System.Data.Common;

namespace StarCorp.Data.Connection;

public interface IDbConnectionFactory
{
    DbConnection Create();
}
