using System;
using System.Threading.Tasks;
namespace MiniHabr.Application;
sealed partial record App {
	public async Task<bool> SetArticleIsPublished(Guid articleId, bool isPublished) {
		await using var cursor = await Db.Connect(readWrite: true);
		var changed = await cursor.Execute(
			@"
UPDATE
	articles a
SET
	is_published = :is_published,
	publication_time = CASE
		WHEN (
			:is_published
			AND NOT is_published
		) THEN :current_time
		ELSE a.publication_time
	END
WHERE
	a.id = :article_id
	AND a.author_user_id = :current_user_id
",
			new() {
				{ "article_id", articleId },
				{ "is_published", isPublished },
				{ "current_time", GetCurrentTime() },
				{ "current_user_id", CurrentUserId },
			}
		);
		if (changed != 1) {
			return false;
		}
		await cursor.Commit();
		return true;
	}
}
