using System;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Infrastructure;
using Allard.DomainDrivenDesign;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Allard.Configinator.Infrastructure.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var serviceProvider = new ServiceCollection()
            .AddMediatR(typeof(UnitTest1))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();
        //await mediator.Publish(new MediatorNotification<StuffHappenedEvent>(new StuffHappenedEvent()));
        var pub = new MediatorPublisher(mediator);
        await pub.PublishAsync(new[] {new StuffHappenedEvent()});
    }

    public class Handler : INotificationHandler<MediatorNotification<StuffHappenedEvent>>
    {
        public Task Handle(MediatorNotification<StuffHappenedEvent> notification, CancellationToken cancellationToken)
        {
            Console.WriteLine("stuff event handled");
            return Task.CompletedTask;
        }
    }
    
    public class Handler2 : INotificationHandler<MediatorNotification<StuffHappenedEvent>>
    {
        public Task Handle(MediatorNotification<StuffHappenedEvent> notification, CancellationToken cancellationToken)
        {
            Console.WriteLine("stuff event handled2");
            return Task.CompletedTask;
        }
    }

    public class GoodyHandler : INotificationHandler<MediatorNotification<Goody>>
    {
        public Task Handle(MediatorNotification<Goody> notification, CancellationToken cancellationToken)
        {
            Console.WriteLine("goody");
            return Task.CompletedTask;
        }
    }

    public class StuffHappenedEvent : IDomainEvent
    {
        public DateTime EventDate { get; }
    }

    public class Goody : IDomainEvent
    {
        public DateTime EventDate { get; }
    }
}