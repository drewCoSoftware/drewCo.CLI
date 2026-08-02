// GENERATED CODE!  DO NOT EDIT BY HAND!
using Tommy;
using drewCo.CLI;

namespace drewCo.CLI.Commands;

public class Generate : ICommand
{
  public String Path = "input.toml";
  public String TargetLanguage = "csharp";
  public String OutputPath = ".";

  public Generate() { }

  public static Generate FromToml(TomlTable table)
  {
    var res = new Generate();
    res.Path = table.GetString("Path");
    res.TargetLanguage = table.GetString("TargetLanguage");
    res.OutputPath = table.GetString("OutputPath");
    return res;
  }

  public CommandValidationResult Validate()
  {
    var res = new CommandValidationResult();
    if (string.IsNullOrWhiteSpace(Path))
    {
      res.AddError("Option: 'Path' (--path) is required!");
    }
    if (string.IsNullOrWhiteSpace(TargetLanguage))
    {
      res.AddError("Option: 'TargetLanguage' (--lang) is required!");
    }
    if (string.IsNullOrWhiteSpace(OutputPath))
    {
      res.AddError("Option: 'OutputPath' (--output, -o) is required!");
    }
    return res;
  }

  public CommandDef Configure()
  {
    var res = new CommandDef();
    res.Name = "Generate";
    res.HelpText = "Generate data types for your target language from a TOML file.";

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
}

