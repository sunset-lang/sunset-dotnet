namespace Sunset.Parser.Errors;

public interface IErrorContainer
{
    List<Error> Errors { get; }
    void AddError(ErrorCode code);
    bool HasErrors { get; }
}