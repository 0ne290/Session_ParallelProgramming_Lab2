namespace ParallelProgrammingLab2.PetriNet;

public class Position
{
    public Position(int tokens = 0)
    {
        if (tokens < 0)
            throw new Exception("Tokens count in a position cannot be less than 0.");

        Tokens = tokens;
    }

    public void Reset()
    { 
        lock (_locker)
        {
            Tokens = 0;
        }
    }

    public void Load(int tokens)
    {
        lock (_locker)
        {
            Tokens += tokens;
        }
    }
    
    public void Unload(int tokens)
    {
        lock (_locker)
        {
            if (Tokens < tokens)
                throw new Exception("Tokens count in a position cannot be less than 0.");

            Tokens -= tokens;
        }
    }
    
    public int Tokens { get; private set; }

    private readonly object _locker = new();
}