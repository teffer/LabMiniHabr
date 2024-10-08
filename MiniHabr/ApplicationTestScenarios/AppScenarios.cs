using MiniHabr.Application;
using MiniHabr.CommonHelpers;
using MiniHabr.TestHelpers;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace MiniHabr.ApplicationTestScenarios;
static class AppScenarios {
	public static async Task CheckGetArticleList(Db db) {
		var client = new App(db) {
			CurrentUserId = Guid.Parse("d6bd5bde-7c57-3b81-3487-d1f1f2ebe27b"),
		};
		var articleList1 = await client.GetArticleList(new() {
			StartIndex = 0,
			MaxCount = 2,
		});
		Check.IsTrue(articleList1.Articles.Count == 2);
		var articleList2 = await client.GetArticleList(new() {
			StartIndex = 1,
			MaxCount = 3,
		});
		Check.IsTrue(articleList2.Articles.Count == 3);
		Check.IsTrue(articleList1.TotalCount == articleList2.TotalCount);
		Check.IsTrue(Serializer.ToJson(articleList1.Articles[1]) == Serializer.ToJson(articleList2.Articles[0]));
		var hubId = Guid.Parse("2f81e816-f32d-2caf-6bba-673c84915ee2");
		var articleListByHub = await client.GetArticleList(new() {
			HubId = hubId,
			StartIndex = 0,
			MaxCount = 2,
		});
		Check.IsTrue(articleListByHub.Articles.All(article => article.Hubs.Any(hub => hub.Id == hubId)));
		var companyId = Guid.Parse("22378388-6aad-7fcf-f142-d07f53135304");
		var articleListByCompany = await client.GetArticleList(new() {
			CompanyId = companyId,
			StartIndex = 0,
			MaxCount = 2,
		});
		Check.IsTrue(articleListByCompany.Articles.All(article => article.Company?.Id == companyId));
		Logger.Log($"{nameof(AppScenarios)}.{nameof(CheckGetArticleList)}: ok");
	}
	public static async Task CheckCommentBookmarks(Db db) {
		var userId = Guid.Parse("d6bd5bde-7c57-3b81-3487-d1f1f2ebe27b");
		var client = new App(db) {
			CurrentUserId = userId,
		};
		var anotherUserId = Guid.Parse("78661aad-03d9-819a-2145-84cd9b4d1e54");
		var anotherClient = new App(db) {
			CurrentUserId = anotherUserId,
		};
		var articleIds = new[] {
			Guid.Parse("b2b35173-9880-ebf3-550b-560f7ac283af"),
			Guid.Parse("c9a8f973-9b56-4ed5-765b-b6df569c823b"),
		};
		var articleList1 = await client.GetArticleList(new() {
			StartIndex = 0,
			MaxCount = 2,
		});
		var articleList2 = await anotherClient.GetArticleList(new() {
			StartIndex = 1,
			MaxCount = 3,
		});
		Check.IsTrue((await client.GetUserCard(userId))?.CommentBookmarkCount == 74);
		Check.IsTrue((await client.GetUserCard(anotherUserId))?.CommentBookmarkCount == 103);
		foreach (var articleId in articleIds) {
			var clientArticleWithComments = await client.GetArticle(articleId);
			Check.IsTrue(clientArticleWithComments != null);
			foreach (var c in clientArticleWithComments.Comments) {
				Check.IsTrue(!c.IsBookmarked);
				Check.IsTrue(await client.AddCommentBookmark(c.Id));
				Check.IsTrue(await client.AddCommentBookmark(c.Id));
			}
			var changedClientArticleWithComments = await client.GetArticle(articleId);
			Check.IsTrue(changedClientArticleWithComments != null);
			foreach (var c in changedClientArticleWithComments.Comments) {
				Check.IsTrue(c.IsBookmarked);
			}
		}
		Check.IsTrue((await client.GetUserCard(userId))?.CommentBookmarkCount == 87);
		Check.IsTrue((await client.GetUserCard(anotherUserId))?.CommentBookmarkCount == 103);
		foreach (var articleId in articleIds) {
			var wrongUserArticleWithComments = await anotherClient.GetArticle(articleId);
			Check.IsTrue(wrongUserArticleWithComments != null);
			Check.IsTrue(wrongUserArticleWithComments.Id == articleId);
			Check.IsTrue(!wrongUserArticleWithComments.Comments[0].IsBookmarked);
			var clientArticleWithComments = await client.GetArticle(articleId);
			Check.IsTrue(clientArticleWithComments != null);
			Check.IsTrue(clientArticleWithComments.Id == articleId);
			Check.IsTrue(clientArticleWithComments.Comments[0].IsBookmarked);
			foreach (var c in wrongUserArticleWithComments.Comments) {
				Check.IsTrue(await anotherClient.AddCommentBookmark(c.Id));
				Check.IsTrue(await anotherClient.AddCommentBookmark(c.Id));
			}
			wrongUserArticleWithComments = await anotherClient.GetArticle(articleId);
			Check.IsTrue(wrongUserArticleWithComments != null);
			foreach (var c in wrongUserArticleWithComments.Comments) {
				Check.IsTrue(c.IsBookmarked);
				Check.IsTrue(await anotherClient.RemoveCommentBookmark(c.Id));
				Check.IsTrue(await anotherClient.RemoveCommentBookmark(c.Id));
			}
			clientArticleWithComments = await client.GetArticle(articleId);
			Check.IsTrue(clientArticleWithComments != null);
			foreach (var c in clientArticleWithComments.Comments) {
				Check.IsTrue(c.IsBookmarked);
				Check.IsTrue(await client.RemoveCommentBookmark(c.Id));
				Check.IsTrue(await client.RemoveCommentBookmark(c.Id));
			};
		}
		Check.IsTrue((await client.GetUserCard(userId))?.CommentBookmarkCount == 74);
		Check.IsTrue((await client.GetUserCard(anotherUserId))?.CommentBookmarkCount == 103);
		Logger.Log($"{nameof(AppScenarios)}.{nameof(CheckCommentBookmarks)}: ok");
	}
	public static async Task CheckAddNewArticle(Db db) {
		var anonymousClient = new App(db);
		var currentUserId = Guid.Parse("d6bd5bde-7c57-3b81-3487-d1f1f2ebe27b");
		var client = anonymousClient with {
			CurrentUserId = currentUserId,
		};
		var anotherClient = anonymousClient with {
			CurrentUserId = Guid.Parse("78661aad-03d9-819a-2145-84cd9b4d1e54"),
		};
		var userCard = await client.GetUserCard(currentUserId);
		Check.IsTrue(userCard != null);
		var userCardJson = Serializer.ToJson(userCard);
		foreach (var c in new[] { anonymousClient, anotherClient }) {
			Check.IsTrue(Serializer.ToJson(await anonymousClient.GetUserCard(currentUserId)) == userCardJson);
		}
		var newArticle = new NewArticle() {
			Title = "t1",
			Content = "content1",
			HubIds = new[]{
				Guid.Parse("2f81e816-f32d-2caf-6bba-673c84915ee2"),
				Guid.Parse("513fccaa-bd89-4c5f-1ef3-130293174cd2"),
			},
			Polls = Enumerable.Range(1, 2).Select(pollIndex => new NewArticle.Poll() {
				Title = $"poll {pollIndex} title",
				Multiple = pollIndex % 2 == 0,
				Variants = Enumerable.Range(1, 3).Select(variantIndex => new NewArticle.Poll.Variant {
					Title = $"poll {pollIndex} variant {variantIndex} title",
				}).ToList(),
			}).ToList(),
		};
		Check.IsTrue(await anonymousClient.AddNewArticle(newArticle) == null);
		var maybeArticleId = await client.AddNewArticle(newArticle);
		Check.IsTrue(maybeArticleId != null);
		var articleId = maybeArticleId.Value;
		var article1 = await client.GetArticle(articleId);
		Check.IsTrue(article1 != null);
		foreach (var c in new[] { anonymousClient, anotherClient }) {
			Check.IsTrue(await c.GetArticle(articleId) == null);
			Check.IsTrue(!await c.SetArticleIsPublished(articleId, true));
			Check.IsTrue(!await c.SetArticleIsPublished(articleId, false));
			Check.IsTrue(Serializer.ToJson(await anonymousClient.GetUserCard(currentUserId)) == userCardJson);
		}
		Check.IsTrue(await client.SetArticleIsPublished(articleId, true));
		Check.IsTrue(await client.SetArticleIsPublished(articleId, true));
		foreach (var c in new[] { client, anonymousClient, anotherClient }) {
			Check.IsTrue(await c.GetArticle(articleId) != null);
			var newUserCard = await anonymousClient.GetUserCard(currentUserId);
			Check.IsTrue(Serializer.ToJson(newUserCard) == Serializer.ToJson(userCard with {
				ArticleCount = userCard.ArticleCount + 1,
			}));
		}
		foreach (var c in new[] { anonymousClient, anotherClient }) {
			Check.IsTrue(!await c.SetArticleIsPublished(articleId, true));
			Check.IsTrue(!await c.SetArticleIsPublished(articleId, false));
		}
		Check.IsTrue(await client.SetArticleIsPublished(articleId, false));
		Check.IsTrue(await client.SetArticleIsPublished(articleId, false));
		foreach (var c in new[] { anonymousClient, anotherClient }) {
			Check.IsTrue(await c.GetArticle(articleId) == null);
			Check.IsTrue(!await c.SetArticleIsPublished(articleId, true));
			Check.IsTrue(!await c.SetArticleIsPublished(articleId, false));
		}
		foreach (var c in new[] { client, anonymousClient, anonymousClient }) {
			Check.IsTrue(Serializer.ToJson(await anonymousClient.GetUserCard(currentUserId)) == userCardJson);
		}
		Logger.Log($"{nameof(AppScenarios)}.{nameof(CheckAddNewArticle)}: ok");
	}
}
