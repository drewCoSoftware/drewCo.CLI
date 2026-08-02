using System.Net.Http.Headers;

namespace drewCo.CLI;

// =========================================================================================================================
/// <summary>
/// Our command definition.  Really just a way to export types to our target language of choice...
/// </summary>
public class CommandDef
{
  public string Name { get; set; }
  public string HelpText { get; set; }
  public string? Alias { get; set; } = null;

  // Computed:
  public List<CommandOption> Options { get; set; } = new List<CommandOption>();


  // --------------------------------------------------------------------------------------------------------------------------
  public string GetCLIName()
  {
    string res = this.Alias != null ? this.Alias : this.Name.ToLower();
    return res;
  }


  // --------------------------------------------------------------------------------------------------------------------------
  public CommandOption? GetOptionByName(string optionNameOrAlias)
  {
    foreach (var item in Options)
    {
      if (item.Name == optionNameOrAlias) { return item; }
      if ("--" + item.Name == optionNameOrAlias) { return item; }
      if (item.HasAlias(optionNameOrAlias)) { return item; }
    }

    // No match!
    return null;
  }

}

// =========================================================================================================================
public class CommandOption
{
  public string Name { get; set; }
  public Type DataType { get; set; }
  public string DefaultValue { get; set; }
  public string HelpText { get; set; }

  public string? TypeHint { get; set; } = null;

  /// <summary>
  /// This is used during parsing.
  /// </summary>
  public bool IsValid { get; set; } = true;

  // Constraints ==========================================================
  // TODO: We can just use an instance of the constraints class instead?
  /// <summary>
  /// Is this option required?
  /// </summary>
  public bool IsRequired { get; set; } = false;

  /// <summary>
  /// Set of possible option values.
  /// </summary>
  public string[]? Options { get; set; }

  /// <summary>
  /// Any defined aliases.
  /// </summary>
  public string[]? Aliases { get; set; }

  // --------------------------------------------------------------------------------------------------------------------------
  internal bool HasAlias(string nextArg)
  {
    bool res = Aliases?.Contains(nextArg) ?? false;
    return res;
  }



  // --------------------------------------------------------------------------------------------------------------------------
  public string GetCLIName()
  {
    // OPTIONS:
    const bool INCLUDE_NON_ALIAS = false;

    string res = "--" + Name;
    if (Aliases != null)
    {
      res = string.Join(", ", Aliases);

      if (INCLUDE_NON_ALIAS)
      {
        res += $"(--{Name})";
      }
    }

    return res;
  }


}

// =========================================================================================================================
/// <summary>
/// Command options.
/// We don't want to go nuts with this stuff as we want to avoid creating some kind of meta language...
/// </summary>
public class CommandOptions
{
  /// <summary>
  /// Indicates that the command is required.
  /// </summary>
  public bool IsRequired { get; set; }

  /// <summary>
  /// Set of options, if any.
  /// </summary>
  public string[]? Options { get; set; }

  public string[]? Aliases { get; set; }

  public string? TypeHint { get; set; }
}

