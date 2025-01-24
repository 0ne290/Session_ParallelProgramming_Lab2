namespace ParallelProgrammingLab2.PetriNet;

public class SchaeffersStroke
{
    public SchaeffersStroke(bool arg1, bool arg2)
    {
        var arg1Position = new Position(arg1 ? 1 : 0);
        var arg2Position = new Position(arg2 ? 1 : 0);
        _resultPosition = new Position(1);

        _transision1 = new Transition([arg1Position, arg2Position], [resultPosition], (inputPositions, outputPositions) =>
                                         {
                                             if (inputPositions[0].Tokens > 0 && inputPositions[1].Tokens > 0)
                                             {
                                                 outputPositions[0].Load(2);
                                                 inputPositions[0].Unload(1);
                                                 inputPositions[2].Unload(1);
                                             }
                                         });
        _transision2 = new Transition([resultPosition], [], (inputPositions, outputPositions) =>
                                         {
                                             if (inputPositions[0].Tokens > 2)
                                                 inputPositions[0].Unload(1);
                                         });
    }

    public void Start()
    {
        _start = true;
        
        _thread1 = new Thread(() =>
                                 {
                                     while (_start)
                                     {
                                         lock (this)
                                         {
                                             transition1.Execute();
                                         }
                                         Thread.Yield();
                                     }
                                 });
        _thread2 = new Thread(() =>
                                 {
                                     while (_start)
                                     {
                                         lock (this)
                                         {
                                             transition2.Execute();
                                         }
                                         Thread.Yield();
                                     }
                                 });

        _thread1.Start();
        _thread2.Start();
    }

    public void Stop()
    {
        _start = false;

        _thread1.Join();
        _thread2.Join();
    }

    public void 

    public bool Result => _resultPosition.Tokens > 0;
}
