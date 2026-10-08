using AcademiaDoZe.Application.Enums;//Giovane Melo
using AcademiaDoZe.Infrastructure.Data;
namespace AcademiaDoZe.Application.Mappings;

public static class DatabaseTypeMappingExtensions
{
    public static DatabaseType ToInfrastructure(this AppDatabaseType appDatabaseType)
    {
        return appDatabaseType switch
        {
            AppDatabaseType.SqlServer => DatabaseType.SqlServer,
            AppDatabaseType.MySql => DatabaseType.MySql,
            AppDatabaseType.Sqlite => DatabaseType.Sqlite,
            _ => throw new ArgumentOutOfRangeException(nameof(appDatabaseType), appDatabaseType, "Tipo de banco de dados não suportado.")
        };
    }
    public static AppDatabaseType ToApplication(this DatabaseType databaseType)
    {
        return databaseType switch
        {
            DatabaseType.SqlServer => AppDatabaseType.SqlServer,
            DatabaseType.MySql => AppDatabaseType.MySql,
            DatabaseType.Sqlite => AppDatabaseType.Sqlite,
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, "Tipo de banco de dados não suportado.")
        };
    }
}
