using UnityEngine;

namespace Conditions
{
    public abstract class BaseCountableCondition : BaseConditionSO
    {
        public int count;
        public CompareType compareType = CompareType.GreaterAndEqual;

        protected abstract int Counter { get; }

        public override bool IsPassed()
        {
            switch (compareType)
            {
                case CompareType.Equal:
                    return Counter == count;
                case CompareType.Greater:
                    return Counter > count;
                case CompareType.Less:
                    return Counter < count;
                case CompareType.NotEqual:
                    return Counter != count;
                case CompareType.GreaterAndEqual:
                    return Counter >= count;
                case CompareType.LessAndEqual:
                    return Counter <= count;
                default:
                    return false;
            }
        }

        public override float GetProgress()
        {
            var progress = (float)Counter / count;
            progress = Mathf.Clamp(progress, 0f, 1f);

            return progress;
        }

        public int GetProgressCount() => Counter;
    }

    public enum CompareType
    {
        Greater,
        Equal,
        Less,
        NotEqual,
        GreaterAndEqual,
        LessAndEqual
    }
}