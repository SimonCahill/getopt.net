# getop.net changelog

# v0.3.1
This is a minor release related to #11.

# Changes:

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
