using System;
namespace MiniHabr.Application.Common;
interface IGuidGenerator {
	Guid GetNextGuid();
}
