using System.Collections.Generic;
using Features.Affectors;
using Features.Health;
using Features.Levels.Data;
using Features.Levels.Implementation;
using Features.Projectiles;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Infrastructure.Services.ApplicationObservers.Runtime;

namespace Features.Towers.TowerLaunchers
{
	/// <summary>
	///     Rapid fire launcher, launchers a homing projectile
	/// </summary>
	public class SuperTowerLauncher : HomingLauncher
    {
        [Inject] private IUpdater Updater { get; }
        [Inject] private LevelManager LevelManager { get; }

	    /// <summary>
	    ///     How long the tower will stay active
	    /// </summary>
	    public float towerLifeSpan = 10;

	    /// <summary>
	    ///     Angle, in degrees, to rotate, on the x axis, the fire vector by
	    /// </summary>
	    public float fireVectorXRotationAdjustment = 45.0f;

	    /// <summary>
	    ///     Fires when the max amount of projectiles has been reached
	    /// </summary>
	    public UnityEvent death;

	    /// <summary>
	    ///     Timer to invoke a unity event when it elapses
	    /// </summary>
	    protected Timer m_LifeTimer;

        /// <summary>
        ///     Tick the timer
        /// </summary>
        protected virtual void OnUpdate(float _)
        {
            if (m_LifeTimer == null) return;
            m_LifeTimer.Tick(Time.deltaTime);
        }

        protected virtual void Start()
        {
            Updater.Subscribe(OnUpdate, 0);
        }

        protected virtual void OnDestroy()
        {
            Updater?.Unsubscribe(OnUpdate);
        }

        /// <summary>
        ///     Subscribes to Level Manager onStateChanged
        ///     If waves have already begun, then begin death timer
        /// </summary>
        protected virtual void OnEnable()
        {
            if (!LevelManager) return;
            var currentState = LevelManager.levelState;
            if (currentState == LevelState.SpawningEnemies || currentState == LevelState.AllEnemiesSpawned)
                m_LifeTimer = new Timer(towerLifeSpan, OnLifeTimerElapsed);
            else
                LevelManager.levelStateChanged += OnLevelStateChanged;
        }

        /// <summary>
        ///     Unsubscribe from Level Manager onStateChanged
        /// </summary>
        protected virtual void OnDisable()
        {
            if (LevelManager) LevelManager.levelStateChanged -= OnLevelStateChanged;
        }

        /// <summary>
        ///     Finds a random enemy in a list and fires from a random point
        /// </summary>
        /// <param name="enemies">
        ///     The list of enemies to sample from
        /// </param>
        /// <param name="attack">
        ///     The object used to attack
        /// </param>
        /// <param name="firingPoints"></param>
        public override void Launch(List<Targetable> enemies, GameObject attack, Transform[] firingPoints)
        {
            var poolable = Poolable.TryGetPoolable<Poolable>(attack);
            if (poolable == null) return;
            var enemy = enemies[Random.Range(0, enemies.Count)];
            var firingPoint = GetRandomTransform(firingPoints);
            Launch(enemy, poolable.gameObject, firingPoint);
        }

        public override void Launch(Targetable enemy, GameObject attack, Transform firingPoint)
        {
            var homingMissile = attack.GetComponent<HomingLinearProjectile>();
            if (homingMissile == null)
            {
                Debug.LogError("No HomingLinearProjectile attached to attack object");
                return;
            }

            var startingPoint = firingPoint.position;
            var targetPoint = Ballistics.CalculateLinearLeadingTargetPoint(
                startingPoint, enemy.position,
                enemy.velocity, homingMissile.startSpeed,
                homingMissile.acceleration);

            homingMissile.SetHomingTarget(enemy);

            var attackAffector = GetComponent<AttackAffector>();
            var direction = attackAffector.towerTargetter.turret.forward;

            var binormal = Vector3.Cross(direction, Vector3.up);
            var rotation = Quaternion.AngleAxis(fireVectorXRotationAdjustment, binormal);

            var adjustedFireVector = rotation * direction;

            homingMissile.FireInDirection(startingPoint, adjustedFireVector);
        }

        /// <summary>
        ///     Invoke the UnityEvent once the timer elapses
        /// </summary>
        protected void OnLifeTimerElapsed()
        {
            death.Invoke();
        }

        /// <summary>
        ///     Checks the current state, if within a valid state, start the death timer
        /// </summary>
        /// <param name="previousState">
        ///     The previous state the the LevelManager was in
        /// </param>
        /// <param name="currentState">
        ///     The current state the LevelManager was in
        /// </param>
        private void OnLevelStateChanged(LevelState previousState, LevelState currentState)
        {
            if (currentState == LevelState.SpawningEnemies || currentState == LevelState.AllEnemiesSpawned)
                m_LifeTimer = new Timer(towerLifeSpan, OnLifeTimerElapsed);
        }
    }
}