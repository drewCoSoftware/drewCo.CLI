# drewCo.CLI
Trying to make a tool where I can get the best of both fully formed config files + command line depending on my current need.  Should be simple, and without a lot of magic features.






## Configure Your CLI Type
You can use an existing TOML file + generate tool, or your can configure them manually in your application.

### Generated Configuration
The easiest way to define and configure your command types is to use the generation tool, and an existing TOML file. *GenerateCommandDef.toml* was used to generate the type and configuration for drewCo.CLI.Tools.

Each command is a TOML table:
```
# This is help text for the command.
[ExampleCommand]

# This is a number
MyNumber = 123

# This is a string
MyString = "abc"

```

#### Aliases:
Command definitions and options can be aliased for more convenient CLI interactions:

For command definitions, use a single string value.
For command options, you can provide long (--) and short(-) style aliases.
```
# @ALIAS my-command
[Command]

# @ALIAS [--alias, -a]
Option = x
```

#### Type Hints:
Non default types can be used for each option.  This is a convenient way to make sure that the generated code contains the exact data type that you want.  This feature was originally added to help with integer / float types and isn't intended for anything else...
```
# @TYPE : <typedef>

# typedef is of the form:
<type (i/u/f)><size (8/16/32/64)> []?
where:
i = signed integer
u = unsigned integer
f = floating floating point

#Examples:
u64   -> unsinged 64 bit integer.
f64   -> double precision floating point.
i8    -> signed 8-bit integer.
u16[] -> array of unsigned 16 bit unsigned integer

```
Obviously, the generator tool will fail if you try to use annotations on incompatible types.


### Manual Configuration
Manual configuration is possible, but not recommended.  The following excerpt is from *Generate.cs*.  The entire file was created using the generate tool, but you can see how you might go about creating / configuring the command defs manually.

```
  public CommandDef Configure()
  {
    // Create the command definition + give it some basic properties.
    var res = new CommandDef();
    res.Name = "Generate";
    res.HelpText = "Generate data types for your target language from a TOML file.";

    // Now add all of the options that you wish your command to have:
    var pathOption = new CommandOption();
    pathOption.Name = "Path";
    pathOption.HelpText = "Path to input TOML file.";
    pathOption.DataType = typeof(String);
    pathOption.IsRequired = true;
    pathOption.Aliases = new[] { "--path" };
    pathOption.Options = null;
    res.Options.Add(pathOption);

    var targetLanguageOption = new CommandOption();
    targetLanguageOption.Name = "TargetLanguage";
    targetLanguageOption.HelpText = "The target output language.";
    targetLanguageOption.DataType = typeof(String);
    targetLanguageOption.IsRequired = true;
    targetLanguageOption.Aliases = new[] { "--lang" };
    targetLanguageOption.Options = new[] { "csharp", "python", "cpp" };
    res.Options.Add(targetLanguageOption);

    var outputPathOption = new CommandOption();
    outputPathOption.Name = "OutputPath";
    outputPathOption.HelpText = "The path where the output file will go.";
    outputPathOption.DataType = typeof(String);
    outputPathOption.IsRequired = true;
    outputPathOption.Aliases = new[] { "--output", "-o" };
    outputPathOption.Options = null;
    res.Options.Add(outputPathOption);

    return res;
  }

```