using System;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Bus.Tests;

public class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test1()
    {
        var bus = new TheBus();
        var test1 = new Action<Junk>(j => { _testOutputHelper.WriteLine("1 - " + j.Message); });
        var test2 = new Action<Junk>(j => { _testOutputHelper.WriteLine("2 - " + j.Message); });
        bus.Subscribe(test1);
        bus.Subscribe(test2);

        bus.Publish(new Junk("wow!"));
        bus.Publish(new Junk("wow2!"));
    }
    

    public record Junk(string Message);
}