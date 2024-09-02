using System.Collections.Generic;
using System.Linq;
namespace MiniHabr.CommonHelpers;
static class SqlDmlHelper {
	const string indent = "\t\t";
	public static string MakeInsert(string tableName, IEnumerable<string> parameterNames) {
		return @$"
INSERT INTO
	{tableName} (
{string.Join(",\n", parameterNames.Select(name => $"{indent}{name}"))}
	)
VALUES
	(
{string.Join(",\n", parameterNames.Select(name => $"{indent}:{name}"))}
	)
".Trim();
	}
	public static string MakeUpdateAssignments(IEnumerable<string> parameterNames) {
		return string.Join(",\n", parameterNames.Select(name => $"{indent}{name} = :{name}"));
	}
	public static string QuoteName(string name) {
		return '\"' + name.Replace("\"", "\"\"") + '\"';
	}
}
