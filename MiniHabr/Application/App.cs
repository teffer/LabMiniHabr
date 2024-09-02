using MiniHabr.Application.Common;
using MiniHabr.CommonHelpers;
using System;
namespace MiniHabr.Application;
sealed partial record App(Db Db) {
	public IClock Clock { get; init; } = SystemClock.Instance;
	DateTime GetCurrentTime() {
		return Clock.GetCurrentTime();
	}
	public IGuidGenerator GuidGenerator { get; init; } = RandomGuidGenerator.Instance;
	Guid GetNextGuid() {
		return GuidGenerator.GetNextGuid();
	}
	public Guid? CurrentUserId { get; init; }
}
