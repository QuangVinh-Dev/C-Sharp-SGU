using LinqToDB;
using LinqToDB.Data;

namespace UM.Core.config;

public class DatabaseConnection : DataConnection
{
    public DatabaseConnection(string connectionString)
        : base(
            new DataOptions()
                .UseConnectionString(
                    ProviderName.SqlServer2017,
                    connectionString))
    {
    }
}