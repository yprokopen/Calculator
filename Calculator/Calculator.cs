namespace Calculator;

public interface ICalculator
{
    Task<long> CalculateSumAsync(int number, CancellationToken token);
}
internal class Calculator : ICalculator
{
    public async Task<long> CalculateSumAsync(int number, CancellationToken token)
    {
        long sum = 0;

        for (var i = 0; i < number; i++)
        {
            // i + 1 is to allow 2147483647 (Max(Int32)) 
            sum += (i + 1);
            await Task.Delay(10, token);
        }

        return sum;
    }
}