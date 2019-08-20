# EditorConfig .NET Core

The EditorConfig .NET core provides the same functionality as the
[EditorConfig C Core][] and [EditorConfig Python Core][].

## Installation

The library exists on nuget under 

```
nuget install editorconfig
```

The commandline tooling is uploaded to chocolatey under

```
cinst editorconfig.core
```

## Usage

Usage as a library:

```csharp
var parser = new EditorConfigParser();
var configuration = parser.Parse(fileName);
foreach (var kv in configuration.Properties)
{
    Console.WriteLine("{0}={1}", kv.Key, kv.Value);
}
```

Usage as a command line tool:

```
> editorconfig.exe

    Usage: editorconfig [OPTIONS] FILEPATH1 [FILEPATH2 FILEPATH3 ...]

    EditorConfig .NET Core Version 0.12

    FILEPATH can be a hyphen (-) if you want path(s) to be read from stdin.

    Options:

        -h, --help     output usage information
        -V, --version  output the version number
        -f <path>      Specify conf filename other than ".editorconfig"
        -b <version>   Specify version (used by devs to test compatibility)
```

Example:

    > editorconfig.exe C:\Users\zoidberg\Documents\anatomy.md
    charset=utf-8
    insert_final_newline=true
    end_of_line=lf
    tab_width=8
    trim_trailing_whitespace=sometimes


## Development

Clone this repos and init the test submodule
```
git clone git@github.com:editorconfig/editorconfig-core-net.git
git submodule init
git submodule update
```

building in visual studio should just work (tm)

Building on the command line (will run all the unit tests too)

```
build
```

Release builds can be made using

```
build release X.X.X
```

# Testing

We have several NUnit tests that you can run from visual studio or the build scripts. 

If you want to run the official editorconfig tests you'll need to install [CMAKE](http://www.cmake.org) and call

```
cmake .
``` 

in the root of this repository once.

After which you can simply call 

```
ctest .
```

To run the official editorconfig tests located in `/tests` right now we pass all but one related to utf-8 which fails 
when run from `ctest .` but when I run it directly from the commandline it succeeds.

[EditorConfig C Core]: https://github.com/editorconfig/editorconfig-core
[EditorConfig Python Core]: https://github.com/editorconfig/editorconfig-core-py
[cmake]: http://www.cmake.org
