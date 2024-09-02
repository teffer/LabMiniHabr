using System;
using System.Globalization;
namespace MiniHabr.CommonHelpers;
static class DateTimeHelper {
	public static DateTime ParseUtcIso(string s) {
		return DateTime.ParseExact(s, "s", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
	}
}
