using System.Text.Encodings.Web;
using System.Text.Json;
namespace MiniHabr.CommonHelpers;
static class Serializer {
	public static string ToJson(object? value) {
		return JsonSerializer.Serialize(value, jsonSerializerOptions);
	}
	static readonly JsonSerializerOptions jsonSerializerOptions = new() {
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	};
}
