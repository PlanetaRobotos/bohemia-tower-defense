using System;
using Features.Economy;
using Features.Economy.Models;
using Features.Health.Core;
using Features.Levels.Data;
using Features.Levels.Declaration;
using Features.Towers.Data;
using Features.Waves;
using UnityEngine;
using Infrastructure.Services.ApplicationObservers.Runtime;

namespace Features.Levels.Implementation
{
	/// <summary>
	///     The level manager - handles the level states and tracks the player's currency
	/// </summary>
	[RequireComponent(typeof(WaveManager))]
    public class LevelManager : MonoBehaviour
    {
	    /// <summary>
	    ///     The configured level intro. If this is null the LevelManager will fall through to the gameplay state (i.e.
	    ///     SpawningEnemies)
	    /// </summary>
	    public LevelIntro intro;

	    /// <summary>
	    ///     The tower library for this level
	    /// </summary>
	    public TowerLibrary towerLibrary;

	    /// <summary>
	    ///     The currency that the player starts with
	    /// </summary>
	    public int startingCurrency;

	    /// <summary>
	    ///     The controller for gaining currency
	    /// </summary>
	    public CurrencyGainer currencyGainer;

	    /// <summary>
	    ///     Configuration for if the player gains currency even in pre-build phase
	    /// </summary>
	    [Header("Setting this will allow currency gain during the Intro and Pre-Build phase")]
        public bool alwaysGainCurrency;

	    /// <summary>
	    ///     The home bases that the player must defend
	    /// </summary>
	    public PlayerHomeBase[] homeBases;

        public Collider[] environmentColliders;

        /// <summary>
        ///     The attached wave manager
        /// </summary>
        public WaveManager waveManager { get; protected set; }

        /// <summary>
        ///     Number of enemies currently in the level
        /// </summary>
        public int numberOfEnemies { get; protected set; }

        /// <summary>
        ///     The current state of the level
        /// </summary>
        public LevelState levelState { get; protected set; }

        /// <summary>
        ///     The currency controller
        /// </summary>
        public Currency currency { get; protected set; }

        /// <summary>
        ///     Number of home bases left
        /// </summary>
        public int numberOfHomeBasesLeft { get; protected set; }

        /// <summary>
        ///     Starting number of home bases
        /// </summary>
        public int numberOfHomeBases { get; protected set; }

        /// <summary>
        ///     An accessor for the home bases
        /// </summary>
        public PlayerHomeBase[] playerHomeBases => homeBases;

        /// <summary>
        ///     If the game is over
        /// </summary>
        public bool isGameOver => levelState == LevelState.Win || levelState == LevelState.Lose;

        [Inject] private IUpdater Updater { get; }

        /// <summary>
        ///     Caches the attached wave manager and subscribes to the spawning completed event
        ///     Sets the level state to intro and ensures that the number of enemies is set to 0
        /// </summary>
        protected virtual void Awake()
        {
            waveManager = GetComponent<WaveManager>();
            currency = new Currency(startingCurrency);
            numberOfHomeBases = homeBases.Length;
            numberOfHomeBasesLeft = numberOfHomeBases;

            foreach (var homeBase in homeBases) homeBase.died += OnHomeBaseDestroyed;

            if (intro != null)
            {
                ChangeLevelState(LevelState.Intro);
                intro.introCompleted += IntroCompleted;
            }
            else
            {
                ChangeLevelState(LevelState.SpawningEnemies);
            }
        }

        private void Start()
        {
            Updater.Subscribe(OnUpdate, 0);
        }

        /// <summary>
        ///     Updates the currency gain controller
        /// </summary>
        protected virtual void OnUpdate(float _)
        {
            if (alwaysGainCurrency ||
                (!alwaysGainCurrency && levelState != LevelState.Building && levelState != LevelState.Intro))
                currencyGainer.Tick(Time.deltaTime);
        }

        /// <summary>
        ///     Unsubscribes from events
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (intro != null) intro.introCompleted -= IntroCompleted;

            foreach (var homeBase in homeBases)
                if (homeBase != null)
                    homeBase.died -= OnHomeBaseDestroyed;

            Updater?.Unsubscribe(OnUpdate);
        }

        /// <summary>
        ///     Fired when all the waves are done and there are no more enemies left
        /// </summary>
        public event Action levelCompleted;

        /// <summary>
        ///     Fired when the level is failed
        /// </summary>
        public event Action levelFailed;

        /// <summary>
        ///     Fired when the level state changes
        /// </summary>
        public event Action<LevelState, LevelState> levelStateChanged;

        /// <summary>
        ///     Fired when the number of enemies changes
        /// </summary>
        public event Action<int> numberOfEnemiesChanged;

        /// <summary>
        ///     Fired when a home base is destroyed
        /// </summary>
        public event Action homeBaseDestroyed;

        /// <summary>
        ///     Increments the number of enemies. Called on Agent spawn
        /// </summary>
        public virtual void IncrementNumberOfEnemies()
        {
            numberOfEnemies++;
            SafelyCallNumberOfEnemiesChanged();
        }

        /// <summary>
        ///     Returns the sum of all HomeBases' health
        /// </summary>
        public float GetAllHomeBasesHealth()
        {
            var health = 0.0f;
            foreach (var homebase in homeBases) health += homebase.configuration.currentHealth;
            return health;
        }

        /// <summary>
        ///     Decrements the number of enemies. Called on Agent death
        /// </summary>
        public virtual void DecrementNumberOfEnemies()
        {
            numberOfEnemies--;
            SafelyCallNumberOfEnemiesChanged();
            if (numberOfEnemies < 0)
            {
                Debug.LogError("[LEVEL] There should never be a negative number of enemies. Something broke!");
                numberOfEnemies = 0;
            }

            if (numberOfEnemies == 0 && levelState == LevelState.AllEnemiesSpawned) ChangeLevelState(LevelState.Win);
        }

        /// <summary>
        ///     Completes building phase, setting state to spawn enemies
        /// </summary>
        public virtual void BuildingCompleted()
        {
            ChangeLevelState(LevelState.SpawningEnemies);
        }

        /// <summary>
        ///     Fired when Intro is completed or immediately, if no intro is specified
        /// </summary>
        protected virtual void IntroCompleted()
        {
            ChangeLevelState(LevelState.Building);
        }

        /// <summary>
        ///     Fired when the WaveManager has finished spawning enemies
        /// </summary>
        protected virtual void OnSpawningCompleted()
        {
            ChangeLevelState(LevelState.AllEnemiesSpawned);
        }

        /// <summary>
        ///     Changes the state and broadcasts the event
        /// </summary>
        /// <param name="newState">The new state to transitioned to</param>
        protected virtual void ChangeLevelState(LevelState newState)
        {
            // If the state hasn't changed then return
            if (levelState == newState) return;

            var oldState = levelState;
            levelState = newState;
            if (levelStateChanged != null) levelStateChanged(oldState, newState);

            switch (newState)
            {
                case LevelState.SpawningEnemies:
                    waveManager.StartWaves();
                    break;
                case LevelState.AllEnemiesSpawned:
                    // Win immediately if all enemies are already dead
                    if (numberOfEnemies == 0) ChangeLevelState(LevelState.Win);
                    break;
                case LevelState.Lose:
                    SafelyCallLevelFailed();
                    break;
                case LevelState.Win:
                    SafelyCallLevelCompleted();
                    break;
            }
        }

        /// <summary>
        ///     Fired when a home base is destroyed
        /// </summary>
        protected virtual void OnHomeBaseDestroyed(DamageableBehaviour homeBase)
        {
            // Decrement the number of home bases
            numberOfHomeBasesLeft--;

            // Call the destroyed event
            if (homeBaseDestroyed != null) homeBaseDestroyed();

            // If there are no home bases left and the level is not over then set the level to lost
            if (numberOfHomeBasesLeft == 0 && !isGameOver) ChangeLevelState(LevelState.Lose);
        }

        /// <summary>
        ///     Calls the <see cref="levelCompleted" /> event
        /// </summary>
        protected virtual void SafelyCallLevelCompleted()
        {
            if (levelCompleted != null) levelCompleted();
        }

        /// <summary>
        ///     Calls the <see cref="numberOfEnemiesChanged" /> event
        /// </summary>
        protected virtual void SafelyCallNumberOfEnemiesChanged()
        {
            if (numberOfEnemiesChanged != null) numberOfEnemiesChanged(numberOfEnemies);
        }

        /// <summary>
        ///     Calls the <see cref="levelFailed" /> event
        /// </summary>
        protected virtual void SafelyCallLevelFailed()
        {
            if (levelFailed != null) levelFailed();
        }
    }
}