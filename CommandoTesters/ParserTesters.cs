using Commando;
using Commando.Commands;
using System.Text;

namespace CommandoTesters
{

  // ==============================================================================================================================
  public class Tests
  {
    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that multiple defs can be generated / contained in a single TOML file.
    /// </summary>
    [Test]
    public void CanGenerateMultipleDefsFromSingleInput()
    {
      Assert.Fail("Please finish this test!");
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test shows that we can define + use aliases (i.e. -m, --my-thing type names) with our command options. 
    /// </summary>
    [Test]
    public void CanUseOptionNameAliases()
    {
      var def = ParseDefFromFile("ManyTypesDef.toml");
      var shortAlias = def.GetOptionByName("-m");
      var longAlias = def.GetOptionByName("--my-alias");

      Assert.IsNotNull(shortAlias);
      Assert.IsNotNull(longAlias);
      Assert.That(shortAlias, Is.SameAs(longAlias), "Both aliases should resolve to the same command!");
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case shows that we have support for / can deal with integers, datetimes, arrays, etc.
    /// </summary>
    [Test]
    public void ParserSupportsTOMLDataTypesAndArrays()
    {
      var def = ParseDefFromFile("ManyTypesDef.toml");

      // Make sure that the def + its defaults are represented correctly.
      // NOTE: Just add to this as you see fit.
      {
        // String Array Type
        var op = def.GetOptionByName("Animals");
        Assert.That(op.DataType.Name, Is.EqualTo("String[*]"));
      }
      {
        // Integer Array Type
        var op = def.GetOptionByName("PickThree");
        Assert.That(op.DataType.Name, Is.EqualTo("Int32[*]"));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This shows that we can parse out commands + their options directly from the command line.
    /// It shows that both the actual property name, or aliases / positional parameters can be used.
    /// </summary>
    [Test]
    public void CanParseCommandFromCommandLine()
    {

      bool cmdExecuted = false;

      var def1 = new Generate().Configure();
      var cli = new SimParser();
      cli.Register(new Generate(), (t) =>
      {
        return Generate.FromToml(t);
      },
      (g) =>
      {
        var cmd = g as Generate;
        if (cmd == null) { throw new InvalidOperationException("Incorrect command type!"); }

        // OK, now 
        cmdExecuted = true;
        return 0;
      });



      // Case: We have called the command, and we are including the required params via MemberName
      {
        const string TEST_PATH = "path-2";
        const string TEST_LANG = "cpp";

        cmdExecuted = false;
        var args = new[] { "generate", "--Path", TEST_PATH, "--TargetLanguage", TEST_LANG };
        int cliRes = cli.ParseCommandLine(args);

        Assert.That(cliRes, Is.EqualTo(0), "This command should have run!");
        Assert.IsTrue(cmdExecuted, "The command should have executed!");
      }

      // Case: We have called the command, but we are missing required paramters.
      {
        cmdExecuted = false;
        var args = new[] { "generate" };
        int cliRes = cli.ParseCommandLine(args);

        Assert.That(cliRes, Is.EqualTo(Parser.DEFAULT_ERROR_CODE), "This command should be invalid!");
        Assert.IsFalse(cmdExecuted, "The command should not have executed!");
      }

      // Case: Show that an invalid command name will fail.
      {
        cmdExecuted = false;
        var args = new[] { "badcommand" };

        int cliRes = cli.ParseCommandLine(args);

        Assert.That(cliRes, Is.EqualTo(Parser.DEFAULT_ERROR_CODE), "This command should be invalid!");
        Assert.IsFalse(cmdExecuted, "The command should not have executed!");
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This shows that we can get a validated instance of a command from a file on disk.
    /// </summary>
    [Test]
    public void CanParseCommandFromTomlFile()
    {
      bool cmdExecuted = false;


      var def1 = new Generate().Configure();
      var cli = new Parser();
      cli.Register(new Generate(), (t) =>
      {
        return Generate.FromToml(t);
      },
      (g) =>
      {
        var cmd = g as Generate;
        if (cmd == null) { throw new InvalidOperationException("Incorrect command type!"); }

        // OK, now 
        cmdExecuted = true;
        return 0;
      });


      string fromPath = Path.Combine("test-data", "GenerateCommand_1.toml");
      var args = new[] { fromPath };


      int res = cli.ParseCommandLine(args);
      Assert.That(res, Is.EqualTo(0));

      Assert.IsTrue(cmdExecuted, "The command should have been executed!");
    }


    // ----------------------------------------------------------------------    
    /// <summary>
    /// This is a test to show that we can derserialize data from a TOML file into a
    /// concrete type.  Usually one would use the code generator to create the type,
    /// and then they can read in its data from a file.
    /// NOTE: C++ implementation will look different.
    /// NOTE: Any implementation of a feature like this will be dependent on the library that is
    /// being used to parse the TOML files.
    /// </summary>
    [Test]
    public void CanDeserializeDataFromInputFile()
    {
      string fromPath = Path.Combine("test-data", "GenerateCommand_1.toml");

      var table = Helpers.FromFile(fromPath, out string commandName);
      var cmd = Generate.FromToml(table);

      Assert.IsNotNull(cmd, "The deserialized command should not be null!");

      // Make sure that our data is correct.....
      Assert.That(cmd.Path, Is.EqualTo("example.toml"));
      Assert.That(cmd.TargetLanguage, Is.EqualTo("cpp"));
      Assert.That(cmd.OutputPath, Is.EqualTo("the-output.cs"));

    }

    // ----------------------------------------------------------------------    
    [Test]
    public void CanGenerateCSharpCommandDefFromTOMLFile()
    {
      var def = ParseDefFromFile("GenerateCommandDef.toml");

      const string OUTPUT_PATH = "test-output.cs";
      var cg = new CodeGen();
      cg.OutputCSharp(def, OUTPUT_PATH, "Commando.Commands");

      string data = File.ReadAllText(OUTPUT_PATH, Encoding.UTF8);
      string refData = File.ReadAllText("..\\..\\..\\..\\Commando\\Commands\\Generate.cs", Encoding.UTF8);

      // Yes, this is correct.  We expect the output of the file from this test case
      // to be used in the actual program, verbatim.
      // NOTE: When changes are made to CodeGen.cs, it is possible to get minor whitespace differences, which is of course a huge pain in the ass.
      // Maybe someone else can come up with a more robust comparison technique?
      if (data != refData)
      {
        Assert.Fail("The generated data, and the reference data should match!");
      }

    }

    // ----------------------------------------------------------------------    
    [Test]
    public void CanParseCommandDefsFromTOMFile()
    {
      var def = ParseDefFromFile("GenerateCommandDef.toml");

      Assert.That(def.Options.Count, Is.EqualTo(3), "There should be three commands on this def.");

      var targetOp = def.Options[1];
      Assert.IsNotNull(targetOp.HelpText, "There should be some help text!");
      Assert.That(targetOp.Name, Is.EqualTo("TargetLanguage"));
      Assert.IsNotNull(targetOp.Options);
      Assert.That(targetOp.Options.Length, Is.EqualTo(3));

      Assert.IsNotNull(targetOp.DefaultValue, "There should be a default value!");
      Assert.That(targetOp.DefaultValue, Is.EqualTo("csharp"));

      var lastOp = def.Options[2]!;
      Assert.IsFalse(lastOp.IsRequired, $"The option for {lastOp.Name} shoul not be required!");
    }


    // --------------------------------------------------------------------------------------------------------------------------
    private static CommandDef ParseDefFromFile(string fileName)
    {
      string path = Path.Combine("test-data", fileName);
      var generator = new DefGenerator();
      var res = generator.ParseCommandDefsFromTOML(path);
      return res;
    }
  }
}