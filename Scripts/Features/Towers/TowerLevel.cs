using System.Collections.Generic;
using Features.Affectors;
using Features.Health.Core;
using Features.Spawning;
using Features.Towers.Data;
using UnityEngine;

namespace Features.Towers
{
	/// <summary>
	///     An individual level of a tower
	/// </summary>
	[DisallowMultipleComponent]
    public class TowerLevel : MonoBehaviour, ISerializationCallbackReceiver
    {
	    /// <summary>
	    ///     The prefab for communicating placement in the scene
	    /// </summary>
	    public TowerPlacementGhost towerGhostPrefab;

	    /// <summary>
	    ///     Build effect gameObject to instantiate on start
	    /// </summary>
	    public GameObject buildEffectPrefab;

	    /// <summary>
	    ///     Reference to scriptable object with level data on it
	    /// </summary>
	    public TowerLevelData levelData;

	    /// <summary>
	    ///     The list of effects attached to the tower
	    /// </summary>
	    private Affector[] m_Affectors;

	    /// <summary>
	    ///     The parent tower controller of this tower
	    /// </summary>
	    protected Tower m_ParentTower;

	    /// <summary>
	    ///     Gets the list of effects attached to the tower
	    /// </summary>
	    protected Affector[] Affectors
        {
            get
            {
                if (m_Affectors == null) m_Affectors = GetComponentsInChildren<Affector>();
                return m_Affectors;
            }
        }

	    /// <summary>
	    ///     The physics layer mask that the tower searches on
	    /// </summary>
	    public LayerMask mask { get; protected set; }

	    /// <summary>
	    ///     Gets the cost value
	    /// </summary>
	    public int cost => levelData.cost;

	    /// <summary>
	    ///     Gets the sell value
	    /// </summary>
	    public int sell => levelData.sell;

	    /// <summary>
	    ///     Gets the max health
	    /// </summary>
	    public int maxHealth => levelData.maxHealth;

	    /// <summary>
	    ///     Gets the starting health
	    /// </summary>
	    public int startingHealth => levelData.startingHealth;

	    /// <summary>
	    ///     Gets the tower description
	    /// </summary>
	    public string description => levelData.description;

	    /// <summary>
	    ///     Gets the tower description
	    /// </summary>
	    public string upgradeDescription => levelData.upgradeDescription;

	    /// <summary>
	    ///     Insntiate the build particle effect object
	    /// </summary>
	    private void Start()
        {
            if (buildEffectPrefab != null) Instantiate(buildEffectPrefab, transform);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            // Setting this member to null is required because we are setting this value on a prefab which will 
            // persists post run in editor, so we null this member to ensure it is repopulated every run
            m_Affectors = null;
        }

        /// <summary>
        ///     Initialises the Effects attached to this object
        /// </summary>
        public virtual void Initialize(Tower tower, LayerMask enemyMask, IAlignmentProvider alignment)
        {
            mask = enemyMask;

            foreach (var effect in Affectors) effect.Initialize(alignment, mask);
            m_ParentTower = tower;
        }

        /// <summary>
        ///     A method for activating or deactivating the attached <see cref="Affectors" />
        /// </summary>
        public void SetAffectorState(bool state)
        {
            foreach (var affector in Affectors)
                if (affector != null)
                    affector.enabled = state;
        }

        /// <summary>
        ///     Returns a list of affectors that implement ITowerRadiusVisualizer
        /// </summary>
        /// <returns>ITowerRadiusVisualizers of tower</returns>
        public List<ITowerRadiusProvider> GetRadiusVisualizers()
        {
            var visualizers = new List<ITowerRadiusProvider>();
            foreach (var affector in Affectors)
            {
                var visualizer = affector as ITowerRadiusProvider;
                if (visualizer != null) visualizers.Add(visualizer);
            }

            return visualizers;
        }

        /// <summary>
        ///     Returns the dps of the tower
        /// </summary>
        /// <returns>The dps of the tower</returns>
        public float GetTowerDps()
        {
            float dps = 0;
            foreach (var affector in Affectors)
            {
                var attack = affector as AttackAffector;
                if (attack != null && attack.damagerProjectile != null)
                    dps += attack.GetProjectileDamage() * attack.fireRate;
            }

            return dps;
        }

        public void Kill()
        {
            m_ParentTower.KillTower();
        }
    }
}