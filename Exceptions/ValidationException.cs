namespace ApiwebMVC.Exceptions;

public sealed class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Se encontraron uno o más errores de validación.")
    {
        Errors = errors;
    }
}
