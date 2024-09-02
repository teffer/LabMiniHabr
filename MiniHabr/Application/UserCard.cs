using System;
namespace MiniHabr.Application;
sealed record UserCard {
	public required Guid Id { get; init; }
	public required string FullName { get; init; }
	public required string Handle { get; init; }
	public required int ArticleCount { get; init; }
	public required int CommentCount { get; init; }
	public required int CommentBookmarkCount { get; init; }
}
