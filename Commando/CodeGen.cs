using dhll.CodeGen;
using drewCo.Tools;
using drewCo.Tools.Logging;

namespace Commando
{

  // ==================================================================================================
  public class CodeGen
  {

    // --------------------------------------------------------------------------------
    public void OutputCSharp(CommandDef fromDef, string toPath, string? useNamespace = null)
    {
      var file = new CodeFile();

      string nameProp = $"Name = \"{fromDef.Name}\"";
      var allProps = nameProp;
      if (!string.IsNullOrWhiteSpace(fromDef.HelpText))
      {
        allProps += $", HelpText = \"{fromDef.HelpText}\"";
      }

      file.WriteLine("// GENERATED CODE!  DO NOT EDIT BY HAND!");
      file.WriteLine("using Tommy;");
      file.NextLine();

      if (!string.IsNullOrWhiteSpace(useNamespace))
      {
        file.WriteLine($"namespace {useNamespace};");
        file.NextLine();
      }

      file.WriteLine($"public class {fromDef.Name} : {nameof(ICommand)}");
      file.OpenBlock(true);

      // This is where the different options would go......
      // NOTE: I don't think that we need to fiddle with extra attributes, etc. on the command defs.
      // We already have a way to define them, and their rules can be interpreted when the 
      // command line is first processed...  Having a concrete type is a nice to have.?
      // --> Trying to decide where to draw the line.... This is all stuff that I need to think about....

      foreach (var op in fromDef.Options)
      {
        string typeName = op.DataType.Name;
        string val = op.DefaultValue;

        if (op.DataType == typeof(string))
        {
          if (val == string.Empty) { val = "\"\""; } else if (val == null) { val = "null"; }
        }

        string opProps = $"Name = {op.Name}";
        if (op.IsRequired)
        {
          opProps += $", Required = true";
        }
        if (!string.IsNullOrWhiteSpace(op.HelpText))
        {
          opProps += $", HelpText = {op.HelpText}";
        }
        //file.WriteLine($"[CommandOption({opProps})]");
        if (op.DataType == typeof(string))
        {
          val = $"\"{val}\"";
        }
        file.WriteLine($"public {typeName} {op.Name} = {val};");
      }

      // Default constructor, for correct code elsewhere....
      file.NextLine();
      file.WriteLine($"public {fromDef.Name}() {{ }}");


      // A function that will deserialize this against a TomlTable instance.
      file.NextLine();
      file.WriteLine($"public static {fromDef.Name} FromToml(TomlTable table)");
      file.OpenBlock(true);

      file.WriteLine($"var res = new {fromDef.Name}();");

      foreach (var item in fromDef.Options)
      {
        string conv = $".{AsType(item.DataType)}.Value;";
        file.WriteLine($"res.{item.Name} = table[\"{item.Name}\"]{conv}");
      }
      file.WriteLine("return res;");
      file.CloseBlock(1);

      EmitValidationFunction(fromDef, file);

      EmitConfigureFunction(fromDef, file);

      file.CloseBlock(1);

      file.Save(toPath);

      Log.Info($"Wrote C# code to file: {toPath}");
    }

    // --------------------------------------------------------------------------------
    /// <summary>
    /// Configuration function so that we can setup the command for use in your program.
    /// </summary>
    private void EmitConfigureFunction(CommandDef def, CodeFile file)
    {
      file.NextLine();

      file.WriteLine($"public CommandDef Configure()");
      file.OpenBlock(true);

      file.WriteLine($"var res = new CommandDef();");
      file.WriteLine($"res.Name = \"{def.Name}\";");
      foreach (var op in def.Options)
      {
        file.NextLine();
        var opName = $"{StringTools.LowerFirst(op.Name)}Option";
        file.WriteLine($"var {opName} = new CommandOption();");
        file.WriteLine($"{opName}.Name = \"{op.Name}\";");

        file.WriteLine($"{opName}.IsRequired = {(op.IsRequired ? "true" : "false")};");

        string useOpsVal = "null";
        if (op.Options != null)
        {
          useOpsVal = $"new[] {{ " + string.Join(", ", from x in op.Options select $"\"{x}\"") + " }";
        }
        file.WriteLine($"{opName}.Options = {useOpsVal};");
        file.WriteLine($"res.Options.Add({opName});");
      }

      file.NextLine();
      file.WriteLine("return res;");

      file.CloseBlock(1);
    }

    // --------------------------------------------------------------------------------
    private void EmitValidationFunction(CommandDef def, CodeFile file)
    {
      file.NextLine();
      file.WriteLine($"public {nameof(CommandValidationResult)} Validate()");
      file.OpenBlock(true);

      file.WriteLine($"var res = new {nameof(CommandValidationResult)}();");

      foreach (var item in def.Options)
      {
        // Check the constraints.....
        if (item.IsRequired)
        {
          if (item.DataType == typeof(string))
          {
            file.WriteLine($"if (string.IsNullOrWhiteSpace({item.Name}))");
            file.OpenBlock(true);

            file.WriteLine($"res.AddError(\"Option: '{item.Name}' is required!\");");

            file.CloseBlock(1);
          }
          else
          {
            // Other types should be OK?
            if (item.DefaultValue == null)
            {
              throw new NotImplementedException("not there yet...");
            }
          }
        }

        // Make sure that the supplied value exists in the list of possible options.
        if (item.Options != null)
        {
        }
      }

      file.WriteLine("return res;");
      file.CloseBlock(1);
    }

    // --------------------------------------------------------------------------------
    /// <summary>
    /// Bandaid function to get over odd Tommy API...
    /// </summary>
    public string AsType(Type fromType)
    {
      if (fromType == typeof(string))
      {
        return "AsString";
      }
      else if (fromType == typeof(int))
      {
        return "AsInteger";
      }

      // Other cases here.....
      else
      {
        throw new ArgumentOutOfRangeException($"Unsupported type: {fromType}!");
      }
    }

    //// --------------------------------------------------------------------------------
    //public void OutputCSharp(CommandDef fromDef, Stream toStream)
    //{
    //    var sb = new StringBuilder();


    //    var file = new CodeFile();
    //    file.WriteLine($"[Command(\"{fromDef.Name}\")]");

    //    file.WriteLine($"public class {fromDef.Name}");
    //    file.OpenBlock();
    //    file.WriteLine("// wow!");
    //    file.CloseBlock();

    //    file.Save(

    //    string res = sb.ToString();
    //    var data = Encoding.UTF8.GetBytes(res);
    //    toStream.Write(data, 0, data.Length);   
    //}

  }
}
