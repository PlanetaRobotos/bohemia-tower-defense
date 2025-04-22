using Features.Agents;
using Features.Towers;
using TowerDefense.Targetting;
using UnityEngine;

namespace Features.Affectors
{
	/// <summary>
	///     Abstract class that is used to apply <see cref="AgentEffect" />s to <see cref="Agent" />s
	/// </summary>
	[RequireComponent(typeof(Targetter))]
    public abstract class PassiveAffector : Affector, ITowerRadiusProvider
    {
	    /// <summary>
	    ///     Color of effect radius visualization
	    /// </summary>
	    public Color radiusEffectColor;

        public Targetter towerTargetter;

        /// <summary>
        ///     Gets or sets the attack radius
        /// </summary>
        public float effectRadius => towerTargetter.effectRadius;

        /// <summary>
        ///     Gets the color used for effect radius visualisation
        /// </summary>
        public Color effectColor => radiusEffectColor;

        /// <summary>
        ///     Gets the targetter
        /// </summary>
        public Targetter targetter => towerTargetter;
    }
}