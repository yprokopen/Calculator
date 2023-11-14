using Microsoft.Extensions.DependencyInjection;

namespace Calculator;

internal class Program
{
    /// <summary>
    /// The Main method should not be changed at all.
    /// </summary>
    /// <param name="args"></param>
    private static async Task Main()
    {
        Console.WriteLine("Mentoring program L2. Async/await.V1. Task 1");
        Console.WriteLine("Calculating the sum of integers from 0 to N.");
        Console.WriteLine("Use 'q' key to exit...");
        Console.WriteLine();
        var timeSpan = TimeSpan.FromSeconds(5);
        using var cts = new CancellationTokenSource(timeSpan);
        var services = new ServiceCollection()
            .AddSingleton<IInputOutputHandler, InputOutputHandler>()
            .AddSingleton<ICalculator, Calculator>()
            .AddSingleton<IInteractionHandler, InteractionHandler>();

        await using var provider = services.BuildServiceProvider();

        var interactionHandler = provider.GetRequiredService<IInteractionHandler>();

        try
        {
            await interactionHandler.InteractAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"You are using trial version. It exits after {timeSpan}.");
        }
    }
}
