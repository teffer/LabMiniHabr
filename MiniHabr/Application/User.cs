using MiniHabr.CommonHelpers;
using System;
namespace MiniHabr.Application;
sealed record User {
	public required Guid Id { get; init; }
	public required string FullName { get; init; }
	public required string Handle { get; init; }
	public static User FromIdFullNameHandleSequentialRow(SequentialDbRow row) {
		return new User {
			Id = row.ReadGuid(),
			FullName = row.ReadString(),
			Handle = row.ReadString(),
		};
	}
}
