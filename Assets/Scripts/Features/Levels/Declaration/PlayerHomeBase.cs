using System.Collections.Generic;
using Features.Agents;
using Features.Health.Core;
using UnityEngine;

namespace Features.Levels.Declaration
{
	/// <summary>
	///     A class representing the home base that players must defend
	/// </summary>
	public class PlayerHomeBase : DamageableBehaviour
    {
	    /// <summary>
	    ///     The current Agents within the home base attack zone
	    /// </summary>
	    protected List<Agent> m_CurrentAgentsInside = new();

	    /// <summary>
	    ///     Subscribes to damaged event
	    /// </summary>
	    protected virtual void Start()
        {
            configuration.damaged += OnDamaged;
        }

	    /// <summary>
	    ///     Unsubscribes to damaged event
	    /// </summary>
	    protected virtual void OnDestroy()
        {
            configuration.damaged -= OnDamaged;
        }

	    /// <summary>
	    ///     Adds triggered Agent to tracked Agents, subscribes to Agent's
	    ///     removed event and plays pfx
	    /// </summary>
	    /// <param name="other">Triggered collider</param>
	    private void OnTriggerEnter(Collider other)
        {
            var homeBaseAttacker = other.GetComponent<HomeBaseAttacker>();
            if (homeBaseAttacker == null) return;
            m_CurrentAgentsInside.Add(homeBaseAttacker.agent);
            homeBaseAttacker.agent.removed += OnAgentRemoved;
        }

	    /// <summary>
	    ///     If the entity that has entered the collider
	    ///     has an <see cref="Agent" /> component on it
	    /// </summary>
	    private void OnTriggerExit(Collider other)
        {
            var homeBaseAttacker = other.GetComponent<HomeBaseAttacker>();
            if (homeBaseAttacker == null) return;
            RemoveTarget(homeBaseAttacker.agent);
        }

	    /// <summary>
	    ///     Plays <see cref="attackPfx" /> if assigned
	    /// </summary>
	    protected virtual void OnDamaged(HealthChangeInfo obj)
        {
            // Debug.Log($"Damaged: {name}");
        }

	    /// <summary>
	    ///     Removes Agent from tracked <see cref="m_CurrentAgentsInside" />
	    /// </summary>
	    private void OnAgentRemoved(DamageableBehaviour targetable)
        {
            targetable.removed -= OnAgentRemoved;
            var attackingAgent = targetable as Agent;
            RemoveTarget(attackingAgent);
        }

	    /// <summary>
	    ///     Removes <paramref name="agent" /> from <see cref="m_CurrentAgentsInside" /> and stops pfx
	    ///     if there are no more <see cref="Agent" />s
	    /// </summary>
	    /// <param name="agent">
	    ///     The agent to remove
	    /// </param>
	    private void RemoveTarget(Agent agent)
        {
            if (agent == null) return;
            m_CurrentAgentsInside.Remove(agent);
        }
    }
}