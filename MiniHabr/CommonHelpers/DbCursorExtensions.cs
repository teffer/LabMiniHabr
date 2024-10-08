using System.Threading.Tasks;
namespace MiniHabr.CommonHelpers;
static class DbCursorExtensions {
	public static async Task<int> Insert(this DbCursor cursor, string tableName, QueryParameters parameters) {
		var sql = SqlDmlHelper.MakeInsert(tableName, parameters.Names);
		return await cursor.Execute(sql, parameters);
	}
}
