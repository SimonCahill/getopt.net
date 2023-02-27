---
name: Bug report
about: Create a report to help us improve
title: "[BUG]"
labels: bug
assignees: SimonCahill

---

**Brief description** (should also be the title!)

My foo isn't baring. 

**Detailled description**

When I add foo to my bar, I get baz.
This issue happens on any Windows machine running .net6 or higher.

**Expected behaviour**
I expect foo + bar == foobar.

**Actual behaviour**

foo + bar == baz

**Minimal, reproducible example**

\```csharp
var foo = "ba";
var bar = "z";

Console.WriteLine(foo + bar);
\```

**Applies to version(s)**

This problem applies to:
 - all versions?
 - a specific version?
 - a range of versions?
