using MiniHabr.CommonHelpers;
using System;
using System.Threading.Tasks;
namespace MiniHabr.Application;
sealed partial record App {
	public async Task<bool> AddCommentBookmark(Guid commentId) {
		await using var cursor = await Db.Connect(readWrite: true);
		var creationTime = GetCurrentTime();
		var commentBookmarkExists = await cursor.QueryFirst(
			dbRow => dbRow != null,
			@"
SELECT
	1
WHERE
	EXISTS (
		SELECT
			1
		FROM
			comment_bookmarks cb
		WHERE
			cb.comment_id = :comment_id
			AND cb.user_id = :user_id
	)
			",
			new()
			{
				{ "comment_id", commentId  },
				{ "user_id", CurrentUserId  },
			}
		);
		if (commentBookmarkExists) {
			return true;
		}
		var affected = await cursor.Execute(
			@"
INSERT INTO
	comment_bookmarks (
		comment_id,
		user_id,
		creation_time
	)
SELECT
	:comment_id,
	:user_id,
	:creation_time
WHERE
	EXISTS (
		SELECT
			1
		FROM
			article_comments ac
		WHERE
			ac.id = :comment_id
	)
			",
			new() {
				{ "comment_id", commentId },
				{ "user_id", CurrentUserId },
				{ "creation_time", creationTime },
			}
		);
		if (affected != 1) {
			return false;
		}
		await cursor.Commit();
		return true;
	}
}
