---
name: record-agent-capabilities
description: Record what the current task verified about tools, services, Unity, MCP servers, the shell or the harness in Docs/General/agent-capabilities.md - new abilities, verified failures, workarounds and gotchas - and correct stale entries in place. Run at the end of every task before the final report, and immediately when an entry on that page proves wrong.
---

# Record Agent Capabilities

`Docs/General/agent-capabilities.md` saves the next agent from rediscovering the same limits.

## Workflow

1. **Collect** what this session verified: commands and tools that worked, exact errors, workarounds,
   limits, timings, which Unity instance the MCP served. One sentence each: what, how, result.
2. **Read** the page and search it for each candidate.
3. **Classify:** already recorded and right → nothing; recorded but wrong → fix in place (never add a
   contradicting line); new → add under the section that owns the topic (or **Other**).
4. **Write** one bullet per fact: exact invocation, observed result or error text, conditions, date
   `YYYY-MM-DD`. Update the page's `Updated:` date.
5. **Report** the edits (or "nothing new") in the final message.

## Never record

Guesses, things only read about, unreproduced one-off flakes, secrets, tokens or personal data.
