namespace Conditions
{
    public interface ICondition
    {
        bool IsPassed();
        float GetProgress();
    }
}