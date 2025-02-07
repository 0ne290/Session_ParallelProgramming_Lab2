using ParallelProgrammingLab2.PetriNet;

namespace ParallelProgrammingLab2;

internal static class Program
{
    private static void Main()
    {
        var shefferStroke = new ShefferStroke();
        
        Console.WriteLine($"Petri net is started, will stop in 5 seconds...{Environment.NewLine}");
        
        shefferStroke.Start();
        
        Thread.Sleep(5000);
        
        shefferStroke.Stop();
        
        Console.WriteLine($"{Environment.NewLine}Petri net is stopped...");
        
        Console.Write($"{Environment.NewLine}To terminate the program, press any key...");
        Console.ReadKey();
    }
}