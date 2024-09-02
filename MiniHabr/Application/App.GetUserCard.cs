using System;
using System.Threading.Tasks;
namespace MiniHabr.Application;
sealed partial record App {
	public async Task<UserCard?> GetUserCard(Guid userId) {
		await using var cursor = await Db.Connect();
		return await cursor.QueryFirst(
			row => row == null ? null : new UserCard {
				Id = row.GetGuid("id"),
				FullName = row.GetString("full_name"),
				Handle = row.GetString("handle"),
				ArticleCount = row.GetInt("article_count"),
				CommentCount = row.GetInt("comment_count"),
				CommentBookmarkCount = row.GetInt("comment_bookmark_count"),
			},
			@"
SELECT
	au.id,
	au.full_name,
	au.handle,
	(
		SELECT
			CAST(count(*) AS int)
		FROM
			articles a
		WHERE
			a.is_published
			AND a.author_user_id = au.id
	) AS article_count,
	(
		SELECT
			CAST(count(*) AS int)
		FROM
			article_comments ac
			JOIN articles a ON a.id = ac.article_id
		WHERE
			a.is_published
			AND ac.user_id = au.id
	) AS comment_count,
	(
		SELECT
			CAST(count(*) AS int)
		FROM
			comment_bookmarks cb
		WHERE
			cb.user_id = au.id
	) AS comment_bookmark_count
FROM
	app_users au
WHERE
	au.id = :user_id
",
			new() {
				{ "user_id", userId },
			}
		);
	}
}
