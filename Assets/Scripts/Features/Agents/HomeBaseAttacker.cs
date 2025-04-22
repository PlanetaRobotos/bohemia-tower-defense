using System;
using Features.Health;
using Features.Health.Core;
using Infrastructure.Services.ApplicationObservers.Runtime;
using TowerDefense.Nodes;
using UnityEngine;
using Utils;

namespace Features.Agents
{
	/// <summary>
	///     A component that attacks a home base when an agent reaches it
	/// </summary>
	[RequireComponent(typeof(Agent))]
    public class HomeBaseAttacker : MonoBehaviour
    {
	    [Inject] private IUpdater Updater { get; }

	    /// <summary>
	    ///     How long the agent charges for before it attacks
	    ///     the home base
	    /// </summary>
	    public float homeBaseAttackChargeTime = 0.5f;

	    /// <summary>
	    ///     The DamageableBehaviour on the home base
	    /// </summary>
	    protected DamageableBehaviour m_FinalDestinationDamageableBehaviour;

	    /// <summary>
	    ///     Timer used to stall attack to the home base
	    /// </summary>
	    protected Timer m_HomeBaseAttackTimer;

	    /// <summary>
	    ///     If the agent has reached the Player Home Base and is charging an attack
	    /// </summary>
	    protected bool m_IsChargingHomeBaseAttack;

	    /// <summary>
	    ///     The agent component attached to this gameObject
	    /// </summary>
	    public Agent agent { get; protected set; }

	    /// <summary>
	    ///     Caches the attached Agent and subscribes to the destinationReached event
	    /// </summary>
	    protected virtual void Awake()
        {
            agent = GetComponent<Agent>();
            agent.destinationReached += OnDestinationReached;
            agent.died += OnDied;
        }

	    private void Start()
	    {
		    Updater.Subscribe(OnUpdate, 0);
	    }

	    /// <summary>
	    ///     Ticks the attack timer
	    /// </summary>
	    protected virtual void OnUpdate(float _)
        {
            // Update HomeBaseAttack Timer
            if (m_IsChargingHomeBaseAttack) m_HomeBaseAttackTimer.Tick(Time.deltaTime);
        }

	    /// <summary>
	    ///     Unsubscribes from the destinationReached event
	    /// </summary>
	    protected virtual void OnDestroy()
        {
            if (agent != null)
            {
                agent.destinationReached -= OnDestinationReached;
                agent.died -= OnDied;
            }
        }

	    /// <summary>
	    ///     Fired on completion of <see cref="m_HomeBaseAttackTimer" />
	    ///     Applies damage to the homebase
	    /// </summary>
	    protected void AttackHomeBase()
        {
            m_IsChargingHomeBaseAttack = false;
            var damager = GetComponent<Damager>();
            if (damager != null)
                m_FinalDestinationDamageableBehaviour.TakeDamage(damager.damage, transform.position,
                    agent.configuration.alignmentProvider);
            agent.Remove();
        }

	    /// <summary>
	    ///     Stops the attack on the home base
	    /// </summary>
	    private void OnDied(DamageableBehaviour damageableBehaviour)
        {
            m_IsChargingHomeBaseAttack = false;
        }

	    /// <summary>
	    ///     Fired then the agent reached its final node,
	    ///     Starts the attack timer
	    /// </summary>
	    /// <param name="homeBase"></param>
	    private void OnDestinationReached(GameNode homeBase)
        {
            m_FinalDestinationDamageableBehaviour = homeBase.GetComponent<DamageableBehaviour>();
            // start timer 
            if (m_HomeBaseAttackTimer == null)
                m_HomeBaseAttackTimer = new Timer(homeBaseAttackChargeTime, AttackHomeBase);
            else
                m_HomeBaseAttackTimer.Reset();
            m_IsChargingHomeBaseAttack = true;
        }
    }
}