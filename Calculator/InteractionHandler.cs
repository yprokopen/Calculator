using Calculator.Exceptions;

namespace Calculator;

public interface IInteractionHandler
{
    Task InteractAsync(CancellationToken token);
}

internal class InteractionHandler : IInteractionHandler
{
    private readonly IInputOutputHandler _inputOutputHandler;
    private readonly ICalculator _calculator;

    public InteractionHandler(IInputOutputHandler inputOutputHandler, ICalculator calculator)
    {
        _inputOutputHandler = inputOutputHandler;
        _calculator = calculator;
    }

    public async Task InteractAsync(CancellationToken token)
    {
        CancellationTokenSource readCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        CancellationTokenSource calculationCts = CancellationTokenSource.CreateLinkedTokenSource(token);

        try
        {
            token.ThrowIfCancellationRequested();
            Console.WriteLine("Enter N: ");
            var number = await ReadNumberAsync(readCts.Token);

            while (true)
            {
                var calculationTask = ProcessCalculationAsync(number, calculationCts.Token);
                var readTask = ReadNumberAsync(readCts.Token);

                await Task.WhenAny(calculationTask, readTask);

                if (readTask.IsCompleted && !calculationTask.IsCompleted)
                {
                    number = readTask.Result;
                    calculationCts.Cancel();
                    calculationCts.Dispose();
                    calculationCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                }
                else if (!readTask.IsCompleted && calculationTask.IsCompleted)
                {
                    readCts.Cancel();
                    readCts.Dispose();
                    readCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                    await _inputOutputHandler.WriteLineAsync("Enter N: ", readCts.Token);
                    number = await ReadNumberAsync(readCts.Token);
                }
                else
                {
                    number = readTask.Result;
                }
            }
        }
        catch (ApplicationExitRequestedException)
        {
            Console.WriteLine("Exit was requested..");
        }
        finally
        {
            readCts?.Dispose();
            calculationCts?.Dispose();
        }
    }

    private async Task<int> ReadNumberAsync(CancellationToken token)
    {
        int number;
        while (true)
        {
            try
            {
                var str = await _inputOutputHandler.ReadLineAsync(token);

                if (int.TryParse(str, out number))
                {
                    break;
                }
                else
                {
                    await _inputOutputHandler.WriteLineAsync($"Invalid input. Please try again.", token);
                }
            }
            catch (TaskCanceledException)
            {

            }
        }

        return number;
    }

    private async Task ProcessCalculationAsync(int number, CancellationToken token)
    {
        try
        {
            await _inputOutputHandler.WriteLineAsync($"The task for {number} started... Enter N to cancel the request:", token);
            var sum = await _calculator.CalculateSumAsync(number, token);
            await _inputOutputHandler.WriteLineAsync($"Sum for {number} = {sum}.", token);
        }
        catch (TaskCanceledException)
        {
            await _inputOutputHandler.WriteLineAsync($"Sum for {number} cancelled...", CancellationToken.None);
        }
    }
}
