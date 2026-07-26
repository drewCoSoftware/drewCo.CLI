using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commando
{

  // ==============================================================================================================================
  /// <summary>
  /// So we can write help messages, and not clutter up the rest of the code with it.
  /// </summary>
  public class HelpWriter
  {
    private bool CanWrite = false;
    private int Width = 0;

    private int Col1Width = -1;
    private int Col2Width = -1;
    public int Indent { get; private set; } = 0;
    private string? IndentString = string.Empty;

    // --------------------------------------------------------------------------------------------------------------------------
    public HelpWriter()
    {
      Init();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Init()
    {
      CanWrite = !Console.IsOutputRedirected && Console.BufferWidth > 0;
      Width = CanWrite ? Console.BufferWidth : 0;

      if (CanWrite)
      {
        Col1Width = Math.Min(20, Width / 2);
        Col2Width = Width - Col1Width;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void SetIndent(int value)
    {
      Indent = value;
      if (Indent > 0)
      {
        IndentString = new string(' ', Indent);
      }
      else
      {
        IndentString = string.Empty;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Write a blank line.
    /// </summary>
    public void WriteMessage()
    {
      Console.WriteLine();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void WriteMessage(string msg)
    {
      if (!CanWrite) { return; }

      string useMsg = IndentString + msg;


      // NOTE: This kind of code should be transferred to ConsoleHelper in the tools lib.
      Console.WriteLine(useMsg);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void WriteMessage(string col1, string col2)
    {
      if (!CanWrite) { return; }

      // TODO: We can care about wrapping, etc. later....
      // I think we would have a starting line # + col position, then we can compute a
      // rectangle, and write the characters line by line.
      // we would also report the number of lines / width that we used....

      // NOTE: This kind of code should be transferred to ConsoleHelper in the tools lib.

      string hacked = col1 + "\t\t" + col2;
      WriteMessage(hacked);



    }

  }

}
