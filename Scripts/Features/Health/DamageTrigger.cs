using UnityEngine;

namespace Features.Health
{
	/// <summary>
	///     Damage trigger - a trigger based implementation of Damage zone
	/// </summary>
	[RequireComponent(typeof(Collider))]
    public class DamageTrigger : DamageZone
    {
	    /// <summary>
	    ///     On entering the trigger see that the collider has a Damager component and if so make the damageableBehaviour take
	    ///     damage
	    /// </summary>
	    /// <param name="triggeredCollider">The collider that entered the trigger</param>
	    protected void OnTriggerEnter(Collider triggeredCollider)
        {
            var damager = triggeredCollider.GetComponent<Damager>();
            if (damager == null) return;
            LazyLoad();

            var scaledDamage = ScaleDamage(damager.damage);
            var collisionPosition = triggeredCollider.ClosestPoint(damager.transform.position);
            damageableBehaviour.TakeDamage(scaledDamage, collisionPosition, damager.alignmentProvider);

            damager.HasDamaged(collisionPosition, damageableBehaviour.configuration.alignmentProvider);
        }
    }
}