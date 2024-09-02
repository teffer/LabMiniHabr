using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace MiniHabr.CommonHelpers;
sealed record Db {
	public Db(string connectionString) {
		ConnectionString = connectionString;
	}
	const string defaultConnectionString = "Include Error Detail=true;Timeout=1024";
	string connectionString = "";
	public string ConnectionString {
		get => connectionString;
		init {
			connectionString = value;
			realConnectionString = new NpgsqlConnectionStringBuilder(
				$"{defaultConnectionString};{connectionString}"
			).ConnectionString;
		}
	}
	string realConnectionString = defaultConnectionString;
	public ILoggerFactory LoggerFactory { get; init; } = NullLoggerFactory.Instance;
	public async Task<DbCursor> Connect(bool serializable = true, bool readWrite = false, CancellationToken ct = default) {
		return await ConnectCustom(async connection => {
			var attempts = 10;
			while (attempts > 0) {
				attempts -= 1;
				try {
					await connection.OpenAsync(ct);
					break;
				}
				catch (NpgsqlException e) when (attempts > 0 && e.SqlState == PostgresErrorCodes.TooManyConnections) {
					await Task.Delay(100, ct);
					continue;
				}
			}
			await BeginTransaction(
				connection: connection,
				serializable: serializable,
				readWrite: readWrite,
				ct: ct
			);
		});
	}
	public async Task<DbCursor> ConnectCustom(Func<NpgsqlConnection, Task> setupConnection) {
		var dataSourceBuilder = new NpgsqlDataSourceBuilder(realConnectionString);
		dataSourceBuilder.UseLoggerFactory(LoggerFactory);
		dataSourceBuilder.EnableParameterLogging();
		await using var dataSource = dataSourceBuilder.Build();
		var connection = dataSource.CreateConnection();
		await setupConnection(connection);
		return new DbCursor(connection, LoggerFactory);
	}
	public static async Task BeginTransaction(NpgsqlConnection connection, bool serializable, bool readWrite, CancellationToken ct = default) {
		await using var command = connection.CreateCommand();
		command.CommandText = string.Join(" ", new[] {
			"BEGIN",
			$"ISOLATION LEVEL {(serializable ? "SERIALIZABLE" : "READ COMMITTED")}",
			readWrite ? "READ WRITE": "READ ONLY",
		});
		await command.ExecuteNonQueryAsync(ct);
	}
	public static async Task CommitTransaction(NpgsqlConnection connection, CancellationToken ct = default) {
		await using var command = connection.CreateCommand();
		command.CommandText = "COMMIT";
		await command.ExecuteNonQueryAsync(ct);
	}
}
