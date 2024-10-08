using Microsoft.Extensions.Logging;
using Npgsql;
using Npgsql.PostgresTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
namespace MiniHabr.CommonHelpers;
sealed class DbCursor : IAsyncDisposable {
	readonly NpgsqlConnection connection;
	readonly ILogger logger;
	public DbCursor(NpgsqlConnection connection, ILoggerFactory loggerFactory) {
		this.connection = connection;
		logger = loggerFactory.CreateLogger<DbCursor>();
	}
	bool commited;
	void CheckNotCommited() {
		if (commited) {
			throw new InvalidOperationException("Already commited");
		}
	}
	public async Task<int> Execute(
		string sql,
		QueryParameters? parameters = null,
		CancellationToken ct = default
	) {
		CheckNotCommited();
		await using var command = MakeCommand(sql, parameters);
		var affected = await command.ExecuteNonQueryAsync(ct);
		return affected;
	}
	public async Task<List<T>> QueryList<T>(
		Func<DbRow, T> makeItem,
		string sql,
		QueryParameters? parameters = null,
		CancellationToken ct = default
	) {
		CheckNotCommited();
		await using var command = MakeCommand(sql, parameters);
		await using var reader = await command.ExecuteReaderAsync(multipleRowsCommandBehavior, ct);
		LogReader(reader);
		var dbRow = new NpgsqlDataReaderDbRow(reader);
		var items = new List<T>();
		while (await reader.ReadAsync(ct)) {
			var item = makeItem(dbRow);
			dbRow.FinishRow();
			items.Add(item);
		}
		return items;
	}
	public async Task<List<T>> QueryListSequentialColumns<T>(
		Func<SequentialDbRow, T> makeItem,
		string sql,
		QueryParameters? parameters = null,
		CancellationToken ct = default
	) {
		CheckNotCommited();
		await using var command = MakeCommand(sql, parameters);
		await using var reader = await command.ExecuteReaderAsync(multipleRowsCommandBehavior, ct);
		LogReader(reader);
		var dbRow = new NpgsqlDataReaderSequentialDbRow(reader);
		var items = new List<T>();
		while (await reader.ReadAsync(ct)) {
			var item = makeItem(dbRow);
			dbRow.FinishRow();
			items.Add(item);
		}
		return items;
	}
	public async IAsyncEnumerable<T> QueryAsyncEnumerable<T>(
		Func<DbRow, T> makeItem,
		string sql,
		QueryParameters? parameters = null,
		[EnumeratorCancellation]
		CancellationToken ct = default
	) {
		CheckNotCommited();
		await using var command = MakeCommand(sql, parameters);
		await using var reader = await command.ExecuteReaderAsync(multipleRowsCommandBehavior, ct);
		LogReader(reader);
		var dbRow = new NpgsqlDataReaderDbRow(reader);
		var items = new List<T>();
		while (await reader.ReadAsync(ct)) {
			var item = makeItem(dbRow);
			dbRow.FinishRow();
			yield return item;
		}
	}
	const CommandBehavior multipleRowsCommandBehavior =
		CommandBehavior.SingleResult |
		CommandBehavior.SequentialAccess;
	public async Task<T> QueryFirst<T>(
		Func<DbRow?, T> makeItem,
		string sql,
		QueryParameters? parameters = null,
		CancellationToken ct = default
	) {
		CheckNotCommited();
		await using var command = MakeCommand(sql, parameters);
		await using var reader = await command.ExecuteReaderAsync(singleRowCommandBehavior, ct);
		LogReader(reader);
		T item = default!;
		var hasItem = false;
		var dbRow = new NpgsqlDataReaderDbRow(reader);
		while (await reader.ReadAsync(ct)) {
			if (hasItem) {
				continue;
			}
			hasItem = true;
			item = makeItem(dbRow);
			dbRow.FinishRow();
		}
		if (!hasItem) {
			item = makeItem(null);
		}
		return item;
	}
	const CommandBehavior singleRowCommandBehavior =
		CommandBehavior.SingleResult |
		CommandBehavior.SingleRow |
		CommandBehavior.SequentialAccess;
	NpgsqlCommand MakeCommand(string sql, QueryParameters? parameters) {
		var command = connection.CreateCommand();
		command.CommandText = sql.Trim();
		if (parameters != null) {
			foreach (var (name, value) in parameters.NameValuePairs) {
				var parameter = new NpgsqlParameter(name, value ?? DBNull.Value);
				command.Parameters.Add(parameter);
			}
		}
		return command;
	}
	void LogReader(NpgsqlDataReader reader) {
		if (logger.IsEnabled(LogLevel.Trace)) {
			logger.LogTrace("{Message}", GetColumnInfosString(reader));
		}
	}
	static string GetColumnInfosString(NpgsqlDataReader reader) {
		return $"Columns:\n{string.Concat(GetColumnInfos(reader).Select(x => $"  {x}\n"))}";
	}
	sealed record ColumnInfo(
		string Name,
		Type FieldType,
		Type ProviderSpecificFieldType,
		string DataTypeName,
		PostgresType PostgresType,
		uint DataTypeOid
	) {
	}
	static IEnumerable<ColumnInfo> GetColumnInfos(NpgsqlDataReader reader) {
		return Enumerable.Range(0, reader.FieldCount).Select(x => {
			return new ColumnInfo(
				Name: reader.GetName(x),
				FieldType: reader.GetFieldType(x),
				ProviderSpecificFieldType: reader.GetProviderSpecificFieldType(x),
				DataTypeName: reader.GetDataTypeName(x),
				PostgresType: reader.GetPostgresType(x),
				DataTypeOid: reader.GetDataTypeOID(x)
			);
		});
	}
	public async Task Commit() {
		CheckNotCommited();
		await Db.CommitTransaction(connection);
		commited = true;
	}
	public ValueTask DisposeAsync() {
		return connection.DisposeAsync();
	}
}
