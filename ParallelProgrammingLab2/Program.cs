using ParallelProgrammingLab2.PetriNet;

namespace ParallelProgrammingLab2;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("X          Y        X | Y");
        
        Console.WriteLine($"0          0          {(ExecuteSchaeffersStroke(false, false) ? 1 : 0)}");
        
        Console.WriteLine($"0          1          {(ExecuteSchaeffersStroke(false, true) ? 1 : 0)}");
        
        Console.WriteLine($"1          0          {(ExecuteSchaeffersStroke(true, false) ? 1 : 0)}");
        
        Console.WriteLine($"1          1          {(ExecuteSchaeffersStroke(true, true) ? 1 : 0)}");
        
        Console.Write($"{Environment.NewLine}To terminate the program, press any key...");
        Console.ReadKey();
    }

    // Как написано в УМП, если у сети Петри есть несколько готовых к исполнению переходов, то невозможно предсказать
    // какой именно переход выполнится в тот или иной момент. Это значит, что сеть Петри - это система, "живущая своей
    // жизнью". Как можно программно смоделировать такую систему? Конечно же с помощью отдельно выделенных под сеть
    // Петри потоков - по одному на каждый переход в сети. Задача этих потоков одна - запускать в цикле свои переходы
    // до тех пор, пока сеть Петри запущена. Поэтому-то у класса штриха Шеффера и существуют методы Start
    // и Stop, которые запускают и останавливают сеть Петри, моделирующую штрих Шеффера. Соответственно, чтобы получить
    // результат от такой сети, нужно сперва ее запустить, подождать некоторое время, чтобы все переходы, которые могут
    // выполниться, выполнились, остановить ее и только потом получать результат.
    private static bool ExecuteSchaeffersStroke(bool arg1, bool arg2)
    {
        var schaeffersStroke = new SchaeffersStroke(arg1, arg2);

        schaeffersStroke.Start();

        Thread.Sleep(1_000);

        schaeffersStroke.Stop();

        return schaeffersStroke.Result;
    }
}