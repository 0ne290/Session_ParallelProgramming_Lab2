using ParallelProgrammingLab2.PetriNet;

namespace ParallelProgrammingLab2;

internal static class Program
{
    private static void Main()
    {
        var schaeffersStroke = new SchaeffersStroke();
        schaeffersStroke.Execute();
        
        Console.Write($"{Environment.NewLine}To terminate the program, press any key...");
        Console.ReadKey();
    }
}