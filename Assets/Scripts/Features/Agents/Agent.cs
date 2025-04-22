using System;
using Features.Affectors;
using Features.Health;
using TowerDefense.Nodes;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Features.Agents
{
	/// <summary>
	///     An agent will follow a path of nodes
	/// </summary>
	[RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AttackAffector))]
    public abstract class Agent : Targetable
    {
	    /// <summary>
	    ///     A means of keeping track of the agent along its path
	    /// </summary>
	    public enum State
        {
	        /// <summary>
	        ///     When the agent is on a path that is not blocked
	        /// </summary>
	        OnCompletePath,

	        /// <summary>
	        ///     When the agent is on a path is blocked
	        /// </summary>
	        OnPartialPath,

	        /// <summary>
	        ///     When the agent has reached the end of a blocked path
	        /// </summary>
	        Attacking,

	        /// <summary>
	        ///     For flying agents, when they move over obstacles
	        /// </summary>
	        PushingThrough,

	        /// <summary>
	        ///     When the agent has completed their path
	        /// </summary>
	        PathComplete
        }

	    /// <summary>
	    ///     Position offset for an applied affect
	    /// </summary>
	    public Vector3 appliedEffectOffset = Vector3.zero;

	    /// <summary>
	    ///     Scale adjustment for an applied affect
	    /// </summary>
	    public float appliedEffectScale = 1;

	    /// <summary>
	    ///     The Current node that the agent must navigate to
	    /// </summary>
	    protected GameNode m_CurrentNode;

	    /// <summary>
	    ///     Stores the Destination to the next node so we don't need to get new random positions every time
	    /// </summary>
	    protected Vector3 m_Destination;

	    /// <summary>
	    ///     The NavMeshAgent component attached to this
	    /// </summary>
	    protected NavMeshAgent m_NavMeshAgent;

	    /// <summary>
	    ///     Gets the attached nav mesh agent velocity
	    /// </summary>
	    public override Vector3 velocity => m_NavMeshAgent.velocity;

	    /// <summary>
	    ///     The current state of the agent along the path
	    /// </summary>
	    public State state { get; protected set; }

	    /// <summary>
	    ///     Accessor to <see cref="m_NavMeshAgent" />
	    /// </summary>
	    public NavMeshAgent navMeshNavMeshAgent
        {
            get => m_NavMeshAgent;
            set => m_NavMeshAgent = value;
        }

	    /// <summary>
	    ///     The area mask of the attached nav mesh agent
	    /// </summary>
	    public int navMeshMask => m_NavMeshAgent.areaMask;

	    /// <summary>
	    ///     Gets this agent's original movement speed
	    /// </summary>
	    public float originalMovementSpeed { get; private set; }

	    /// <summary>
	    ///     Checks if the path is blocked
	    /// </summary>
	    /// <value>
	    ///     The status of the agent's path
	    /// </value>
	    protected virtual bool isPathBlocked => m_NavMeshAgent.pathStatus == NavMeshPathStatus.PathPartial;

	    /// <summary>
	    ///     Is the Agent close enough to its destination?
	    /// </summary>
	    protected virtual bool isAtDestination =>
            navMeshNavMeshAgent.remainingDistance <= navMeshNavMeshAgent.stoppingDistance;

	    /// <summary>
	    ///     Lazy Load, if necesaary and ensure the NavMeshAgent is disabled
	    /// </summary>
	    protected override void Awake()
        {
            base.Awake();
            LazyLoad();
            m_NavMeshAgent.enabled = false;
        }

	    /// <summary>
	    ///     Updates the agent in its different states,
	    ///     Reset destination when path is stale
	    /// </summary>
	    protected virtual void Update()
        {
            // Update behaviour for different states
            PathUpdate();

            // If the path becomes invalid, repath the agent to the destination
            var validStalePath = m_NavMeshAgent.isOnNavMesh && m_NavMeshAgent.enabled &&
                                 !m_NavMeshAgent.hasPath && !m_NavMeshAgent.pathPending;
            if (validStalePath)
            {
                // Compare against squared stopping distance on agent.
                // We intentionally do not pre-square this value so that it can be changed at runtime dynamically
                var squareStoppingDistance = m_NavMeshAgent.stoppingDistance * m_NavMeshAgent.stoppingDistance;
                if (Vector3.SqrMagnitude(m_Destination - transform.position) < squareStoppingDistance &&
                    m_CurrentNode.GetNextNode() != null)
                    // Proceed if we're at our destination
                    GetNextNode(m_CurrentNode);
                else
                    // Otherwise try repath
                    m_NavMeshAgent.SetDestination(m_Destination);
            }
        }


#if UNITY_EDITOR
	    /// <summary>
	    ///     Draw the agent's path
	    /// </summary>
	    protected virtual void OnDrawGizmosSelected()
        {
            if (m_NavMeshAgent != null)
            {
                var pathPoints = m_NavMeshAgent.path.corners;
                var count = pathPoints.Length;
                for (var i = 0; i < count - 1; i++)
                {
                    var from = pathPoints[i];
                    var to = pathPoints[i + 1];
                    Gizmos.DrawLine(from, to);
                }

                Gizmos.DrawWireSphere(m_NavMeshAgent.destination, 0.2f);
            }
        }
#endif

	    /// <summary>
	    ///     Event fired when agent reached its final node
	    /// </summary>
	    public event Action<GameNode> destinationReached;

	    /// <summary>
	    ///     Sets the node to navigate to
	    /// </summary>
	    /// <param name="node">The node that the agent will navigate to</param>
	    public virtual void SetNode(GameNode node)
        {
            m_CurrentNode = node;
        }

	    /// <summary>
	    ///     Stops the navMeshAgent and attempts to return to pool
	    /// </summary>
	    public override void Remove()
        {
            base.Remove();

            LevelManager.DecrementNumberOfEnemies();
            if (m_NavMeshAgent.enabled) m_NavMeshAgent.isStopped = true;
            m_NavMeshAgent.enabled = false;

            Poolable.TryPool(gameObject);
        }

	    /// <summary>
	    ///     Setup all the necessary parameters for this agent from configuration data
	    /// </summary>
	    public virtual void Initialize()
        {
            ResetPositionData();
            LazyLoad();
            configuration.SetHealth(configuration.maxHealth);
            state = isPathBlocked ? State.OnPartialPath : State.OnCompletePath;

            m_NavMeshAgent.enabled = true;
            m_NavMeshAgent.isStopped = false;

            LevelManager.IncrementNumberOfEnemies();
        }

	    /// <summary>
	    ///     Finds the next node in the path
	    /// </summary>
	    public virtual void GetNextNode(GameNode currentlyEnteredNode)
        {
            // Don't do anything if the calling node is the same as the m_CurrentNode
            if (m_CurrentNode != currentlyEnteredNode) return;
            if (m_CurrentNode == null)
            {
                Debug.LogError("Cannot find current node");
                return;
            }

            var nextNode = m_CurrentNode.GetNextNode();
            if (nextNode == null)
            {
                if (m_NavMeshAgent.enabled) m_NavMeshAgent.isStopped = true;
                HandleDestinationReached();
                return;
            }

            Debug.Assert(nextNode != m_CurrentNode);
            SetNode(nextNode);
            MoveToNode();
        }

	    /// <summary>
	    ///     Moves the agent to a position in the <see cref="Agent.m_CurrentNode" />
	    /// </summary>
	    public virtual void MoveToNode()
        {
            var nodePosition = m_CurrentNode.GetRandomPointInNodeArea();
            nodePosition.y = m_CurrentNode.transform.position.y;
            m_Destination = nodePosition;
            NavigateTo(m_Destination);
        }

	    /// <summary>
	    ///     The logic for what happens when the destination is reached
	    /// </summary>
	    public virtual void HandleDestinationReached()
        {
            state = State.PathComplete;
            if (destinationReached != null) destinationReached(m_CurrentNode);
        }

	    /// <summary>
	    ///     Set the NavMeshAgent's destination
	    /// </summary>
	    /// <param name="nextPoint">The position to navigate to</param>
	    protected virtual void NavigateTo(Vector3 nextPoint)
        {
            LazyLoad();
            if (m_NavMeshAgent.isOnNavMesh) m_NavMeshAgent.SetDestination(nextPoint);
        }

	    /// <summary>
	    ///     This is a lazy way of caching several components utilised by the Agent
	    /// </summary>
	    protected virtual void LazyLoad()
        {
            if (m_NavMeshAgent == null)
            {
                m_NavMeshAgent = GetComponent<NavMeshAgent>();
                originalMovementSpeed = m_NavMeshAgent.speed;
            }
        }

	    /// <summary>
	    ///     Move along the path, change to <see cref="Agent.State.OnPartialPath" />
	    /// </summary>
	    protected virtual void OnCompletePathUpdate()
        {
            if (isPathBlocked) state = State.OnPartialPath;
        }

	    /// <summary>
	    ///     Peforms the relevant path update
	    /// </summary>
	    protected abstract void PathUpdate();

	    /// <summary>
	    ///     The behaviour for when the agent has been blocked
	    /// </summary>
	    protected abstract void OnPartialPathUpdate();
    }
}