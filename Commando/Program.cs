using Antlr4.Runtime;
using Commando.Commands;
using drewCo.Tools;
using drewCo.Tools.Logging;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using Tommy;

namespace Commando
{
  // ==============================================================================================================================
  internal class Program
  {
    // ------------------------------------------------------------------------------------------
    static int Main(string[] args)
    {
      // This is how we go about setting up the commands.
      var def1 = new Generate().Configure();

      // TODO: We can come up with a more "C#" way to do this (i.e. generics) once we get a similar C++ library up and running.
      var p = new Parser();
      p.Register(new Generate(), t => Generate.FromToml(t), GenerateCode);


      int res = p.ParseCommandLine(args);


      return res;
    }


    // ------------------------------------------------------------------------------------------
    private static int GenerateCode(object args)
    {
      var g = args as Generate;
      if (g == null)
      {
        throw new InvalidCastException($"Could not cast object to instance of: {typeof(Generate)}!");
      }

      var generator = new DefGenerator();
      var def = generator.ParseCommandDefsFromTOML(g.Path);
      var cg = new CodeGen();

      switch (g.TargetLanguage)
      {
        case "csharp":
          string outDir = Path.GetDirectoryName(g.Path);
          FileTools.CreateDirectory(outDir);

          cg.OutputCSharp(def, g.OutputPath);

          Log.Info($"Code file was saved to: {g.OutputPath}");

          break;

        default:
          // NOTE: This condition should come up during command validation!
          throw new InvalidOperationException($"Unsupported target language: {g.TargetLanguage}");
      }


      return 0;
    }

  }

  // ==============================================================================================================================
  public class Parser
  {
    public const int INVALID_COMMAND = -1;
    public const int HELP_COMMAND = -2;

    private class DefEntry
    {
      public CommandDef Def { get; set; } = default!;
      public Func<TomlTable, ICommand> Hydrate = default!;
      public Func<object, int> OnCommand { get; set; } = default!;
    }

    private Dictionary<string, DefEntry> AllCommands = new Dictionary<string, DefEntry>(StringComparer.OrdinalIgnoreCase);

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
    private void PrintHelp()
    {
      if (Console.IsOutputRedirected) { return; }
      if (Console.BufferWidth == 0) { return; }

      // Spit out the list of commands and their help text....
      Console.WriteLine("TODO: Print the help content!");
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public int ParseCommandLine(string[] args)
    {
      if (args.Length == 0)
      {
        // Print Help.
        PrintHelp();
        return INVALID_COMMAND;
      }
      else if (args[0] == "--help")
      {
        PrintHelp();
        return HELP_COMMAND;
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
        return INVALID_COMMAND;
      }

      //// Create the table from the command definition, as needed.
      //if (table == null) { 
      //  table = new TomlTable();
      //  foreach (var op in entry.Def.Options)
      //  {
      //    if (op.IsRequired) { 
      //      table.AddNode(op.Name, op.data
      //    }
      //  }
      //}

      // TODO: Add support for override values:
      if (args.Length > 1)
      {

        // Options + values are paired off.
        // Boolean options can work like a flag, and defaults to 'true' if no argument is given (can be true/false)
        int max = args.Length;
        bool printHelp = false;

        var errors = new List<string>();

        var optionsAndValues = new List<(CommandOption, string)>();

        for (int i = 1; i < max; i++)
        {
          // I think we can just read them off one by one:
          string nextArg = args[i];
          if (nextArg == "--help")
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
            string? peekedVal = PeekNext(args,i+1);
            if ( peekedVal != null )
            {
              var nextOp = entry.Def.GetOptionByName(peekedVal);
              if (nextOp == null) { 
                // We have a value!
                optionsAndValues.Add((op, peekedVal));
                i++;
              }
              else { 
                // This is another command option.
                // We should have a value unless this is a boolean flag.
                if (op.DataType == typeof(bool)) { 
                  optionsAndValues.Add((op, "true"));
                }
                else { 
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


        //// TODO: look for '--help' in all other parameters, and print it for this command, ignoring all other arguments / params if we find it.
        //throw new NotSupportedException("value overrides are not supported at this time!");
        //return INVALID_COMMAND;
      }

      // Now we have a known command, a table, and a def.
      // Let's create an instance of the data + validate it:
      ICommand cmd = entry.Hydrate(table);
      var vr = cmd.Validate();
      if (vr.Errors.Count > 0)
      {
        Console.WriteLine("There are validation errors!");
        Console.WriteLine("Print errors!");
        Console.WriteLine("Print help for this command!");
        return INVALID_COMMAND;
      }

      // The command is valid, so now we can execute it....
      int res = entry.OnCommand(cmd);
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

    private void PrintHelpForCommand(string useCommand)
    {
      throw new NotImplementedException();
    }
  }



}
