using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using MiniHabr.ApplicationTestScenarios;
using MiniHabr.CommonHelpers;
using MiniHabr.TestHelpers;
using System;
using System.Globalization;
using System.Threading.Tasks;
namespace MiniHabr;
static class Program {
	public static async Task Main() {
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
		Console.OutputEncoding = System.Text.Encoding.UTF8;
		Console.WriteLine("Проверка Unicode: 👍");
		var db = new Db("Host=127.0.0.2;Port=5433;Database=mini_habr;Search Path=public;Username=postgres;Password=123") {
			LoggerFactory = MakeLoggerFactory(LogLevel.Warning),
		};
		if (true) { await DbExamples.RunAll(db); }
		if (!true) { await AppDbTools.TruncateAllTables(db); }
		if (!true) { await AppDbTools.AddRandomData(db, new RandomDataGenerator(123)); }
		if (true) { await AppScenarios.CheckGetArticleList(db); };
		if (true) { await AppScenarios.CheckCommentBookmarks(db); };
		if (true) { await AppScenarios.CheckAddNewArticle(db); };
	}
	static ILoggerFactory MakeLoggerFactory(LogLevel logLevel) {
		return LoggerFactory.Create(builder => {
			builder.SetMinimumLevel(logLevel);
			builder.AddSimpleConsole(options => {
				options.ColorBehavior = LoggerColorBehavior.Enabled;
				options.IncludeScopes = true;
				options.SingleLine = true;
				options.TimestampFormat = "hh:mm:ss.fff ";
			});
		});
	}
}
