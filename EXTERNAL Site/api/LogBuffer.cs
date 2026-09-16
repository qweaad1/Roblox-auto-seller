using System.Collections.Concurrent;
using System.Text;

namespace qweaadAPI
{
   
    public enum LogLevelType
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public static class LogBuffer
    {
        private static readonly ConcurrentQueue<string> _buffer = new();
        private static readonly object _lock = new();
        private static readonly string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"api_{DateTime.Now:yyyyMMdd}.log");
        private static Timer? _flushTimer;
        private static bool _isInitialized = false;

        static LogBuffer()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath)!);
            _flushTimer = new Timer(Flush, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public static void Log(string message, LogLevelType level = LogLevelType.Info)
        {
            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
            _buffer.Enqueue(entry);

            if (_buffer.Count >= GlobalData.MaxLogBufferSize)
            {
                Flush(null);
            }
        }

        public static void LogError(Exception ex, string context = "")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"{context} - {ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            Console.ResetColor();
            Log($"{context} - {ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}", LogLevelType.Error);
        }

        public static void LogApiRequest(string endpoint, string method, int statusCode, string? response = null)
        {
           // Console.WriteLine($"[API] {method} {endpoint} -> {statusCode} {(response?.Length > 100 ? response[..100] + "..." : response)}");
            Log($"[API] {method} {endpoint} -> {statusCode} {(response?.Length > 100 ? response[..100] + "..." : response)}", LogLevelType.Debug);
        }

        private static void Flush(object? state)
        {
            if (_buffer.IsEmpty) return;

            lock (_lock)
            {
                try
                {
                    var sb = new StringBuilder();
                    while (_buffer.TryDequeue(out var entry))
                    {
                        sb.AppendLine(entry);
                    }

                    if (sb.Length > 0)
                    {
                        File.AppendAllText(_logFilePath, sb.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Log flush error: {ex.Message}");
                }
            }
        }

        public static void FlushAndStop()
        {
            _flushTimer?.Dispose();
            Flush(null);
        }
    }
}