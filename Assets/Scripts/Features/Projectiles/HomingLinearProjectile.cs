using System;
using Features.Health;
using Features.Health.Core;
using UnityEngine;
using Utils;
using Infrastructure.Services.ApplicationObservers.Runtime;

namespace Features.Projectiles
{
	/// <summary>
	///     Basic override of LinearProjectile that allows them to adjust their path in-flight to intercept a designated
	///     target.
	/// </summary>
	public class HomingLinearProjectile : LinearProjectile
    {
        public int leadingPrecision = 2;

        public bool leadTarget;

        protected Targetable m_HomingTarget;

        private Vector3 m_TargetVelocity;

        protected override void OnUpdate(float _)
        {
            if (!m_Fired) return;

            if (m_HomingTarget == null)
            {
                m_Rigidbody.rotation = Quaternion.LookRotation(m_Rigidbody.linearVelocity);
                return;
            }

            var aimDirection = Quaternion.LookRotation(GetHeading());

            m_Rigidbody.rotation = aimDirection;
            m_Rigidbody.linearVelocity = transform.forward * m_Rigidbody.linearVelocity.magnitude;

            base.OnUpdate(_);
        }

        protected virtual void FixedUpdate()
        {
            if (m_HomingTarget == null) return;

            m_TargetVelocity = m_HomingTarget.velocity;
        }

        /// <summary>
        ///     Sets the target transform that will be homed in on once fired.
        /// </summary>
        /// <param name="target">Transform of the target to home in on.</param>
        public void SetHomingTarget(Targetable target)
        {
            m_HomingTarget = target;
        }

        protected Vector3 GetHeading()
        {
            if (m_HomingTarget == null) return Vector3.zero;
            Vector3 heading;
            if (leadTarget)
                heading = Ballistics.CalculateLinearLeadingTargetPoint(transform.position, m_HomingTarget.position,
                    m_TargetVelocity, m_Rigidbody.linearVelocity.magnitude,
                    acceleration,
                    leadingPrecision) - transform.position;
            else
                heading = m_HomingTarget.position - transform.position;

            return heading.normalized;
        }

        protected override void Fire(Vector3 firingVector)
        {
            if (m_HomingTarget == null)
            {
                Debug.LogError("Homing target has not been specified. Aborting fire.");
                return;
            }

            m_HomingTarget.removed += OnTargetDied;

            base.Fire(firingVector);
        }

        private void OnTargetDied(DamageableBehaviour targetable)
        {
            targetable.removed -= OnTargetDied;
            m_HomingTarget = null;
        }
    }
}