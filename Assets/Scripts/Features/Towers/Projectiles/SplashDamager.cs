using Features.Health;
using Features.Health.Core;
using UnityEngine;

namespace Features.Towers.Projectiles
{
	/// <summary>
	///     Component that will apply splash damage on collision enter
	/// </summary>
	public class SplashDamager : MonoBehaviour
    {
        private static readonly Collider[] s_Enemies = new Collider[64];

        /// <summary>
        ///     The Area this projectile will attack in
        /// </summary>
        public float attackRange = 0.6f;

        /// <summary>
        ///     The amount of damage done, a percentage of the damager damage
        /// </summary>
        public float damageAmount;

        /// <summary>
        ///     The physics layer mask to search on
        /// </summary>
        public LayerMask mask = -1;

        /// <summary>
        ///     The alignment of the projectile
        /// </summary>
        public SerializableIAlignmentProvider alignment;

        public float damage => damageAmount;

        /// <summary>
        ///     Gets this damager's alignment
        /// </summary>
        public IAlignmentProvider alignmentProvider => alignment != null ? alignment.GetInterface() : null;

        /// <summary>
        ///     Searches for Targetables within a radius of <see cref="attackRange" />
        ///     and damages them if valid
        /// </summary>
        protected virtual void OnCollisionEnter(Collision other)
        {
            var number = Physics.OverlapSphereNonAlloc(transform.position, attackRange, s_Enemies, mask);
            for (var index = 0; index < number; index++)
            {
                var enemy = s_Enemies[index];
                var damageable = enemy.GetComponent<Targetable>();
                if (damageable == null) continue;
                damageable.TakeDamage(damageAmount, damageable.position, alignmentProvider);
            }
        }
    }
}