# getop.net changelog

# v1.0.0
Version 1.0.0 introduces a non-breaking feature; the dynamic creation of help menus.

## Changes
 - getopt.net now generates a dynamic help text.
    - Added customisable extension method which can be used to dynamically generate help texts.

# v0.9.1
Version 0.9.1 introduces a non-breaking change which allows creating a shortopt string from an array of long options.

## Changes
 - Added extension method Option[].ToShortOptString()
 - Updated tests

# v0.9.0
Version 0.9.0 introduces a non-breaking change which fixes a bug in paramfiles.

## Changes
 - Paramfiles now support comments.
 - Comments are only supported at the beginning of lines
 - Comments are denoted by a preceeding `#`

# v0.8.1
Version 0.8.1 introduces a non-breaking change which allows for more than 255 possible values for long options.

## Changes
 - Option struct now contains an Int32 for its Value property
    - An override constructor was added to allow for backwards-compatiblity.

# v0.8.0

Version 0.8.0 introduces a non-breaking change which enables support for paramfiles!

Some applications, notably GCC, use paramfiles as a way to pass a large amount of options and arguments to an application.
Paramfiles are line-separated lists of arguments and can be enabled by setting `AllowParamFiles = true`.
Each line in the paramfile will be parsed as if it were passed directly to getopt.net!
To allow Powershell or Windows conventions, you will still need to enable `AllowWindowsConventions` or `AllowPowershellConventions` respectively.

## Changes
 - Added support for paramfiles
 - Updated reference implementations

# v0.7.0

Version 0.7.0 introduces a non-breaking change which enables support for Powershell-style options!
Check out the wiki/README for more information!

## Changes
 - Added support for Powershell-style options!

# v0.6.0

Version 0.6.0 introduces a non-breaking change which enabled support for optional short-opt arguments!
Check out the wiki/README for more info!

## Changes
  - Added support for optional short args!


# v0.5.1

## Changes
 - Added new property to completely enable or disable exceptions
 - This change does **NOT** affect the way the `IgnoreXXX` properties work!

# v0.5.0

## Changes
 - Added support for Windows-style command-line options!

# v0.4.0
This release introduces a potentially breaking change!

## Changes:
 - Removed support for `Flag` property in `Option`


# v0.3.1
This is a minor release related to #11.

## Changes:

 - DoubleDashStopsParsing is now default true.
 - Bumped up version to 0.3.1

# v0.3.0

 - Added check to determine whether 0x01 must be returned, depending on whether or not `ShortOpts[0] == '-'`.
 - Added check to determine if parsing should stop, depending on whether or not `ShortOpts[0] == '+'`.
 - If `IgnoreInvalidOptions` is `true` and `MustReturnChar1()` returns `true`, then `NonOptChar` is returned.
 - Previously private `const`s are now public
 - If a non-option string is encountered and `IgnoreInvalidOptions` is `true`, `outOptArg` is set to the value of the non-option string.
 - Updated and refactored MSTests
 - Corrected false behavior where encountering "--" would immediately stop GetNextOpt
   - the correct behavior is to simply stop parsing options, but still return `0x01` and `outOptArg` contains the value of the argument passed
 - bumped up library version

## Issues fixed

This PR fixes #8.

# v0.2.0b
 - Bugfix patch
 - Fixes index out of bounds exception when looping through arguments


# v0.1.0b
 - Initial release
 - Implements full functionality of getopt
