using Calculator.Exceptions;

namespace Calculator;

public interface IInputOutputHandler
{
    Task WriteLineAsync(string text, CancellationToken token);
    Task<string> ReadLineAsync(CancellationToken token);

}

internal class InputOutputHandler : IInputOutputHandler
{
    private const ConsoleKey END_KEY = ConsoleKey.Q;
    private const ConsoleKey ENTER_LINE_KEY = ConsoleKey.Enter;

    private readonly List<ConsoleKeyInfo> _buffer = new();

    public Task WriteLineAsync(string text, CancellationToken token)
    {
        return Task.Run(() =>
        {
            lock (_buffer)
            {
                if (_buffer.Count > 0)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                }

                foreach (var character in text)
                {
                    token.ThrowIfCancellationRequested();
                    Console.Write(character);
                }

                Console.WriteLine();

                foreach (var keyInfo in _buffer)
                {
                    token.ThrowIfCancellationRequested();
                    Console.Write(keyInfo.KeyChar);
                }
            }
        }, token);
    }

    public Task<string> ReadLineAsync(CancellationToken token)
    {
        return Task.Run(() =>
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                var key = Console.ReadKey();

                if (key.Key == END_KEY)
                {
                    Console.SetCursorPosition(0, Console.CursorTop + 1);
                    throw new ApplicationExitRequestedException();
                }
                else if (key.Key == ENTER_LINE_KEY)
                {
                    Console.SetCursorPosition(0, Console.CursorTop + 1);
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace && _buffer.Count > 0)
                {
                    lock (_buffer)
                    {
                        _buffer.RemoveAt(_buffer.Count - 1);
                        Console.Write(" \b");
                    }
                }
                else
                {
                    lock (_buffer)
                    {
                        _buffer.Add(key);
                    }
                }
            }

            lock (_buffer)
            {
                var str = string.Join("", _buffer.Select(x => x.KeyChar));
                _buffer.Clear();
                return str;
            }
        }, token);
    }
}
