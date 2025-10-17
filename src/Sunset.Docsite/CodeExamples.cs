namespace Sunset.Docsite;

public static class CodeExamples
{
    public const string ElementExample = """
                                         define Square:
                                             inputs:
                                                 Width <w> {mm} = 100 {mm}
                                                 Length <l> {mm} = 200 {mm}
                                             outputs:
                                                 Area <A> {mm^2} = Width * Length
                                         end
                                           
                                         SquareInstance = Square(
                                             Width = 200 {mm},
                                             Length = 350 {mm}
                                         )
                                           
                                         Result {mm^2} = SquareInstance.Area
                                         """;
}