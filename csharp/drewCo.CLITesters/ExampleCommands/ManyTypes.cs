// GENERATED CODE!  DO NOT EDIT BY HAND!
using Tommy;
using drewCo.CLI;

public class ManyTypes : ICommand
{
  public Boolean IsTest = false;
  public String Name = "Name";
  public Int32 Number = 123;
  public Single Pie = 3.1415926f;
  public String[] Animals = [ "Dog", "Cat", "Monkey" ];
  public Int32[] PickThree = [ 42, 27, 999 ];

  public ManyTypes() { }

  public static ManyTypes FromToml(TomlTable table)
  {
    var res = new ManyTypes();
    res.IsTest = table.GetBool("IsTest", false);
    res.Name = table.GetString("Name", "Name");
    res.Number = table.GetInt("Number", 123);
    res.Pie = table.GetSingle("Pie", 3.1415926f);
    res.Animals = table.GetStringArray("Animals", [ "Dog", "Cat", "Monkey" ]);
    res.PickThree = table.GetIntArray("PickThree", [ 42, 27, 999 ]);
    return res;
  }

  public CommandValidationResult Validate()
  {
    var res = new CommandValidationResult();
    return res;
  }

  public CommandDef Configure()
  {
    var res = new CommandDef();
    res.Name = "ManyTypes";
    res.Alias = "many-types";
    res.HelpText = "This is just some fake command that we use to test support for different data types.";

    var isTestOption = new CommandOption();
    isTestOption.Name = "IsTest";
    isTestOption.HelpText = "";
    isTestOption.DataType = typeof(Boolean);
    isTestOption.IsRequired = false;
    isTestOption.Options = null;
    res.Options.Add(isTestOption);

    var nameOption = new CommandOption();
    nameOption.Name = "Name";
    nameOption.HelpText = "Just a name.";
    nameOption.DataType = typeof(String);
    nameOption.IsRequired = false;
    nameOption.Aliases = new[] { "-m", "--my-alias" };
    nameOption.Options = null;
    res.Options.Add(nameOption);

    var numberOption = new CommandOption();
    numberOption.Name = "Number";
    numberOption.HelpText = "";
    numberOption.DataType = typeof(Int32);
    numberOption.IsRequired = false;
    numberOption.Options = null;
    res.Options.Add(numberOption);

    var pieOption = new CommandOption();
    pieOption.Name = "Pie";
    pieOption.HelpText = "";
    pieOption.DataType = typeof(Single);
    pieOption.IsRequired = false;
    pieOption.Options = null;
    res.Options.Add(pieOption);

    var animalsOption = new CommandOption();
    animalsOption.Name = "Animals";
    animalsOption.HelpText = "";
    animalsOption.DataType = typeof(String[]);
    animalsOption.IsRequired = false;
    animalsOption.Options = null;
    res.Options.Add(animalsOption);

    var pickThreeOption = new CommandOption();
    pickThreeOption.Name = "PickThree";
    pickThreeOption.HelpText = "";
    pickThreeOption.DataType = typeof(Int32[]);
    pickThreeOption.IsRequired = false;
    pickThreeOption.Options = null;
    res.Options.Add(pickThreeOption);

    return res;
  }
}

