using Npgsql;

namespace DotNetCore.CAP.PostgreSql.Test
{
    public static class ConnectionUtil
    {
        private const string ConnectionStringTemplateVariable = "Cap_SqlServer_ConnectionString";

        private const string MasterDatabaseName = "postgres";
        private const string DefaultDatabaseName = "cap_test";

        private const string DefaultConnectionString =
            @"Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=postgres;";

        public static string GetDatabaseName()
        {
            return DefaultDatabaseName;
        }

        public static string GetMasterConnectionString()
        {
            return GetConnectionString().Replace(DefaultDatabaseName, MasterDatabaseName);
        }

        public static string GetConnectionString()
        {
            return
                Environment.GetEnvironmentVariable(ConnectionStringTemplateVariable) ??
                DefaultConnectionString;
        }

        public static NpgsqlConnection CreateConnection(string connectionString = null)
        {
            connectionString ??= GetConnectionString();
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}