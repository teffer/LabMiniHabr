using System;
using System.Collections.Generic;
namespace MiniHabr.Application;
sealed record ArticleList {
	public required IReadOnlyList<Article> Articles { get; init; }
	public required int TotalCount { get; init; }
	public sealed record Article {
		public required Guid Id { get; init; }
		public required string Title { get; init; }
		public required string ContentPreview { get; init; }
		public required User Author { get; init; }
		public required Company? Company { get; init; }
		public required bool IsPublished { get; init; }
		public required DateTime PublicationTime { get; init; }
		public required long ViewCount { get; init; }
		public required IReadOnlyList<Hub> Hubs { get; init; }
	}
}
