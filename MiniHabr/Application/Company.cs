using MiniHabr.CommonHelpers;
using System;
namespace MiniHabr.Application;
sealed record Company {
	public required Guid Id { get; init; }
	public required string Name { get; init; }
	public required string Handle { get; init; }
	public static Company FromIdNameHandleSequentialRow(SequentialDbRow row) {
		return new Company {
			Id = row.ReadGuid(),
			Name = row.ReadString(),
			Handle = row.ReadString(),
		};
	}
}
