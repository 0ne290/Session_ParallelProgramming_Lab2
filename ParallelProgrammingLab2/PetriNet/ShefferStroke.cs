namespace ParallelProgrammingLab2.PetriNet;

// Классы позиции и перехода спроектированы в расчете на то, чтобы классы сетей Петри использовали их следующим образом:
// 1. Сеть Петри создает все необходимые ей позиции.
// 2. Создает переходы, передавая им списки входных-выходных позиций и т. н. триггеры. Триггер - функция с параметрами
// входных-выходных позиций. Функция триггера, конечно, может быть любой, но с точки зрения теории сетей Петри,
// она должна использовать список входных позиций для проверки того, возможен ли вообще переход, и если да, то
// исполнять его - т. е. просто разгружать входные позиции и загружать выходные.
// 3. Создает на каждый переход поток, запускающий в цикле триггер перехода до тех пор, пока сеть Петри запущена.
// 4. В любой момент времени триггер должен работать максимум только у одного перехода, чтобы не было состояния гонки
// за позициями.
public class ShefferStroke
{
    public ShefferStroke()
    {
        _positions = new Position[PositionsCount]
        {
            new(1),
            new(),
            new(),
            new(),
            new(),
            new(),
            new(),
            new(),
            new(),
            new(),
            new(),
            new(),
            new()
        };

        var transitions = new Transition[TransitionsCount]
        {
            new([_positions[0]], [_positions[1], _positions[5], _positions[7]], (input, output) =>
            {
                if (input[0].Tokens > 0)
                {
                    input[0].Unload();

                    output[0].Load();
                    output[1].Load();
                    output[2].Load();

                    LogOperands();
                }
            }),
            new([_positions[1], _positions[4]], [_positions[2], _positions[5], _positions[8]], (input, output) =>
            {
                if (input[0].Tokens > 0 && input[1].Tokens > 0)
                {
                    input[0].Unload();
                    input[1].Unload();

                    output[0].Load();
                    output[1].Load();
                    output[2].Load();

                    LogOperands();
                }
            }),
            new([_positions[2], _positions[4]], [_positions[3], _positions[6], _positions[7]], (input, output) =>
            {
                if (input[0].Tokens > 0 && input[1].Tokens > 0)
                {
                    input[0].Unload();
                    input[1].Unload();

                    output[0].Load();
                    output[1].Load();
                    output[2].Load();

                    LogOperands();
                }
            }),
            new([_positions[3], _positions[4]], [_positions[6], _positions[8]], (input, output) =>
            {
                if (input[0].Tokens > 0 && input[1].Tokens > 0)
                {
                    input[0].Unload();
                    input[1].Unload();

                    output[0].Load();
                    output[1].Load();

                    LogOperands();
                }
            }),
            new([_positions[5], _positions[7]], [_positions[9]], (input, output) =>
            {
                if (input[0].Tokens > 0 && input[1].Tokens > 0)
                {
                    input[0].Unload();
                    input[1].Unload();

                    output[0].Load();
                }
            }),
            new([_positions[5], _positions[8]], [_positions[9]], (input, output) =>
            {
                if (input[0].Tokens > 0 && input[1].Tokens > 0)
                {
                    input[0].Unload();
                    input[1].Unload();

                    output[0].Load();
                }
            }),
            new([_positions[6], _positions[7]], [_positions[9]], (input, output) =>
            {
                if (input[0].Tokens > 0 && input[1].Tokens > 0)
                {
                    input[0].Unload();
                    input[1].Unload();

                    output[0].Load();
                }
            }),
            new([_positions[6], _positions[8]], [_positions[10]], (input, output) =>
            {
                if (input[0].Tokens > 0 && input[1].Tokens > 0)
                {
                    input[0].Unload();
                    input[1].Unload();

                    output[0].Load();
                }
            }),
            new([_positions[9]], [_positions[11]], (input, output) =>
            {
                if (input[0].Tokens > 0)
                {
                    input[0].Unload();

                    output[0].Load();
                }
            }),
            new([_positions[10]], [_positions[12]], (input, output) =>
            {
                if (input[0].Tokens > 0)
                {
                    input[0].Unload();

                    output[0].Load();
                }
            }),
            new([_positions[11]], [_positions[4]], (input, output) =>
            {
                if (input[0].Tokens > 0)
                {
                    LogResult();
                    
                    input[0].Unload();

                    output[0].Load();
                }
            }),
            new([_positions[12]], [_positions[4]], (input, output) =>
            {
                if (input[0].Tokens > 0)
                {
                    LogResult();
                    
                    input[0].Unload();

                    output[0].Load();
                }
            })
        };

        _threads = new Thread[TransitionsCount];
        _handlers = new ThreadStart[TransitionsCount];
        for (var i = 0; i < TransitionsCount; i++)
        {
            var j = i;
            _handlers[i] = () =>
            {
                while (true)
                {
                    lock (this)
                    {
                        if (_petriNetIsWorked)
                            transitions[j].Execute();
                        else
                            break;
                    }

                    Thread.Yield();
                }
            };
        }

        _petriNetIsWorked = false;
    }

    private void LogOperands() => Console.Write($"{(_positions[5].Tokens > 0 ? "0" : "1")}          {(_positions[7].Tokens > 0 ? "0" : "1")}");

    private void LogResult() => Console.WriteLine($"          {(_positions[11].Tokens > 0 ? "1" : "0")}");

    public void Start()
    {
        if (_petriNetIsWorked)
        {
            Stop();
            
            throw new Exception("Petri net is already started.");
        }
        
        for (var i = 0; i < PositionsCount; i++)
            _positions[i].Reset();

        for (var i = 0; i < TransitionsCount; i++)
            _threads[i] = new Thread(_handlers[i]);
        
        Console.WriteLine($"Work log of Sheffer stroke Petri net:{Environment.NewLine}");
        Console.WriteLine("X          Y        X | Y");
        
        _petriNetIsWorked = true;

        for (var i = 0; i < TransitionsCount; i++)
            _threads[i].Start();
    }

    public void Stop()
    {
        _petriNetIsWorked = false;
        
        for (var i = 0; i < TransitionsCount; i++)
            _threads[i].Join();
    }

    private const int PositionsCount = 13;
    
    private const int TransitionsCount = 12;

    private readonly Position[] _positions;

    // Как написано в УМП, если у сети Петри есть несколько готовых к исполнению переходов, то невозможно предсказать
    // какой именно переход выполнится в тот или иной момент. Это значит, что сеть Петри - это система, "живущая своей
    // жизнью". Как можно программно смоделировать такую систему? Конечно же с помощью отдельно выделенных под сеть
    // Петри потоков - по одному на каждый переход в сети. Задача этих потоков одна - запускать в цикле свои переходы
    // до тех пор, пока сеть Петри запущена.
    private readonly Thread[] _threads;

    private readonly ThreadStart[] _handlers;
    
    private bool _petriNetIsWorked;
}