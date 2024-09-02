using System;
namespace MiniHabr.Application.Common;
sealed class RandomGuidGenerator : IGuidGenerator {
	public static readonly RandomGuidGenerator Instance = new();
	public Guid GetNextGuid() {
		return Guid.NewGuid();
	}
}
