namespace ParallelProgrammingLab2.PetriNet;

public class Transition
{
    public Transition(IReadOnlyList<Position> inputPositions, IReadOnlyList<Position> outputPositions, Action<IReadOnlyList<Position>, IReadOnlyList<Position>> trigger)
    {
        _inputPositions = inputPositions;
        _outputPositions = outputPositions;
        _trigger = trigger;
    }

    public void Execute() => _trigger(_inputPositions, _outputPositions);

    private readonly IReadOnlyList<Position> _inputPositions;
    
    private readonly IReadOnlyList<Position> _outputPositions;
    
    private readonly Action<IReadOnlyList<Position>, IReadOnlyList<Position>> _trigger;
}