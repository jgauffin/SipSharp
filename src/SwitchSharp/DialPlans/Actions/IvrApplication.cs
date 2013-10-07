using System;

namespace SwitchSharp.DialPlans.Actions
{
    /// <summary>
    /// Execute a IVR application.
    /// </summary>
    public class IvrApplication : IAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IvrApplication"/> class.
        /// </summary>
        /// <param name="scriptName">Name of the application.</param>
        /// <param name="arguments">Arguments used in the application.</param>
        /// <exception cref="ArgumentNullException"><c>scriptName</c> is <c>null</c>.</exception>
        public IvrApplication(string scriptName, params string[] arguments)
        {
            if (scriptName == null)
                throw new ArgumentNullException("scriptName");

            ScriptName = scriptName;
            Arguments = arguments;
        }

        /// <summary>
        /// Name of the application
        /// </summary>
        public string ScriptName { get; private set; }

        /// <summary>
        /// Arguments used in the application
        /// </summary>
        public string[] Arguments { get; private set; }

    }
}
