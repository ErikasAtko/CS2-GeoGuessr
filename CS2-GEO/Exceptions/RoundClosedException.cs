namespace CS2_GEO.Exceptions;

public class RoundClosedException(int roundNumber) : GameException($"Round {roundNumber} has already ended")
{
    public int RoundNumber { get; } = roundNumber;
}
