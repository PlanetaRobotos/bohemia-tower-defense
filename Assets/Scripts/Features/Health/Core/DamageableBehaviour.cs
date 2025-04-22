using System;
using Features.Levels.Implementation;
using UnityEngine;

namespace Features.Health.Core
{
	/// <summary>
	///     Abstract class for any MonoBehaviours that can take damage
	/// </summary>
	public class DamageableBehaviour : MonoBehaviour
    {
	    /// <summary>
	    ///     The Damageable object
	    /// </summary>
	    public Damageable configuration;

        [Inject] protected LevelManager LevelManager { get; }

        /// <summary>
        ///     Gets whether this <see cref="DamageableBehaviour" /> is dead.
        /// </summary>
        /// <value>True if dead</value>
        public bool isDead => configuration.isDead;

        /// <summary>
        ///     The position of the transform
        /// </summary>
        public virtual Vector3 position => transform.position;

        protected virtual void Awake()
        {
            configuration.Init();
            configuration.died += OnConfigurationDied;
        }

        /// <summary>
        ///     Occurs when damage is taken
        /// </summary>
        public event Action<HitInfo> hit;

        /// <summary>
        ///     Event that is fired when this instance is removed, such as when pooled or destroyed
        /// </summary>
        public event Action<DamageableBehaviour> removed;

        /// <summary>
        ///     Event that is fired when this instance is killed
        /// </summary>
        public event Action<DamageableBehaviour> died;


        /// <summary>
        ///     Takes the damage and also provides a position for the damage being dealt
        /// </summary>
        /// <param name="damageValue">Damage value.</param>
        /// <param name="damagePoint">Damage point.</param>
        /// <param name="alignment">Alignment value</param>
        public virtual void TakeDamage(float damageValue, Vector3 damagePoint, IAlignmentProvider alignment)
        {
            HealthChangeInfo info;
            configuration.TakeDamage(damageValue, alignment, out info);
            var damageInfo = new HitInfo(info, damagePoint);
            if (hit != null) hit(damageInfo);
        }

        /// <summary>
        ///     Kills this damageable
        /// </summary>
        protected virtual void Kill()
        {
            HealthChangeInfo healthChangeInfo;
            configuration.TakeDamage(configuration.currentHealth, null, out healthChangeInfo);
        }


        /// <summary>
        ///     Removes this damageable without killing it
        /// </summary>
        public virtual void Remove()
        {
            // Set health to zero so that this behaviour appears to be dead. This will not fire death events
            configuration.SetHealth(0);
            OnRemoved();
        }

        /// <summary>
        ///     Fires kill events
        /// </summary>
        private void OnDeath()
        {
            if (died != null) died(this);
        }

        /// <summary>
        ///     Fires the removed event
        /// </summary>
        private void OnRemoved()
        {
            if (removed != null) removed(this);
        }

        /// <summary>
        ///     Event fired when Damageable takes critical damage
        /// </summary>
        private void OnConfigurationDied(HealthChangeInfo changeInfo)
        {
            OnDeath();
            Remove();
        }
    }
}