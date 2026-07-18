namespace Commando;

// =========================================================================================================================
/// <summary>
/// Our command definition.  Really just a way to export types to our target language of choice...
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CommandDef : Attribute
{
    public string Name { get; set; }
    public string HelpText { get; set; }

    // Computed:
    public List<CommandOption> Options { get; set; } = new List<CommandOption>();
}

// =========================================================================================================================
[AttributeUsage(AttributeTargets.Property)]
public class CommandOption : Attribute
{
    public string Name { get; set; }
    public Type DataType { get; set; }
    public string DefaultValue { get; set; }
    public string HelpText { get; set; }


    // Constraints ==========================================================
    /// <summary>
    /// Is this option required?
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Set of possible option values.
    /// </summary>
    public string[]? Options { get; set; }

    // =======================================================================
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

