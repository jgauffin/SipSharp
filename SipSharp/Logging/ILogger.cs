using System;

namespace SipSharp.Logging
{
    /// <summary>
    /// Interface used to write to log files.
    /// </summary>
    /// <remarks>
    /// If you want to use the built in filtering mechanism, create a constructor
    /// which takes one parameter, a <see cref="ILogFilter"/>.
    /// </remarks>
    public interface ILogger
    {
        /// <summary>
        /// Write an entry that helps when debugging code.
        /// </summary>
        /// <param name="message">Log message</param>
        void Debug(string message);

        /// <summary>
        /// Write an entry that helps when debugging code.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="err">Exception being logged.</param>
        void Debug(string message, Exception err);


        /// <summary>
        /// Something went wrong, but the application do not need to die. The current thread/request
        /// cannot continue as expected.
        /// </summary>
        /// <param name="message">Log message</param>
        void Error(string message);

        /// <summary>
        /// Something went wrong, but the application do not need to die. The current thread/request
        /// cannot continue as expected.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="err">Exception being logged.</param>
        void Error(string message, Exception err);

        /// <summary>
        /// Something went very wrong, application might not recover.
        /// </summary>
        /// <param name="message">Log message</param>
        void Fatal(string message);

        /// <summary>
        /// Something went very wrong, application might not recover.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="err">Exception being logged.</param>
        void Fatal(string message, Exception err);

        /// <summary>
        /// Informational message, needed when helping customer to find a problem.
        /// </summary>
        /// <param name="message">Log message</param>
        void Info(string message);

        /// <summary>
        /// Informational message, needed when helping customer to find a problem.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="err">Exception being logged.</param>
        void Info(string message, Exception err);

        /// <summary>
        /// Write a entry needed when following through code during hard to find bugs.
        /// </summary>
        /// <param name="message">Log message</param>
        void Trace(string message);

        /// <summary>
        /// Write a entry needed when following through code during hard to find bugs.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="err">Exception being logged.</param>
        void Trace(string message, Exception err);

        /// <summary>
        /// Something is not as we expect, but the code can continue to run without any changes.
        /// </summary>
        /// <param name="message">Log message</param>
        void Warning(string message);

        /// <summary>
        /// Something is not as we expect, but the code can continue to run without any changes.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="err">Exception being logged.</param>
        void Warning(string message, Exception err);
    }
}