using Microsoft.Extensions.Logging;

namespace Playlist.Logging;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logDirectory;
    private readonly object _writeLock = new();

    public FileLoggerProvider(string logDirectory)
    {
        _logDirectory = logDirectory;
    }

    public ILogger CreateLogger(string categoryName) =>
        new FileLogger(categoryName, _logDirectory, _writeLock);

    public void Dispose()
    {
    }
}

internal sealed class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logDirectory;
    private readonly object _writeLock;

    public FileLogger(string categoryName, string logDirectory, object writeLock)
    {
        _categoryName = categoryName;
        _logDirectory = logDirectory;
        _writeLock = writeLock;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        if (string.IsNullOrWhiteSpace(message) && exception == null)
        {
            return;
        }

        var singleLineMessage = message.ReplaceLineEndings(" ");
        var singleLineException = exception?.ToString().ReplaceLineEndings(" ") ?? string.Empty;
        var line = $"{DateTimeOffset.Now:O}\t{logLevel}\t{eventId.Id}\t{_categoryName}\t{singleLineMessage}\t{singleLineException}{Environment.NewLine}";

        try
        {
            lock (_writeLock)
            {
                Directory.CreateDirectory(_logDirectory);
                var logPath = Path.Combine(_logDirectory, $"musicbar-{DateTime.UtcNow:yyyyMMdd}.log");
                File.AppendAllText(logPath, line);
            }
        }
        catch (IOException)
        {
            // Logging must never stop the application from serving a request.
        }
        catch (UnauthorizedAccessException)
        {
            // A read-only deployment can continue using the other logging providers.
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
