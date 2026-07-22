// GENERATED CODE!  DO NOT EDIT BY HAND!
using Tommy;

namespace Commando.Commands;

public class Generate : ICommand
{
  public String Path = "input.toml";
  public String TargetLanguage = "csharp";
  public String OutputPath = ".";

  public Generate() { }

  public static Generate FromToml(TomlTable table)
  {
    var res = new Generate();
    res.Path = table["Path"].AsString.Value;
    res.TargetLanguage = table["TargetLanguage"].AsString.Value;
    res.OutputPath = table["OutputPath"].AsString.Value;
    return res;
  }

  public CommandValidationResult Validate()
  {
    var res = new CommandValidationResult();
    if (string.IsNullOrWhiteSpace(Path))
    {
      res.AddError("Option: 'Path' is required!");
    }
    if (string.IsNullOrWhiteSpace(TargetLanguage))
    {
      res.AddError("Option: 'TargetLanguage' is required!");
    }
    return res;
  }

  public CommandDef Configure()
  {
    var res = new CommandDef();
    res.Name = "Generate";

    var pathOption = new CommandOption();
    pathOption.Name = "Path";
    pathOption.IsRequired = true;
    pathOption.Options = null;
    res.Options.Add(pathOption);

    var targetLanguageOption = new CommandOption();
    targetLanguageOption.Name = "TargetLanguage";
    targetLanguageOption.IsRequired = true;
    targetLanguageOption.Options = new[] { "csharp", "python", "cpp" };
    res.Options.Add(targetLanguageOption);

    var outputPathOption = new CommandOption();
    outputPathOption.Name = "OutputPath";
    outputPathOption.IsRequired = false;
    outputPathOption.Options = null;
    res.Options.Add(outputPathOption);

    return res;
  }
}
