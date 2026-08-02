using System.Text.RegularExpressions;

namespace drewCo.CLI
{
  // ==============================================================================================================================
  public class TypeHint
  {

    private static Regex Pattern = new Regex("([iuf])([0-9]+)(\\[\\])?", RegexOptions.Compiled);

    public bool IsArray { get; set; }
    public bool IsSigned { get; set; }
    public bool IsFloat { get; set; }
    public int DataSize { get; set; }

    // --------------------------------------------------------------------------------------------------------------------------
    public TypeHint(string hint)
    {
      var m = Pattern.Match(hint);

      // Validate contents.
      if (m.Groups.Count < 4)
      {
        throw new InvalidOperationException($"Invalid type hint: {hint}!");
      }

      // groups 1, 2, and 3 are all we care about.
      this.IsArray = m.Groups[3].Value == "[]";
      this.IsFloat = m.Groups[1].Value == "f";
      this.IsSigned = this.IsFloat || m.Groups[1].Value == "i";

      // Data size must be a power of two.
      this.DataSize = int.Parse(m.Groups[2].Value);
      if (this.DataSize < 8 || !IsPowerOfTwo(this.DataSize))
      {
        throw new InvalidOperationException("Data size must be a power of two and greater than 8!");
      }
      if (this.IsFloat && this.DataSize < 16)
      {
        throw new InvalidOperationException("Data size for floating point must be 16, 32, or 64 bits");
      }
      else
      {
        if (DataSize > 128) { throw new InvalidOperationException("Data size for integers must be power of two and between 8 and 128 bits!"); }
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public Type AsType()
    {
      Type res = null!;
      if (IsFloat)
      {
        switch (DataSize)
        {
          case 16:
            res = typeof(Half);
            break;
          case 32:
            res = typeof(float);
            break;
          case 64:
            res = typeof(double);
            break;
          default:
            throw new Exception("Illegal!");
        }
      }
      else
      {
        if (DataSize == 8)
        {
          res = IsSigned ? typeof(sbyte) : typeof(byte);
        }
        else if (DataSize == 16)
        {
          res = IsSigned ? typeof(Int16) : typeof(UInt16);
        }
        else if (DataSize == 32)
        {
          res = IsSigned ? typeof(Int32) : typeof(UInt32);
        }
        else if (DataSize == 64)
        {
          res = IsSigned ? typeof(Int64) : typeof(UInt64);
        }

        else
        {
          throw new Exception("Illegal!");
        }

      }

      if (IsArray)
      {
        res = res.MakeArrayType();
      }

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    // TODO: SHARE:
    public static bool IsPowerOfTwo(int value)
    {
      return value > 0 && (value & (value - 1)) == 0;
    }

  }
}
