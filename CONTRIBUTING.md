# Preamble

I hope for this project to become widespread and community-driven.
Originally started because I wanted getopt-like functionality for one of my C# projects,<br >
and all the previously available options were fairly outdated and didn't support newer language features/weren't being maintained.

In this document, I'd like to outline some of my expectations for contributing to getopt.net.

# Development container

The repository includes an Alpine Linux devcontainer with the .NET 8 SDK, Git, OpenSSH and Zsh.
Oh My Zsh uses the `jispwoso` theme with the `git` and `dotnet` plugins enabled.

Install Docker and the VS Code Dev Containers extension, then run **Dev Containers: Reopen in Container**.
The container restores the solution automatically and runs as the non-root `vscode` user.

The .NET SDK restores the .NET Framework 4.6 reference assemblies through NuGet, allowing both library targets to build on Alpine.
The native .NET Framework runtime requires Windows; application execution and tests in this container use .NET 8.

```sh
dotnet build getopt.net/getopt.net.csproj --configuration Release --framework net46 --no-restore
dotnet build getopt.net/getopt.net.csproj --configuration Release --framework net8.0 --no-restore
dotnet test getopt.net.tests/getopt.net.tests.csproj --configuration Release --framework net8.0 --no-restore
```

The host user's `.ssh` directory is mounted read-only at `/home/vscode/.ssh`, including SSH configuration, keys and known hosts.
Create that directory on the host before opening the container. Add new SSH hosts to the host's `known_hosts` file there as needed.
Use `~/.ssh/...` paths in SSH configuration; absolute host-specific paths need to be adjusted to paths available inside the container.
For encrypted keys, load the key into the host SSH agent before opening VS Code; Dev Containers forwards the running agent automatically.
Git operations can then use the repository's SSH remote with the mounted configuration and available credentials.

Oh My Zsh is pinned through `OH_MY_ZSH_COMMIT` in `.devcontainer/Dockerfile`. Update that revision and rebuild the container to upgrade it.

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

