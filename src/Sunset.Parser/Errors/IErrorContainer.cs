namespace Sunset.Parser.Errors;

public interface IErrorContainer
{
    List<Error> Errors { get; }
    bool HasErrors { get; }
    void AddError(ErrorCode code);
}