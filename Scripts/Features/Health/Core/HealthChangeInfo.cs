using UnityEngine;

namespace Features.Health.Core
{
	/// <summary>
	///     Health change info - stores information about the health change
	/// </summary>
	public struct HealthChangeInfo
    {
        public Damageable damageable;

        public float oldHealth;

        public float newHealth;

        public IAlignmentProvider damageAlignment;

        public float healthDifference => newHealth - oldHealth;

        public float absHealthDifference => Mathf.Abs(healthDifference);
    }
}