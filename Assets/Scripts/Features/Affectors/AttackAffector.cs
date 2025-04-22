using Features.Health;
using Features.Health.Core;
using Features.Towers;
using Features.Towers.Projectiles;
using TowerDefense.Targetting;
using UnityEngine;

namespace Features.Affectors
{
	/// <summary>
	///     The common effect for handling firing projectiles to attack
	///     Requires an ILauncher but it is not automatically added
	///     Add an ILauncher implementation to this GameObject before you add this script
	/// </summary>
	[RequireComponent(typeof(ILauncher))]
    public class AttackAffector : Affector, ITowerRadiusProvider
    {
	    /// <summary>
	    ///     The projectile used to attack
	    /// </summary>
	    public GameObject projectile;

	    /// <summary>
	    ///     The list of points to launch the projectiles from
	    /// </summary>
	    public Transform[] projectilePoints;

	    /// <summary>
	    ///     The reference to the center point where the tower will search from
	    /// </summary>
	    public Transform epicenter;

	    /// <summary>
	    ///     Configuration for when the tower does splash damage
	    /// </summary>
	    public bool isMultiAttack;


	    /// <summary>
	    ///     The fire rate in fires-per-second
	    /// </summary>
	    public float fireRate;

	    /// <summary>
	    ///     Gets the targetter
	    /// </summary>
	    public Targetter towerTargetter;

	    /// <summary>
	    ///     Color of effect radius visualization
	    /// </summary>
	    public Color radiusEffectColor;

	    /// <summary>
	    ///     Fire condition
	    /// </summary>
	    public Filter fireCondition;

	    /// <summary>
	    ///     The time before firing is possible
	    /// </summary>
	    protected float m_FireTimer;

	    /// <summary>
	    ///     The reference to the attached launcher
	    /// </summary>
	    protected ILauncher m_Launcher;

	    /// <summary>
	    ///     Reference to the current tracked enemy
	    /// </summary>
	    protected Targetable m_TrackingEnemy;

	    /// <summary>
	    ///     Search condition
	    /// </summary>
	    public Filter searchCondition;

	    /// <summary>
	    ///     Gets the search rate from the targetter
	    /// </summary>
	    public float searchRate
        {
            get => towerTargetter.searchRate;
            set => towerTargetter.searchRate = value;
        }

	    /// <summary>
	    ///     Gets the targetable
	    /// </summary>
	    public Targetable trackingEnemy => m_TrackingEnemy;

        public Damager damagerProjectile => projectile == null ? null : projectile.GetComponent<Damager>();

        /// <summary>
        ///     Update the timers
        /// </summary>
        protected virtual void Update()
        {
            m_FireTimer -= Time.deltaTime;
            if (trackingEnemy != null && m_FireTimer <= 0.0f)
            {
                OnFireTimer();
                m_FireTimer = 1 / fireRate;
            }
        }

        private void OnDestroy()
        {
            towerTargetter.acquiredTarget -= OnAcquiredTarget;
            towerTargetter.lostTarget -= OnLostTarget;
        }

#if UNITY_EDITOR
	    /// <summary>
	    ///     Draws the search area
	    /// </summary>
	    private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(epicenter.position, towerTargetter.effectRadius);
        }
#endif

	    /// <summary>
	    ///     Gets or sets the attack radius
	    /// </summary>
	    public float effectRadius => towerTargetter.effectRadius;

        public Color effectColor => radiusEffectColor;

        public Targetter targetter => towerTargetter;

        /// <summary>
        ///     Initializes the attack affector
        /// </summary>
        public override void Initialize(IAlignmentProvider affectorAlignment)
        {
            Initialize(affectorAlignment, -1);
        }

        /// <summary>
        ///     Initialises the  attack affector with a layer mask
        /// </summary>
        public override void Initialize(IAlignmentProvider affectorAlignment, LayerMask mask)
        {
            base.Initialize(affectorAlignment, mask);
            SetUpTimers();

            towerTargetter.ResetTargetter();
            towerTargetter.alignment = affectorAlignment;
            towerTargetter.acquiredTarget += OnAcquiredTarget;
            towerTargetter.lostTarget += OnLostTarget;
        }

        private void OnLostTarget()
        {
            m_TrackingEnemy = null;
        }

        private void OnAcquiredTarget(Targetable acquiredTarget)
        {
            m_TrackingEnemy = acquiredTarget;
        }

        /// <summary>
        ///     Returns the total projectile damage
        /// </summary>
        public float GetProjectileDamage()
        {
            var splash = projectile.GetComponent<SplashDamager>();
            var splashDamage = splash != null ? splash.damage : 0;
            return damagerProjectile.damage + splashDamage;
        }

        /// <summary>
        ///     Initialise the RepeatingTimer
        /// </summary>
        protected virtual void SetUpTimers()
        {
            m_FireTimer = 1 / fireRate;
            m_Launcher = GetComponent<ILauncher>();
        }

        /// <summary>
        ///     Fired at every poll of the fire rate timer
        /// </summary>
        protected virtual void OnFireTimer()
        {
            if (fireCondition != null)
                if (!fireCondition())
                    return;
            FireProjectile();
        }

        /// <summary>
        ///     Common logic when attacking
        /// </summary>
        protected virtual void FireProjectile()
        {
            if (m_TrackingEnemy == null) return;

            if (isMultiAttack)
            {
                var enemies = towerTargetter.GetAllTargets();
                m_Launcher.Launch(enemies, projectile, projectilePoints);
            }
            else
            {
                m_Launcher.Launch(m_TrackingEnemy, damagerProjectile.gameObject, projectilePoints);
            }
        }

        /// <summary>
        ///     A delegate to compare distances of components
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        protected virtual int ByDistance(Targetable first, Targetable second)
        {
            var firstSqrMagnitude = Vector3.SqrMagnitude(first.position - epicenter.position);
            var secondSqrMagnitude = Vector3.SqrMagnitude(second.position - epicenter.position);
            return firstSqrMagnitude.CompareTo(secondSqrMagnitude);
        }
    }

	/// <summary>
	///     A delegate for boolean calculation logic
	/// </summary>
	public delegate bool Filter();
}