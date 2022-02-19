namespace Allard.Configinator.Core;

public static class Guards
{
    public static string HasValue(string? value, string name)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentNullException(nameof(value))
            : value;
    }

    public static T HasValue<T>(T value, string name)
    {
        return EqualityComparer<T>.Default.Equals(value, default)
            ? throw new ArgumentNullException(nameof(name))
            : value;
    }
}