using MiniHabr.CommonHelpers;
using System;
namespace MiniHabr.TestHelpers;
static class Logger {
	public static void Log<T>(T value) {
		Console.WriteLine(AddEmptyLine(SerializeValue(value)));
	}
	static string SerializeValue(object? value) {
		if (value is string stringValue) {
			return stringValue;
		}
		return Serializer.ToJson(value);
	}
	static string AddEmptyLine(string message) {
		if (message.Contains('\n')) {
			message = "\n" + message;
		}
		return message;
	}
}
