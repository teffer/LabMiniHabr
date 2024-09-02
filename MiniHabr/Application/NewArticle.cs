using System;
using System.Collections.Generic;
namespace MiniHabr.Application;
sealed record NewArticle {
	public required string Title { get; init; }
	public required string Content { get; init; }
	public required IReadOnlyList<Guid> HubIds { get; init; }
	public required IReadOnlyList<Poll> Polls { get; init; }
	public sealed record Poll {
		public required string Title { get; init; }
		public required bool Multiple { get; init; }
		public required IReadOnlyList<Variant> Variants { get; init; }
		public sealed record Variant {
			public required string Title { get; init; }
		}
	}
}
