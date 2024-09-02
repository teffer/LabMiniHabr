using System;
using System.Collections.Generic;
namespace MiniHabr.Application;
sealed record Article {
	public required Guid Id { get; init; }
	public required string Title { get; init; }
	public required string Content { get; init; }
	public required User Author { get; init; }
	public required Company? Company { get; init; }
	public required bool IsPublished { get; init; }
	public required DateTime PublicationTime { get; init; }
	public required long ViewCount { get; init; }
	public required IReadOnlyList<Hub> Hubs { get; init; }
	public required IReadOnlyList<Poll> Polls { get; init; }
	public required IReadOnlyList<Comment> Comments { get; init; }
	public sealed record Poll {
		public required Guid Id { get; init; }
		public required string Title { get; init; }
		public required bool Multiple { get; init; }
		public required IReadOnlyList<Variant> Variants { get; init; }
		public sealed record Variant {
			public required Guid Id { get; init; }
			public required string Title { get; init; }
		}
	}
	public sealed record Comment {
		public required Guid Id { get; init; }
		public required User Author { get; init; }
		public required DateTime PublicationTime { get; init; }
		public required bool IsBookmarked { get; init; }
		public required string Content { get; init; }
		public required IReadOnlyList<Comment> Children { get; init; }
	}
}
