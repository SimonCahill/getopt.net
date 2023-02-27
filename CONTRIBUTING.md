# Preamble

I hope for this project to become widespread and community-driven.
Originally started because I wanted getopt-like functionality for one of my C# projects,<br >
and all the previously available options were fairly outdated and didn't support newer language features/weren't being maintained.

In this document, I'd like to outline some of my expectations for contributing to getopt.net.

# Reporting Issues

No piece of software is perfect, much less any software I've written.<br >
Bugs and even poor choices on my end may lead to unexpected behaviour.

If you believe to have found a bug, then please create an [Issue](https://github.com/SimonCahill/getopt.net/issues) here on GitHub.

When you create your issue, please be sure to provide some debugging information.

Your issue should look a little like this:

```
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

```

# Creating pull requests

If you've found an issue with getopt.net or you have added a feature you find useful, which isn't provided, create a fork of getopt.net and add your changes to a <u >feature branch</u>.

A feature branch should look something like this: `feat/name-of-feature-or-fix`.

Before you create a pull request, please ensure all tests pass and create new tests to test your enhancements.
Pull requests failing tests/lacking new tests for new features will be rejected.

All tests can be found in `getopt.net.tests`.

Your pull request should then look something like this:

```
# Bug fix for XXX
OR
# New feature YYY

<describe your bugfix or addition>

# Proposed new version

I propose this bug fix/feature enhancement get the version X.X.X because:
 - reasons
 - and more
 
# Tests modified or added

<add any tests you have modified (with a reason) or any tests you have added>
```

