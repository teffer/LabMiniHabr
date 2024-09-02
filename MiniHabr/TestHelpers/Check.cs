using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
namespace MiniHabr.TestHelpers;
static class Check {
	public static void IsTrue([DoesNotReturnIf(false)] bool condition, [CallerArgumentExpression(nameof(condition))] string? conditionMessage = null) {
		if (condition) {
			return;
		}
		throw new InvalidOperationException(conditionMessage);
	}
}
