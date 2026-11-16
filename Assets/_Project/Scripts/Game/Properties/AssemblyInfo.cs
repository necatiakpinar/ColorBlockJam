using System.Runtime.CompilerServices;

// Views expose a few read-only counters and their pooled block views to the end-to-end tests only.
[assembly: InternalsVisibleTo("RollicCase.Tests.EditMode")]
[assembly: InternalsVisibleTo("RollicCase.Tests.PlayMode")]
