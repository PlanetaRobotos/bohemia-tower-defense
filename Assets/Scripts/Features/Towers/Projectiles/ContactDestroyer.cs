using UnityEngine;
using Utils;
using Infrastructure.Services.ApplicationObservers.Runtime;

namespace Features.Towers.Projectiles
{
	/// <summary>
	///     For objects that destroyer themselves on contact
	/// </summary>
	public class ContactDestroyer : MonoBehaviour
    {
	    /// <summary>
	    ///     The y-value of the position the object will destroy itself
	    /// </summary>
	    public float yDestroyPoint = -50;

	    /// <summary>
	    ///     The attached collider
	    /// </summary>
	    protected Collider m_AttachedCollider;

	    [Inject] private IUpdater Updater { get; }

	    /// <summary>
	    ///     Caches the attached collider
	    /// </summary>
	    protected virtual void Awake()
        {
            m_AttachedCollider = GetComponent<Collider>();
            Updater.Subscribe(OnUpdate, 0);
        }

	    /// <summary>
	    ///     Checks the y-position against <see cref="yDestroyPoint" />
	    /// </summary>
	    protected virtual void OnUpdate(float _)
        {
            if (transform.position.y < yDestroyPoint) ReturnToPool();
        }

        protected virtual void OnDestroy()
        {
            Updater?.Unsubscribe(OnUpdate);
        }

        private void OnCollisionEnter(Collision other)
        {
            ReturnToPool();
        }

        /// <summary>
        ///     Returns the object to pool if possible, otherwise destroys
        /// </summary>
        private void ReturnToPool()
        {
            if (!gameObject.activeInHierarchy) return;
            Poolable.TryPool(gameObject);
        }
    }
}