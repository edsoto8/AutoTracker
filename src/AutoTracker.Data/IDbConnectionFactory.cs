using System.Data;

namespace AutoTracker.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
