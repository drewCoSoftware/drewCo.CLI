using Commando;
using dhll.v1;

namespace CommandoTesters
{
    public class Tests
    {

        // ----------------------------------------------------------------------    
        [Test]
        public void CanGenerateCSharpCommandDefFromTOMLFile()
        {
            var defs = ParseDefsFromFile("GenerateCommandTypes.toml");
            var def  = defs.SingleOrDefault();



            var cg = new CodeGen();
            cg.OutputCSharp(def, "test-output.cs");

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


            var lastOp = def.Options[2]!;
            Assert.IsFalse(lastOp.IsRequired, $"The option for {lastOp.Name} shoul not be required!");
        }


        private static CommandDef[] ParseDefsFromFile(string fileName)
        {
            string path = Path.Combine("test-data", fileName);
            var parser = new DefGenerator();
            var defs = parser.ParseCommandDefsFromTOML(path);
            return defs;
        }
    }
}