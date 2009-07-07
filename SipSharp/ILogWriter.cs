using System;
using System.Diagnostics;
using System.Text;

namespace SipSharp
{
    /// <summary>
    /// Priority for log entries
    /// </summary>
    /// <seealso cref="ILogWriter"/>
    public enum LogLevel
    {
        /// <summary>
        /// Very detailed logs to be able to follow the flow of the program.
        /// </summary>
        Trace,

        /// <summary>
        /// Logs to help debug errors in the application
        /// </summary>
        Debug,

        /// <summary>
        /// Information to be able to keep track of state changes etc.
        /// </summary>
        Info,

        /// <summary>
        /// Something did not go as we expected, but it's no problem.
        /// </summary>
        Warning,

        /// <summary>
        /// Something that should not fail failed, but we can still keep
        /// on going.
        /// </summary>
        Error,

        /// <summary>
        /// Something failed, and we cannot handle it properly.
        /// </summary>
        Fatal
    }

    /// <summary>
    /// Interface used to write to log files.
    /// </summary>
    public interface ILogWriter
    {
        /// <summary>
        /// Write an entry to the log file.
        /// </summary>
        /// <param name="source">object that is writing to the log</param>
        /// <param name="priority">importance of the log message</param>
        /// <param name="message">the message</param>
        void Write(object source, LogLevel priority, string message);
    }

    /// <summary>
    /// This class writes to the console. 
    /// </summary>
    /// <remarks>
    /// It colors the output depending on the log level 
    /// and includes a 3-level stack trace (in debug mode)
    /// </remarks>
    /// <seealso cref="ILogWriter"/>
    public sealed class ConsoleLogWriter : ILogWriter
    {
        /// <summary>
        /// The actual instance of this class.
        /// </summary>
        public static readonly ConsoleLogWriter Instance = new ConsoleLogWriter();

        /// <summary>
        /// Write an entry
        /// </summary>
        /// <param name="source">object that wrote the log entry.</param>
        /// <param name="level">Importance of the log message</param>
        /// <param name="message">The message.</param>
        public void Write(object source, LogLevel level, string message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString());
            sb.Append(" ");
            sb.Append(level.ToString().PadRight(10));
            sb.Append(" | ");
#if DEBUG
            StackTrace trace = new StackTrace();
            StackFrame[] frames = trace.GetFrames();
            if (frames != null)
            {
                int endFrame = frames.Length > 4 ? 4 : frames.Length;
                int startFrame = frames.Length > 0 ? 1 : 0;
                for (int i = startFrame; i < endFrame; ++i)
                {
                    sb.Append(frames[i].GetMethod().Name);
                    sb.Append(" -> ");
                }
            }
#else
            sb.Append(System.Reflection.MethodBase.GetCurrentMethod().Name);
            sb.Append(" | ");
#endif
            sb.Append(message);

            Console.ForegroundColor = GetColor(level);
            Console.WriteLine(sb.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Get color for the specified log level
        /// </summary>
        /// <param name="level">Level for the log entry</param>
        /// <returns>A <see cref="ConsoleColor"/> for the level</returns>
        public static ConsoleColor GetColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    return ConsoleColor.DarkGray;
                case LogLevel.Debug:
                    return ConsoleColor.Gray;
                case LogLevel.Info:
                    return ConsoleColor.White;
                case LogLevel.Warning:
                    return ConsoleColor.DarkMagenta;
                case LogLevel.Error:
                    return ConsoleColor.Magenta;
                case LogLevel.Fatal:
                    return ConsoleColor.Red;
            }

            return ConsoleColor.Yellow;
        }
    }

    /// <summary>
    /// Default log writer, writes everything to null (nowhere).
    /// </summary>
    /// <seealso cref="ILogWriter"/>
    public sealed class NullLogWriter : ILogWriter
    {
        /// <summary>
        /// The logging instance.
        /// </summary>
        public static readonly NullLogWriter Instance = new NullLogWriter();

        /// <summary>
        /// Writes everything to null
        /// </summary>
        /// <param name="source">object that wrote the log entry.</param>
        /// <param name="level">Importance of the log message</param>
        /// <param name="message">The message.</param>
        public void Write(object source, LogLevel level, string message)
        {}
    }

    /// <summary>
    /// Factory is used to create new logs in the system.
    /// </summary>
    public static class LogFactory
    {
        private static ILogFactory _factory;

        /// <summary>
        /// Assigns log factory being used.
        /// </summary>
        /// <param name="logFactory">The log factory.</param>
        /// <exception cref="InvalidOperationException">A factory have already been assigned.</exception>
        public static void Assign(ILogFactory logFactory)
        {
            if (_factory != null && !(logFactory is NullLogFactory))
                throw new InvalidOperationException("A factory have already been assigned.");
            _factory = logFactory;
        }

        /// <summary>
        /// Create a new logger.
        /// </summary>
        /// <param name="type">Type that requested a logger.</param>
        /// <returns>Logger for the specified type;</returns>
        public static ILogWriter CreateLogger(Type type)
        {
            return _factory.CreateLogger(type);
        }
    }

    /// <summary>
    /// Factory implementation used to create logs.
    /// </summary>
    public interface ILogFactory
    {
        /// <summary>
        /// Create a new logger.
        /// </summary>
        /// <param name="type">Type that requested a logger.</param>
        /// <returns>Logger for the specified type;</returns>
        /// <remarks>
        /// MUST ALWAYS return a logger. Return <see cref="NullLogWriter"/> if no logging
        /// should be used.
        /// </remarks>
        ILogWriter CreateLogger(Type type);
    }

    /// <summary>
    /// Factory creating null logger.
    /// </summary>
    public class NullLogFactory : ILogFactory
    {
        /// <summary>
        /// Create a new logger.
        /// </summary>
        /// <param name="type">Type that requested a logger.</param>
        /// <returns>Logger for the specified type;</returns>
        /// <remarks>
        /// MUST ALWAYS return a logger. Return <see cref="NullLogWriter"/> if no logging
        /// should be used.
        /// </remarks>
        public ILogWriter CreateLogger(Type type)
        {
            return new NullLogWriter();
        }
    }
}