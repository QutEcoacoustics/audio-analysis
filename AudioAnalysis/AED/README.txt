Dependencies

------------


F#
http://msdn.microsoft.com/en-gb/fsharp/default.aspx
Version: 1.9.6.16


xUnit.net
http://www.codeplex.com/xunit
Version: 1.5 CTP1


Tests

-----

To execute the tests:

1. Build solution in Visual Studio

2. cd Test

3. PATH_TO_XUNIT/xunit.console.exe bin/Debug/Test.dll


Releasing
---------

Ideas for release process
- make sure all code is checked in
- remove dev from version number
- do a clean and then a release build
- run tests
- tag svn repo with version number
- add dll to svn repo
- increment version number by 1, append dev suffix & commit

Not sure how this fits in with MS build/version standards.
