using drewCo.CLI.Commands;
using drewCo.Tools;
using drewCo.Tools.Logging;

namespace drewCo.CLI.Tools
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
      var def = generator.ParseCommandDefsFromTOML(g.Path).Single();
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



}
