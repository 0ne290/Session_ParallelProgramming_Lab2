namespace ParallelProgrammingLab2.PetriNet;

public class Position
{
    public Position(int tokens = 0)
    {
        if (tokens < 0)
            throw new Exception("Tokens count in a position cannot be less than 0.");

        Tokens = tokens;
        _initialTokens = tokens;
    }

    public void Reset()
    {
        Tokens = _initialTokens;
    }

    public void Load(int tokens = 1)
    {
        Tokens += tokens;
    }

    public void Unload(int tokens = 1)
    {
        if (Tokens < tokens)
            throw new Exception("Tokens count in a position cannot be less than 0.");

        Tokens -= tokens;
    }

    public int Tokens { get; private set; }

    private readonly int _initialTokens;
}