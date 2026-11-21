namespace CS2_GEO.Exceptions;

/// <summary>
/// Thrown when the game state changed under a valid request because another player acted first.
/// Always 409 Conflict, handled centrally by GameExceptionHandler.
/// Expected outcomes (not found, wrong code, already started) stay as result enums.
/// </summary>
public abstract class GameException(string message) : Exception(message);
