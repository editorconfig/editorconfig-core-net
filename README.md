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

    EditorConfig .NET Core Version 0.11.4-development

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

TODO document building

# Testing

TODO document testing

[EditorConfig C Core]: https://github.com/editorconfig/editorconfig-core
[EditorConfig Python Core]: https://github.com/editorconfig/editorconfig-core-py
[cmake]: http://www.cmake.org
