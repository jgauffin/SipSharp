using System;
using System.Diagnostics;
using System.Text;

namespace SipSharp.Logging
{
    /// <summary>
    /// This class writes to the console. 
    /// </summary>
    /// <remarks>
    /// It colors the output depending on the log level 
    /// and includes a 3-level stack trace (in debug mode)
    /// </remarks>
    /// <seealso cref="ILogger"/>
    public sealed class ConsoleLogger : ILogger
    {
        private readonly ILogFilter _filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        /// <param name="loggingType">Type being logged.</param>
        /// <param name="filter">Filter used to decide which entries to log.</param>
        public ConsoleLogger(Type loggingType, ILogFilter filter)
        {
            _filter = filter;
            LoggingType = loggingType;
        }

        /// <summary>
        /// Gets or sets type that the logger is for
        /// </summary>
        public Type LoggingType { get; private set; }

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

        /// <summary>
        /// Write an entry
        /// </summary>
        /// <param name="level">Importance of the log message</param>
        /// <param name="message">The message.</param>
        public void Write(LogLevel level, string message)
        {
            if (_filter != null && !_filter.CanLog(level, LoggingType))
                return;

            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString());
            sb.Append(" ");
            sb.Append(level.ToString().PadRight(10));
            sb.Append(" | ");
#if DEBUG
            var trace = new StackTrace();
            StackFrame[] frames = trace.GetFrames();
            if (frames != null)
            {
                int endFrame = frames.Length > 4 ? 4 : frames.Length;
                int startFrame = frames.Length > 1 ? 2 : 0;
                for (int i = endFrame; i >= startFrame; --i)
                {
                    sb.Append(frames[i].GetMethod().ReflectedType.Name + "." + frames[i].GetMethod().Name);
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
        /// Write an entry
        /// </summary>
        /// <param name="level">Importance of the log message</param>
        /// <param name="message">The message.</param>
        public void Write(LogLevel level, string message, Exception exception)
        {
            if (_filter != null && !_filter.CanLog(level, LoggingType))
                return;

            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString());
            sb.Append(" ");
            sb.Append(level.ToString().PadRight(10));
            sb.Append(" | ");
#if DEBUG
            var trace = new StackTrace();
            StackFrame[] frames = trace.GetFrames();
            if (frames != null)
            {
                int endFrame = frames.Length > 4 ? 4 : frames.Length;
                int startFrame = frames.Length > 1 ? 2 : 0;
                for (int i = endFrame; i >= startFrame; --i)
                {
                    sb.Append(frames[i].GetMethod().ReflectedType.Name + "." + frames[i].GetMethod().Name);
                    sb.Append(" -> ");
                }
            }
#else
            sb.Append(System.Reflection.MethodBase.GetCurrentMethod().Name);
            sb.Append(" | ");
#endif
            sb.Append(message);
            sb.AppendLine();
            sb.Append(exception.ToString());

            Console.ForegroundColor = GetColor(level);
            Console.WriteLine(sb.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        #region ILogger Members

        /// <summary>
        /// Write an entry that helps when debugging code.
        /// </summary>
        /// <param name="message">Log message</param>
        public void Debug(string message)
        {
            Write(LogLevel.Debug, message);
        }

        /// <summary>
        /// Write an entry that helps when debugging code.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="err">Exception being logged.</param>
        public void Debug(string message, Exception err)
        {
            Write(LogLevel.Debug, message, err);
        }

        /// <summary>
        /// Write a entry needed when following through code during hard to find bugs.
        /// </summary>
        /// <param name="message">Log message</param>
        public void Trace(string message)
        {
            Write(LogLevel.Trace, message);
        }

        /// <summary>
        /// Write a entry needed when following through code during hard to find bugs.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="err">Exception being logged.</param>
        public void Trace(string message, Exception err)
        {
            Write(LogLevel.Trace, message, err);
        }

        /// <summary>
        /// Informational message, needed when helping customer to find a problem.
        /// </summary>
        /// <param name="message">Log message</param>
        public void Info(string message)
        {
            Write(LogLevel.Info, message);
        }

        /// <summary>
        /// Informational message, needed when helping customer to find a problem.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="err">Exception being logged.</param>
        public void Info(string message, Exception err)
        {
            Write(LogLevel.Info, message, err);
        }

        /// <summary>
        /// Something is not as we expect, but the code can continue to run without any changes.
        /// </summary>
        /// <param name="message">Log message</param>
        public void Warning(string message)
        {
            Write(LogLevel.Warning, message);
        }

        /// <summary>
        /// Something is not as we expect, but the code can continue to run without any changes.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="err">Exception being logged.</param>
        public void Warning(string message, Exception err)
        {
            Write(LogLevel.Warning, message, err);
        }

        /// <summary>
        /// Something went wrong, but the application do not need to die. The current thread/request
        /// cannot continue as expected.
        /// </summary>
        /// <param name="message">Log message</param>
        public void Error(string message)
        {
            Write(LogLevel.Error, message);
        }

        /// <summary>
        /// Something went wrong, but the application do not need to die. The current thread/request
        /// cannot continue as expected.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="err">Exception being logged.</param>
        public void Error(string message, Exception err)
        {
            Write(LogLevel.Error, message, err);
        }

        /// <summary>
        /// Something went very wrong, application might not recover.
        /// </summary>
        /// <param name="message">Log message</param>
        public void Fatal(string message)
        {
            Write(LogLevel.Fatal, message);
        }

        /// <summary>
        /// Something went very wrong, application might not recover.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="err">Exception being logged.</param>
        public void Fatal(string message, Exception err)
        {
            Write(LogLevel.Fatal, message, err);
        }

        #endregion
    }
}