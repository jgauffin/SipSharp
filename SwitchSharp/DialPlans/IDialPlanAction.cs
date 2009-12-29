namespace SwitchSharp.DialPlans
{
    /// <summary>
    /// Action that can be invoked in the dial plan.
    /// </summary>
    public interface IDialPlanAction
    {
        /// <summary>
        /// Gets if more actions can be run after this one.
        /// </summary>
        bool IsTerminatingAction { get; }
    }
}