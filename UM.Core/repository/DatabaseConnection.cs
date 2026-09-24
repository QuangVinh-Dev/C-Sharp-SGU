using LinqToDB;
using LinqToDB.Data;

namespace UM.Core.Repository;

public class DatabaseConnection : DataConnection
{
    public DatabaseConnection(IConfiguration configuration)
        : base(
            new DataOptions()
                .UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")!))
    {
    }
}