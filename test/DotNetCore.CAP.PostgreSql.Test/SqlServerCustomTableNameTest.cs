using Dapper;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCore.CAP.PostgreSql.Test
{
    [Collection("PostgreSql")]
    public class SqlServerCustomTableNameTest : IDisposable
    {
        private const string CustomSchema = "cap_custom";
        private const string CustomPublishedTableName = "CustomPublished";
        private const string CustomReceivedTableName = "CustomReceived";
        private const string CustomLockTableName = "CustomLock";

        private readonly ServiceProvider _provider;
        private readonly IStorageInitializer _initializer;

        public SqlServerCustomTableNameTest()
        {
            EnsureDatabaseExists();

            var services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging();
            services.AddOptions<CapOptions>().Configure(x => x.UseStorageLock = true);
            services.Configure<PostgreSqlOptions>(x =>
            {
                x.ConnectionString = ConnectionUtil.GetConnectionString();
                x.Schema = CustomSchema;
                x.PublishedTableName = CustomPublishedTableName;
                x.ReceivedTableName = CustomReceivedTableName;
                x.LockTableName = CustomLockTableName;
            });
            services.AddSingleton<PostgreSqlDataStorage>();
            services.AddSingleton<IStorageInitializer, PostgreSqlStorageInitializer>();
            services.AddSingleton<ISerializer, JsonUtf8Serializer>();
            services.AddSingleton<ISnowflakeId>(_ => new SnowflakeId(10));

            _provider = services.BuildServiceProvider();
            _initializer = _provider.GetRequiredService<IStorageInitializer>();
            _initializer.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        private static void EnsureDatabaseExists()
        {
            var databaseName = ConnectionUtil.GetDatabaseName();

            using var connection = ConnectionUtil.CreateConnection(ConnectionUtil.GetMasterConnectionString());

            var exists = connection.ExecuteScalar<bool>(
                "SELECT EXISTS (SELECT 1 FROM pg_database WHERE datname = @databaseName);",
                new { databaseName });

            if (!exists)
            {
                connection.Execute($@"CREATE DATABASE ""{databaseName}"";");
            }
        }

        [Fact]
        public void CustomTableNames_AreReturnedByStorageInitializer()
        {
            Assert.Equal($"\"{CustomSchema}\".\"{CustomPublishedTableName}\"", _initializer.GetPublishedTableName());
            Assert.Equal($"\"{CustomSchema}\".\"{CustomReceivedTableName}\"", _initializer.GetReceivedTableName());
            Assert.Equal($"\"{CustomSchema}\".\"{CustomLockTableName}\"", _initializer.GetLockTableName());
        }

        [Theory]
        [InlineData(CustomPublishedTableName)]
        [InlineData(CustomReceivedTableName)]
        [InlineData(CustomLockTableName)]
        public void CustomTable_IsCreatedInCustomSchema(string tableName)
        {
            using var connection = ConnectionUtil.CreateConnection();
            var sql = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA=@Schema AND TABLE_NAME=@TableName";
            var result = connection.QueryFirstOrDefault<string>(sql, new { Schema = CustomSchema, TableName = tableName });
            Assert.Equal(tableName, result);
        }

        [Fact]
        public void DefaultNamedTables_AreNotCreatedInCustomSchema()
        {
            using var connection = ConnectionUtil.CreateConnection();
            var sql = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA=@Schema AND TABLE_NAME=@TableName";
            var result = connection.QueryFirstOrDefault<string>(sql, new { Schema = CustomSchema, TableName = "Published" });
            Assert.Null(result);
        }

        [Fact]
        public async Task StoreMessageAsync_PersistsRowInCustomPublishedTable()
        {
            var storage = _provider.GetRequiredService<PostgreSqlDataStorage>();
            var snowflakeId = _provider.GetRequiredService<ISnowflakeId>();

            var msgId = snowflakeId.NextId().ToString();
            var header = new Dictionary<string, string> { [Headers.MessageId] = msgId };
            var message = new Message(header, null);

            var mdMessage = await storage.StoreMessageAsync("test.custom.name", message);

            using var connection = ConnectionUtil.CreateConnection();
            var sql = $@"SELECT COUNT(1) FROM ""{CustomSchema}"".""{CustomPublishedTableName}""WHERE ""Id"" = @Id";
            var count = connection.QueryFirstOrDefault<int>(sql, new { Id = long.Parse(mdMessage.DbId) });
            Assert.Equal(1, count);
        }

        public void Dispose()
        {
            using (var connection = ConnectionUtil.CreateConnection())
            {
                connection.Execute($@"
DROP TABLE IF EXISTS ""{CustomSchema}"".""{CustomPublishedTableName}"";
DROP TABLE IF EXISTS ""{CustomSchema}"".""{CustomReceivedTableName}"";
DROP TABLE IF EXISTS ""{CustomSchema}"".""{CustomLockTableName}"";
DROP SCHEMA IF EXISTS ""{CustomSchema}"";
");
            }

            _provider.Dispose();
        }
    }
}