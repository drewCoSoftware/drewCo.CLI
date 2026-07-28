using System.ComponentModel.Design;
using System.Reflection;
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

    private HelpWriter HelpWriter = new HelpWriter();

    // --------------------------------------------------------------------------------------------------------------------------
    public Parser(int errCode_ = DEFAULT_ERROR_CODE)
    {
      ErrorCode = errCode_;
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
    private void PrintHelp(string? useCommand = null)
    {
      HelpWriter.Init();
      HelpWriter.WriteNameAndversion();

      // TODO: Write copyright and other info, or is this a config setting?
      // HelpWriter.WriteMessage("TODO: Copyright data");
      HelpWriter.WriteMessage();

      HelpWriter.SetIndent(2);


      var toWrite = new List<(string,string)>();


      if (useCommand != null)
      {
        // Display the help for this specific command.
        var match = AllCommands[useCommand];

        foreach (var item in match.Def.Options)
        {
          toWrite.Add((item.Name, item.HelpText));
        }
        toWrite.Add((HELP_COMMAND, "Display this help message."));
      }
      else
      {
        // Display help for all commands...
        foreach (var item in AllCommands.Values)
        {
          toWrite.Add((item.Def.Name, item.Def.HelpText));
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
    public int ParseCommandLine(string[] args)
    {
      bool printHelp = false;

      if (args.Length == 0 || args[0] == HELP_COMMAND)
      {
        // Print Help.
        PrintHelp();
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
        Console.WriteLine($"Unknown command: {args[0]}!");
        PrintHelp();
        return ErrorCode;
      }

      printHelp = ParseOptionValues(args, printHelp, table, entry);

      // Now we have a known command, a table, and a def.
      // Let's create an instance of the data + validate it:
      ICommand cmd = entry.Hydrate(table);
      var vr = cmd.Validate();
      if (vr.Errors.Count > 0)
      {
        Console.WriteLine("There are validation errors!");
        //Console.WriteLine("Print errors!");
        //Console.WriteLine("Print help for this command!");
        foreach (var item in vr.Errors)
        {
          Console.WriteLine(item.Message);
        }
        printHelp = true;
      }

      if (printHelp)
      {
        PrintHelp(useCommand);
        return ErrorCode;
      }


      // The command is valid, so now we can execute it....
      int res = entry.OnCommand(cmd);
      return res;


    }

    // --------------------------------------------------------------------------------------------------------------------------
    private bool ParseOptionValues(string[] args, bool printHelp, TomlTable table, DefEntry entry)
    {
      if (args.Length > 1)
      {
        // Options + values are paired off.
        // Boolean options can work like a flag, and defaults to 'true' if no argument is given (can be true/false)
        int max = args.Length;

        var errors = new List<string>();

        var optionsAndValues = new List<(CommandOption, string)>();

        for (int i = 1; i < max; i++)
        {
          // I think we can just read them off one by one:
          string nextArg = args[i];
          if (nextArg == HELP_COMMAND)
          {
            printHelp = true;
            continue;
          }

          // Find a match for the next arg.  If none, then we have an error...
          var op = entry.Def.GetOptionByName(nextArg);

          if (op == null)
          {
            errors.Add($"Invalid argument: {nextArg}!");

            // We have to check if the next one is a value?
            string? peekedArg = PeekNext(args, i + 1);
            if (peekedArg != null)
            {
              // Is this an option?
              var nextOp = entry.Def.GetOptionByName(peekedArg);
              if (nextOp == null)
              {
                // OK, it isn't an option, so it must be a value.
                optionsAndValues.Add((new CommandOption()
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
                optionsAndValues.Add((op, peekedVal));
                i++;
              }
              else
              {
                // This is another command option.
                // We should have a value unless this is a boolean flag.
                if (op.DataType == typeof(bool))
                {
                  optionsAndValues.Add((op, "true"));
                }
                else
                {
                  errors.Add($"There is no value for option: '{nextArg}'!");
                }
              }
            }

          }

        }

        // Here we will have collected all of the parameters + their values.
        // We will set those values on the table:
        int len = optionsAndValues.Count;
        for (int i = 0; i < len; i++)
        {
          var item = optionsAndValues[i];
          table.AddIfMissing(item.Item1.Name, item.Item1.DataType);
          table.SetValue(item.Item1.Name, item.Item2);
        }
      }

      return printHelp;
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
