using Commando.Commands;
using drewCo.Tools.Logging;
using System.Runtime.CompilerServices;
using Tommy;

namespace Commando
{
  internal class Program
  {
    // ------------------------------------------------------------------------------------------
    static void Main(string[] args)
    {
      // This is how we go about setting up the commands.
      var def1 = new Generate().Configure();
      // var def2 = 


      // We need to register the defs with the system.
      // Then we need to interpret the command line args.





      // Console.WriteLine("Hello, World!");
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
    public void Register<T>(CommandDef def, Func<TomlTable, ICommand> hydrate, Func<object, int> onCommand)
    {
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
