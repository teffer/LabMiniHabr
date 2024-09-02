namespace MiniHabr.CommonHelpers;
static class StringExtensions {
	public static string ToLiteral(this string s) {
		return '\'' + s.Replace("'", "''") + '\'';
	}
}
