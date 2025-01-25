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
    public SchaeffersStroke(bool arg1, bool arg2)
    {
        var arg1Position = new Position(arg1 ? 1 : 0);
        var arg2Position = new Position(arg2 ? 1 : 0);
        _resultPosition = new Position(1);

        var transition1 = new Transition([arg1Position, arg2Position], [_resultPosition], (inputPositions, outputPositions) =>
        {
            if (inputPositions[0].Tokens < 1 || inputPositions[1].Tokens < 1)
                return;
            
            outputPositions[0].Load(2);
            inputPositions[0].Unload(1);
            inputPositions[1].Unload(1);
        });
        var transition2 = new Transition([_resultPosition], [], (inputPositions, _) =>
        {
            if (inputPositions[0].Tokens > 2)
                inputPositions[0].Unload(3);
        });

        _executeTransition1 = () =>
        {
            while (_petriNetIsWorked)
            {
                lock (this)
                {
                    transition1.Execute();
                }

                Thread.Yield();
            }
        };
        _executeTransition2 = () =>
        {
            while (_petriNetIsWorked)
            {
                lock (this)
                {
                    transition2.Execute();
                }

                Thread.Yield();
            }
        };

        _petriNetIsWorked = false;
        _thread1 = null!;
        _thread2 = null!;
    }

    public void Start()
    {
        _petriNetIsWorked = true;
        
        _thread1 = new Thread(_executeTransition1);
        _thread2 = new Thread(_executeTransition2);

        _thread1.Start();
        _thread2.Start();
    }

    public void Stop()
    {
        _petriNetIsWorked = false;

        _thread1.Join();
        _thread2.Join();
    }

    public bool Result => _resultPosition.Tokens > 0;
    
    private bool _petriNetIsWorked;

    private Thread _thread1;
    
    private Thread _thread2;

    private readonly Position _resultPosition;

    private readonly ThreadStart _executeTransition1;
    
    private readonly ThreadStart _executeTransition2;
}