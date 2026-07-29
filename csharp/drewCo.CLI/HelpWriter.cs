using drewCo.Tools;
using System.Reflection;

namespace drewCo.CLI
{

  // ==============================================================================================================================
  /// <summary>
  /// So we can write help messages, and not clutter up the rest of the code with it.
  /// </summary>
  public class HelpWriter
  {
    private bool CanWrite = false;
    private int MaxWidth = 0;

    private bool WriteColumns = true;

    public int Col1Width { get; private set; } = 1;
    public int Col2Width { get; private set; } = -1;
    public int Indent { get; private set; } = 0;
    private string? IndentString = string.Empty;

    // --------------------------------------------------------------------------------------------------------------------------
    public HelpWriter()
    {
      Init();
      SetIndent(0);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Init()
    {
      CanWrite = !Console.IsOutputRedirected && Console.BufferWidth > 0;
      MaxWidth = CanWrite ? Console.BufferWidth : 0;

      if (CanWrite)
      {
        Col1Width = Math.Min(20, MaxWidth / 2);
        Col2Width = MaxWidth - Col1Width;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void SetCol1Size(int width)
    {
      this.Col1Width = width;
      this.Col2Width = this.MaxWidth - Col1Width;

      if (this.Col1Width > MaxWidth)
      {
        this.Col1Width = MaxWidth;
        this.Col2Width = 0;
        WriteColumns = false;
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
    public void WriteMessage(string col1Msg, string col2Msg)
    {
      if (!CanWrite) { return; }

      // Format the messages.
      if (WriteColumns)
      {
        WriteByColumn(col1Msg, col2Msg);
      }
      else
      {
        // This is used when the screen is deemed too narrow... a rare case, and untested!
        WriteByRow(col1Msg, col2Msg);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private void WriteByRow(string col1Msg, string col2Msg)
    {
      var col1Lines = TextWrapper.Wrap(col1Msg, Col1Width, this.Indent);
      var col2Lines = TextWrapper.Wrap(col2Msg, Col1Width, this.Indent);
      foreach (var item in col1Lines)
      {
        Console.WriteLine(item);
      }
      Console.WriteLine();
      foreach (var item in col2Lines)
      {
        Console.WriteLine(item);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private void WriteByColumn(string col1Msg, string col2Msg)
    {
      var col1Lines = TextWrapper.Wrap(col1Msg, Col1Width, this.Indent);
      var col2Lines = TextWrapper.Wrap(col2Msg, Col2Width, this.Indent * 2);

      // Make it so that each line group is the same size.
      if (col1Lines.Count < col2Lines.Count)
      {
        int diff = col2Lines.Count - col1Lines.Count;
        for (int i = 0; i < diff; i++)
        {
          col1Lines.Add(string.Empty);
        }
      }
      else if (col2Lines.Count < col1Lines.Count)
      {
        int diff = col1Lines.Count - col2Lines.Count;
        for (int i = 0; i < diff; i++)
        {
          col2Lines.Add(string.Empty);
        }
      }

      // Now we will squish them all into a single line + squirt it out to the console.
      int len = col1Lines.Count;
      for (int i = 0; i < len; i++)
      {
        string useLine = StringTools.PadString(col1Lines[i], Col1Width);
        useLine += col2Lines[i];

        Console.WriteLine(useLine);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    internal void WriteNameAndversion()
    {
      var asm = GetAsmName();
      string toWrite = asm.Name + " " + asm.Version.ToString();
      WriteMessage(toWrite);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    internal void WriteVersion()
    {
      var asm = GetAsmName();
      string version = asm.Version.ToString();
      WriteMessage(version);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private AssemblyName _AsmName = null!;
    private AssemblyName GetAsmName()
    {
      if (_AsmName == null)
      {
        var asm = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
        _AsmName = asm.GetName();
      }
      return _AsmName;
    }
  }

}
