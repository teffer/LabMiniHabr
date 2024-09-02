using System;
namespace MiniHabr.Application.Common;
sealed class SystemClock : IClock {
	public static readonly SystemClock Instance = new();
	public DateTime GetCurrentTime() {
		return DateTime.UtcNow;
	}
}
