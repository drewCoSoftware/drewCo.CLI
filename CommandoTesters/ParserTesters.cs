using Commando;
using Commando.Commands;
using dhll.v1;
using System.IO;
using Tommy;

namespace CommandoTesters
{
    public class Tests
    {
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

            var table  = Helpers.FromFile(fromPath, nameof(Generate));
            var cmd = Generate.FromTable(table);

            Assert.IsNotNull(cmd, "The deserialized command should not be null!");

            // Make sure that our data is correct.....
            Assert.That(cmd.Path, Is.EqualTo("example.toml"));
            Assert.That(cmd.TargetLanguage, Is.EqualTo("cpp"));
            Assert.That(cmd.OuputPath, Is.EqualTo("the-output.cs"));

        }

        // ----------------------------------------------------------------------    
        [Test]
        public void CanGenerateCSharpCommandDefFromTOMLFile()
        {
            var defs = ParseDefsFromFile("GenerateCommandDef.toml");
            var def = defs.SingleOrDefault();


            var cg = new CodeGen();
            cg.OutputCSharp(def, "test-output.cs", "Commando.Commands");

            Assert.Fail("please finish this test! (see below)");


            // NOTE: I'll do DHLL later since I don't really have function support in it at this time?
            //// This is kind of what we will do....
            //// juast shove the following code into a function, and develop it from there...
            //TypeDef td = new TypeDef(def.Name, new SourceMetadata());

            //td.Members.Add(new Declare()
            //{


            //});

        }

        // ----------------------------------------------------------------------    
        [Test]
        public void CanParseCommandDefsFromTOMFile()
        {
            var defs = ParseDefsFromFile("GenerateCommandTypes.toml");

            Assert.That(defs.Length, Is.EqualTo(1), "There should be one command definition!");
            var def = defs[0];

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


        private static CommandDef[] ParseDefsFromFile(string fileName)
        {
            string path = Path.Combine("test-data", fileName);
            var generator = new DefGenerator();
            var defs = generator.ParseCommandDefsFromTOML(path);
            return defs;
        }
    }
}