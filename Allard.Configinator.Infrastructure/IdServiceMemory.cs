﻿using Allard.Configinator.Core.Services;

namespace Allard.Configinator.Infrastructure;

public class IdServiceMemory : IIdService
{
    private long id = 0;
    public Task<long> GetNextIdAsync(string type)
    {
        return Task.FromResult(id++);
    }
}