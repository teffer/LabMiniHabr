using MiniHabr.CommonHelpers;
using System;
using System.Threading.Tasks;
namespace MiniHabr.Application;
sealed partial record App {
	public async Task<bool> RemoveCommentBookmark(Guid commentId) {
		await using var cursor = await Db.Connect(readWrite: true);
		var userAndCommentExists = await cursor.QueryFirst(
			dbRow => dbRow != null,
			@"
SELECT
	1
WHERE
	EXISTS (
		SELECT
			1
		FROM
			app_users au
		WHERE
			au.id = :user_id
	)
	AND EXISTS (
		SELECT
			1
		FROM
			article_comments ac
		WHERE
			ac.id = :comment_id
	);
				",
			new()
			{
				{ "comment_id", commentId },
				{ "user_id", CurrentUserId },
			}
		);
		if (!userAndCommentExists) {
			return false;
		}
		await cursor.Execute(
			@"
DELETE FROM
	comment_bookmarks cb
WHERE
	cb.comment_id = :comment_id
	AND cb.user_id = :user_id
",
			new() {
				{ "comment_id", commentId },
				{ "user_id", CurrentUserId },
			}
		);
		await cursor.Commit();
		return true;
	}
}
