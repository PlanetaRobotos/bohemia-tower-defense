using Features.Health.Core;
using UnityEngine;

namespace Features.Health
{
	/// <summary>
	///     A simple class for identifying enemies
	/// </summary>
	public class Targetable : DamageableBehaviour
    {
	    /// <summary>
	    ///     The transform that will be targeted
	    /// </summary>
	    public Transform targetTransform;

	    /// <summary>
	    ///     The position of the object
	    /// </summary>
	    protected Vector3 m_CurrentPosition, m_PreviousPosition;

	    /// <summary>
	    ///     The velocity of the rigidbody
	    /// </summary>
	    public virtual Vector3 velocity { get; protected set; }

	    /// <summary>
	    ///     The transform that objects target, which falls back to this object's transform if not set
	    /// </summary>
	    public Transform targetableTransform => targetTransform == null ? transform : targetTransform;

	    /// <summary>
	    ///     Returns our targetable's transform position
	    /// </summary>
	    public override Vector3 position => targetableTransform.position;

	    /// <summary>
	    ///     Initialises any DamageableBehaviour logic
	    /// </summary>
	    protected override void Awake()
        {
            base.Awake();
            ResetPositionData();
        }

	    /// <summary>
	    ///     Calculates the velocity and updates the position
	    /// </summary>
	    private void FixedUpdate()
        {
            m_CurrentPosition = position;
            velocity = (m_CurrentPosition - m_PreviousPosition) / Time.fixedDeltaTime;
            m_PreviousPosition = m_CurrentPosition;
        }

	    /// <summary>
	    ///     Sets up the position data so velocity can be calculated
	    /// </summary>
	    protected void ResetPositionData()
        {
            m_CurrentPosition = position;
            m_PreviousPosition = position;
        }
    }
}