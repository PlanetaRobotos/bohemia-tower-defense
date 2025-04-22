using System;

namespace Galaxy4Games.Utils
{
    [Serializable]
    public struct MinMaxValue
    {
        public float min;
        public float max;

        public MinMaxValue(float minValue, float maxValue)
        {
            min = minValue;
            max = maxValue;
        }

        public float GetRandom() => UnityEngine.Random.Range(min, max);
    }
}