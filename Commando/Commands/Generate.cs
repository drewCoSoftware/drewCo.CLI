// GENERATED CODE!  DO NOT EDIT BY HAND!
using Commando;
using Tommy;

namespace Commando.Commands;

public class Generate
{
  public String Path = "input.toml";
  public String TargetLanguage = "csharp";
  public String OuputPath = ".";

  public Generate() { }

  public static Generate FromTable(TomlTable table)
  {
    var res = new Generate();
    res.Path = table["Path"].AsString.Value;
    res.TargetLanguage = table["TargetLanguage"].AsString.Value;
    res.OuputPath = table["OuputPath"].AsString.Value;
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

    var ouputPathOption = new CommandOption();
    ouputPathOption.Name = "OuputPath";
    ouputPathOption.IsRequired = false;
    ouputPathOption.Options = null;
    res.Options.Add(ouputPathOption);

    return res;
  }
}
