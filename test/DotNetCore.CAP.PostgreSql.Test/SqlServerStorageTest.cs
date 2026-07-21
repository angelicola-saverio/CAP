using Dapper;

namespace DotNetCore.CAP.PostgreSql.Test
{
    [Collection("PostgreSql")]
    public class SqlServerStorageTest : DatabaseTestHost
    {
        private readonly string _dbName;
        private readonly string _masterDbConnectionString;

        public SqlServerStorageTest()
        {
            _dbName = ConnectionUtil.GetDatabaseName();
            _masterDbConnectionString = ConnectionUtil.GetMasterConnectionString();
        }

        [Fact]
        public void Database_IsExists()
        {
            using (var connection = ConnectionUtil.CreateConnection(_masterDbConnectionString))
            {
                var databaseName = ConnectionUtil.GetDatabaseName();
                var sql = $"SELECT datname FROM pg_database WHERE datname = '{databaseName}'";
                var result = connection.QueryFirstOrDefault<string>(sql);
                Assert.NotNull(result);
                Assert.True(databaseName.Equals(result, System.StringComparison.CurrentCultureIgnoreCase));
            }
        }

        [Theory]
        [InlineData("published")]
        [InlineData("received")]
        public void DatabaseTable_IsExists(string tableName)
        {
            using (var connection = ConnectionUtil.CreateConnection())
            {
                var sql = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='cap' AND TABLE_NAME = '{tableName}'";
                var result = connection.QueryFirstOrDefault<string>(sql);
                Assert.NotNull(result);
                Assert.Equal(tableName, result);
            }
        }
    }
}