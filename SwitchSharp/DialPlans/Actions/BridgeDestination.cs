namespace SwitchSharp.DialPlans.Actions
{
    public interface IBridgeDestination
    {
    }

    public class UserDestination : IBridgeDestination
    {
        public UserDestination(int userId)
        {
            UserId = userId;
        }

        public int UserId { get; set; }
    }

    public class StringDestination : IBridgeDestination
    {
        public StringDestination(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
    }
}