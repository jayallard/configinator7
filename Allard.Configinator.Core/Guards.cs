﻿namespace Allard.Configinator.Core;

public static class Guards
{
    public static string NotEmpty(string? value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentNullException(nameof(value))
            : name;

    public static T NotDefault<T>(T value, string name) =>
        EqualityComparer<T>.Default.Equals(value, default)
            ? throw new ArgumentNullException(nameof(name))
            : value;
}