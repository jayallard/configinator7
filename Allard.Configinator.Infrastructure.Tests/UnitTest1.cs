using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Allard.Configinator.Infrastructure.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var serviceProvider = new ServiceCollection()
            .AddMediatR(typeof(UnitTest1))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.Publish(new Stuff());
    }

    public class Stuff : INotification
    {
        
    }

    public class EventHandler : INotificationHandler<Stuff>
    {
        public Task Handle(Stuff notification, CancellationToken cancellationToken)
        {
            Console.WriteLine("received");
            return Task.CompletedTask;
        }
    }
}