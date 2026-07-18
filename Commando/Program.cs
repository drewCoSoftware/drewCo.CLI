using Tommy;

namespace Commando
{
    internal class Program
    {
        // ------------------------------------------------------------------------------------------
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            string usePath = Path.Combine("test-data", "ExampleCommand1.toml");
          //  OpenToml(usePath);

        }

    }


    /// <summary>
    /// Standin for drewco.tools.log.
    /// </summary>
    internal static class Log
    {
        public static void Info(string msg)
        {
            Console.WriteLine($"{msg}");
        }
    }
}
