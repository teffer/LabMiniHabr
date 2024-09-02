using MiniHabr.CommonHelpers;
using System;
namespace MiniHabr.Application;
sealed record Hub {
	public required Guid Id { get; init; }
	public required string Name { get; init; }
	public required string Handle { get; init; }
	public static Hub FromIdNameHandleSequentialRow(SequentialDbRow row) {
		return new Hub {
			Id = row.ReadGuid(),
			Name = row.ReadString(),
			Handle = row.ReadString(),
		};
	}
}
