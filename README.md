A subcommand enabled option parsing library for .NET
=============
 
 
This library is inspired by NDesk::Options.

Features include,

  - At development time,
    - Hierarchical subcommands are supported.
    - Returns options and subcommands via callbacks.
    - Option value will be automatically casted to the type of callback's parameter.
    - Define Options using collection initializer.
    - Can be referenced as an assembly or as a source file.
  - At runtime,
    - Use `--` for option name and `-` for option shortcut.
    - Multiple shortcuts can be aggregate together.
    - Separate option and option value by space or equal mark.
    - Stop parsing when meets a standalone `--`
