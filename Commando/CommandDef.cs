namespace Commando;

// =========================================================================================================================
/// <summary>
/// Our command definition.  Really just a way to export types to our target language of choice...
/// </summary>
public class CommandDef
{
  public string Name { get; set; }
  public string HelpText { get; set; }

  // Computed:
  public List<CommandOption> Options { get; set; } = new List<CommandOption>();


  // --------------------------------------------------------------------------------------------------------------------------
  public CommandOption? GetOptionByName(string nextArg)
  {
    foreach (var item in Options)
    {
      if ("--" + item.Name == nextArg) { return item; }
      if (item.HasShortcut(nextArg)) { return item; }
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

  /// <summary>
  /// This is used during parsing.
  /// </summary>
  public bool IsValid { get; set; } = true;

  // Constraints ==========================================================
  /// <summary>
  /// Is this option required?
  /// </summary>
  public bool IsRequired { get; set; } = false;

  /// <summary>
  /// Set of possible option values.
  /// </summary>
  public string[]? Options { get; set; }


  // --------------------------------------------------------------------------------------------------------------------------
  internal bool HasShortcut(string nextArg)
  {
    // TEMP: 
    return false;
  }

}

// =========================================================================================================================
/// <summary>
/// Command constraints / options.
/// We don't want to go nuts with this stuff as we want to avoid creating some kind of meta language...
/// </summary>
internal class Constraints
{
  /// <summary>
  /// Indicates that the command is required.
  /// </summary>
  public bool IsRequired { get; set; }

  /// <summary>
  /// Set of options, if any.
  /// </summary>
  public string[]? Options { get; set; }
}

