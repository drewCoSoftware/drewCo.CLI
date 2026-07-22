using Antlr4.Runtime;
using Commando.Commands;
using drewCo.Tools;
using drewCo.Tools.Logging;
using System.Runtime.CompilerServices;
using Tommy;

namespace Commando
{
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
        throw new InvalidOperationException("Commands directly from cmdline args is not supported!");
      }

      if (!AllCommands.TryGetValue(useCommand, out var entry))
      {
        // TODO: Maybe some different text here depending on if we used a file or not....
        Console.WriteLine($"Unknown command: {args[0]}!");
        PrintHelp();
        return INVALID_COMMAND;
      }

      // TODO: Add support for override values:
      if (args.Length > 1)
      {
        // TODO: look for '--help' in all other parameters, and print it for this command, ignoring all other arguments / params if we find it.
        throw new NotSupportedException("value overrides are not supported at this time!");
        return INVALID_COMMAND;
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

      //// Select the command:
      //var cmd = args[0].ToLower();
      //if (AllCommands.TryGetValue(cmd, out DefEntry? entry))
      //{

      //}
      //else
      //{
      //  Console.WriteLine($"Unknown command: {args[0]}!");
      //  PrintHelp();
      //  return;
      //}

      // OK, we have the command, now let 

    }

  }

}
