using drewCo.CLI;
using System.Net.WebSockets;
using System.Security.Cryptography;
using Tommy;

namespace drewCo.CLI
{
  // ==============================================================================================================================
  public class Parser
  {
    public const int DEFAULT_ERROR_CODE = -1;
    private const string HELP_COMMAND = "--help";
    private const string VERSION_COMMAND = "--version";

    private class DefEntry
    {
      public CommandDef Def { get; set; } = default!;
      public Func<TomlTable, ICommand> Hydrate = default!;
      public Func<object, int> OnCommand { get; set; } = default!;
    }

    private Dictionary<string, DefEntry> AllCommands = new Dictionary<string, DefEntry>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The error code that will be reported if no commands run, or if help is printed.
    /// </summary>
    public int ErrorCode { get; private set; } = DEFAULT_ERROR_CODE;

    // NOTE: HelpWriter and the list of messages might end up being merged into a single interface at some point....
    private HelpWriter HelpWriter = new HelpWriter();
    private List<string> Messages = new List<string>();

    // --------------------------------------------------------------------------------------------------------------------------
    public Parser(int errCode_ = DEFAULT_ERROR_CODE)
    {
      ErrorCode = errCode_;
      Messages = new List<string>();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// NOTE: Command names are case-insensitive!
    /// </summary>
    public void Register(ICommand command, Func<TomlTable, ICommand> hydrate, Func<object, int> onCommand)
    {
      var def = command.Configure();
      AllCommands.Add(def.Name, new DefEntry()
      {
        Def = def,
        Hydrate = hydrate,
        OnCommand = onCommand
      });
    }

    // --------------------------------------------------------------------------------------------------------------------------
    // TODO: Shared location or otherwise.....
    private void PrintVersion()
    {
      HelpWriter.Init();
      HelpWriter.WriteVersion();
    }


    // --------------------------------------------------------------------------------------------------------------------------
    private void AddMessage(string message)
    {
      Messages.Add(message);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private void PrintHelpAndMessages(string? helpCommand, OptionsParseResult? parseResult)
    {

      if (parseResult != null)
      {
        foreach (var item in parseResult.Errors)
        {
          AddMessage(item);
        }
      }


      HelpWriter.Init();
      HelpWriter.WriteNameAndversion();
      HelpWriter.WriteMessage();

      foreach (var m in Messages)
      {
        HelpWriter.WriteMessage(m);
      }

      // TODO: Write copyright and other info, or is this a config setting?
      // HelpWriter.WriteMessage("TODO: Copyright data");
      HelpWriter.WriteMessage();

      HelpWriter.SetIndent(2);


      var toWrite = new List<(string, string)>();


      if (helpCommand != null)
      {
        // Display the help for this specific command.
        var match = AllCommands[helpCommand];

        foreach (var item in match.Def.Options)
        {
          if (parseResult?.HasValidOption(item) ?? true) { continue; }

          toWrite.Add((GetCLIName(item), item.HelpText));
        }
        toWrite.Add((HELP_COMMAND, "Display this help message."));
      }
      else
      {
        // Display help for all commands...
        foreach (var item in AllCommands.Values)
        {
          toWrite.Add((GetCLIName(item.Def), item.Def.HelpText));
        }
        toWrite.Add((HELP_COMMAND, "Display help information for a specific command."));
        toWrite.Add((VERSION_COMMAND, "Display version information."));
      }

      // Max col1 width for prettiest printing.
      const int INDENT_SIZE = 2;
      HelpWriter.SetIndent(INDENT_SIZE);

      int maxWidth = HelpWriter.Col1Width;
      foreach (var item in toWrite)
      {
        maxWidth = Math.Max(item.Item1.Length + INDENT_SIZE, maxWidth);
      }
      HelpWriter.SetCol1Size(maxWidth);

      foreach (var item in toWrite)
      {
        HelpWriter.WriteMessage(item.Item1, item.Item2);
        HelpWriter.WriteMessage();
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    // SHARE:
    public static string GetCLIName(CommandDef def)
    {
      string res = def.Name.ToLower();
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    // SHARE:
    /// <summary>
    /// Get the name / aliases for the command option.
    /// </summary>
    public static string GetCLIName(CommandOption option)
    {
      // OPTIONS:
      const bool INCLUDE_NON_ALIAS = false;

      string res = "--" + option.Name;
      if (option.Aliases != null)
      {
        res = string.Join(", ", option.Aliases);

        if (INCLUDE_NON_ALIAS)
        {
          res += $"(--{option.Name})";
        }
      }

      return res;

    }

    // --------------------------------------------------------------------------------------------------------------------------
    public int ParseCommandLine(string[] args)
    {
      bool printHelp = false;

      if (args.Length == 0 || args[0] == HELP_COMMAND)
      {
        // Print Help.
        PrintHelpAndMessages(null, null);
        return ErrorCode;
      }

      if (args.Contains(VERSION_COMMAND))
      {
        PrintVersion();
        return ErrorCode;
      }


      string useCommand = null!;
      TomlTable table = null!;
      if (args[0].EndsWith(".toml"))
      {
        // This is a TOML file with the command definition built in!
        // How do we get the concrete type that is associated with this def.....
        // Think about how we would do this in a C++ context.....
        // We simply want to hydrate the TOML table from the file:
        string path = args[0];
        if (!File.Exists(path))
        {
          throw new InvalidOperationException($"The command file at path: {path} does not exist!");
        }

        table = Helpers.FromFile(path, out string cmdName);
        useCommand = cmdName;
      }
      else
      {
        useCommand = args[0].ToLower();
        table = new TomlTable();
      }

      if (!AllCommands.TryGetValue(useCommand, out var entry))
      {
        // TODO: Maybe some different text here depending on if we used a file or not....
        // TODO: We need to make note of the errors so that they can be written in the proper order (after the program info, mostly)
        AddMessage($"Unknown command: {args[0]}!");
        PrintHelpAndMessages(null, null);
        return ErrorCode;
      }

      var parseResult = ParseOptionValues(args, table, entry);
      if (parseResult.Errors.Count > 0)
      {
        PrintHelpAndMessages(useCommand, parseResult);
        return ErrorCode;
      }

      // Now we have a known command, a table, and a def.
      // Let's create an instance of the data + validate it:
      ICommand cmd = entry.Hydrate(table);
      var vr = cmd.Validate();
      if (vr.Errors.Count > 0)
      {
        AddMessage("There are validation errors!");
        foreach (var item in vr.Errors)
        {
          AddMessage(item.Message);
        }
        printHelp = true;
      }

      if (printHelp)
      {
        PrintHelpAndMessages(useCommand, null);
        return ErrorCode;
      }


      // The command is valid, so now we can execute it....
      int res = entry.OnCommand(cmd);
      return res;


    }


    // --------------------------------------------------------------------------------------------------------------------------
    private OptionsParseResult ParseOptionValues(string[] args, TomlTable table, DefEntry entry)
    {
      var res = new OptionsParseResult();

      if (args.Length > 1)
      {
        // Options + values are paired off.
        // Boolean options can work like a flag, and defaults to 'true' if no argument is given (can be true/false)
        int max = args.Length;


        for (int i = 1; i < max; i++)
        {
          // I think we can just read them off one by one:
          string nextArg = args[i];
          if (nextArg == HELP_COMMAND)
          {
            continue;
          }

          // Find a match for the next arg.  If none, then we have an error...
          var op = entry.Def.GetOptionByName(nextArg);

          if (op == null)
          {
            res.Errors.Add($"Invalid argument: {nextArg}!");

            // We have to check if the next one is a value?
            string? peekedArg = PeekNext(args, i + 1);
            if (peekedArg != null)
            {
              // Is this an option?
              var nextOp = entry.Def.GetOptionByName(peekedArg);
              if (nextOp == null)
              {
                // OK, it isn't an option, so it must be a value.
                res.OptionsAndValues.Add((new CommandOption()
                {
                  Name = nextArg,
                  IsValid = false,
                }, peekedArg)!);
                i++;
              }
              else
              {
                // NOOP:
                // It is an option, so we will pick it up on the next cycle.
              }
            }
          }
          else
          {
            // We have a real option.  Let's grab the value for it.
            string? peekedVal = PeekNext(args, i + 1);
            if (peekedVal != null)
            {
              var nextOp = entry.Def.GetOptionByName(peekedVal);
              if (nextOp == null)
              {
                // We have a value!
                res.OptionsAndValues.Add((op, peekedVal));
                i++;
              }
              else
              {
                // This is another command option.
                // We should have a value unless this is a boolean flag.
                if (op.DataType == typeof(bool))
                {
                  res.OptionsAndValues.Add((op, "true"));
                }
                else
                {
                  res.Errors.Add($"There is no value for option: '{nextArg}'!");
                }
              }
            }

          }

        }

        // Here we will have collected all of the parameters + their values.
        // We will set those values on the table:
        int len = res.OptionsAndValues.Count;
        for (int i = 0; i < len; i++)
        {
          var item = res.OptionsAndValues[i];
          if (item.Item1.IsValid)
          {
            table.AddIfMissing(item.Item1.Name, item.Item1.DataType);
            table.SetValue(item.Item1.Name, item.Item2);
          }
        }
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private string? PeekNext(string[] args, int index)
    {
      if (index >= args.Length)
      {
        return null;
      }
      return args[index];
    }

  }



}

// ==============================================================================================================================
internal class OptionsParseResult
{
  public List<string> Errors { get; set; } = new List<string>();
  public List<(CommandOption, string)> OptionsAndValues { get; set; } = new List<(CommandOption, string)>();

  // --------------------------------------------------------------------------------------------------------------------------
  internal bool HasValidOption(CommandOption item)
  {
    var match = (from x in OptionsAndValues where x.Item1.Name == item.Name select x.Item1).SingleOrDefault();
    if (match != null)
    {
      return match.IsValid;
    }
    return false;
  }
}
