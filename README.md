# EditorConfig .NET Core

The EditorConfig .NET coreprovides the same functionality as the
[EditorConfig C Core][] and [EditorConfig Python Core][].

## Installation

TODO: push to nuget/chocolatey

## Usage

Usage as a library:


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

# Testing

We have several NUnit tests that you can run from visual studio or the build scripts. 

If you want to run the official editorconfig tests you'll need to install CMAKE and call

```
cmake .
```

in the rot once.

Afterwhich you can simply call 

```
ctest .
```

To run the official editorconfig tests located in `/tests` right now we pass all but one related to utf-8 which fails 
when run from `ctest .` but when i run it directly from the commandline it succeeds.

[EditorConfig C Core]: https://github.com/editorconfig/editorconfig-core
[EditorConfig Python Core]: https://github.com/editorconfig/editorconfig-core-py
[cmake]: http://www.cmake.org
