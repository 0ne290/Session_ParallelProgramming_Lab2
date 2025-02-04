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
public class SchaeffersStroke
{
    public SchaeffersStroke()
    {
        _begin = new Position(1);
        _operand1 = new Position();
        _operand2 = new Position();
        _result = new Position(1);

        var transitions = new Transition[4];
        transitions[0] = new Transition([_begin], [_operand1], (input, output) =>
        {
            if (input[0].Tokens == 1)
            {
                input[0].Unload();
                
                output[0].Load();
                
                Log();
            }
        });
        transitions[1] = new Transition([_operand1, _result], [_operand2, _result], (input, output) =>
        {
            if (input[0].Tokens == 1 && input[1].Tokens == 1)
            {
                input[0].Unload();
                input[1].Unload();
                
                output[0].Load();
                output[1].Load();
                
                Log();
            }
        });
        transitions[2] = new Transition([_operand2, _result], [_operand1, _operand2], (input, output) =>
        {
            if (input[0].Tokens == 1 && input[1].Tokens == 1)
            {
                input[0].Unload();
                input[1].Unload();
                
                output[0].Load();
                output[1].Load();
                
                Log();
            }
        });
        transitions[3] = new Transition([_operand1, _operand2], [_begin, _result], (input, output) =>
        {
            if (input[0].Tokens == 1 && input[1].Tokens == 1)
            {
                input[0].Unload();
                input[1].Unload();
                
                output[0].Load();
                output[1].Load();

                _petriNetIsWorked = false;
            }
        });

        _threads = new Thread[4];
        _handlers = new ThreadStart[4];
        for (var i = 0; i < 4; i++)
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

    private void Log() => Console.WriteLine($"{_operand1.Tokens}          {_operand2.Tokens}          {_result.Tokens}");

    public void Execute()
    {
        Console.WriteLine("X          Y        X | Y");
        Log();
        
        Start();
        
        for (var i = 0; i < 4; i++)
            _threads[i].Join();
    }

    private void Start()
    {
        _begin.Reset();
        _operand1.Reset();
        _operand2.Reset();
        _result.Reset();

        // ReSharper disable once InconsistentlySynchronizedField
        _petriNetIsWorked = true;

        for (var i = 0; i < 4; i++)
            _threads[i] = new Thread(_handlers[i]);

        for (var i = 0; i < 4; i++)
            _threads[i].Start();
    }

    private readonly Position _begin;

    private readonly Position _operand1;

    private readonly Position _operand2;

    private readonly Position _result;

    // Как написано в УМП, если у сети Петри есть несколько готовых к исполнению переходов, то невозможно предсказать
    // какой именно переход выполнится в тот или иной момент. Это значит, что сеть Петри - это система, "живущая своей
    // жизнью". Как можно программно смоделировать такую систему? Конечно же с помощью отдельно выделенных под сеть
    // Петри потоков - по одному на каждый переход в сети. Задача этих потоков одна - запускать в цикле свои переходы
    // до тех пор, пока сеть Петри запущена.
    private readonly Thread[] _threads;

    private readonly ThreadStart[] _handlers;
    
    private bool _petriNetIsWorked;
}