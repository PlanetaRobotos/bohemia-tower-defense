using System;

namespace Galaxy4Games.Utils
{
    [Serializable]
    public struct MinMaxValueInt
    {
        public int min;
        public int max;

        public MinMaxValueInt(int minValue, int maxValue)
        {
            min = minValue;
            max = maxValue;
        }

        public int GetRandom()
        {
            return UnityEngine.Random.Range(min, max + 1);
        }
    }
}