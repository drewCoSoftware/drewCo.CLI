namespace drewCo.CLI
{
  // ==================================================================================================
  public interface ICommand
  {
    CommandValidationResult Validate();
    CommandDef Configure();
  }

  // ==================================================================================================
  /// <summary>
  /// This is used when validating the commands that have been read in from a command line, file, etc.
  /// </summary>
  public class CommandValidationResult
  {
    public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
    public void AddError(string errMsg)
    {
      var err = new ValidationError(errMsg);
      Errors.Add(err);
    }
  }

  // ==================================================================================================
  public class ValidationError
  {
    public ValidationError(string msg_)
    {
      Message = msg_;
    }
    public string Message { get; private set; }
  }


}

