using Features.Health.Core;
using UnityEngine;

namespace Features.Spawning
{
	/// <summary>
	///     Hit object spawner - provides a public method for spawning game objects based on a hit
	///     The spawned game object may have a HitObject component, which consumes the hit information
	/// </summary>
	public abstract class HitObjectSpawner : MonoBehaviour
    {
	    /// <summary>
	    ///     Gets the game object to instantiate.
	    ///     This is needed to that we can provide different mechanisms for choosing game objects to instantiate
	    /// </summary>
	    /// <returns>The game object to instantiate.</returns>
	    protected abstract GameObject GetGameObjectToInstantiate();

	    /// <summary>
	    ///     The public method for instantiating a hit object - this can be accessed by methods on the DamageableListener
	    /// </summary>
	    /// <param name="hitInfo">Hit info.</param>
	    public virtual void InstantiateHitObject(HitInfo hitInfo)
        {
            var gameObjectToInstantiate = GetGameObjectToInstantiate();
            var gameObjectInstance = Instantiate(gameObjectToInstantiate, hitInfo.damagePoint, Quaternion.identity);
            var hitObjects = gameObjectInstance.GetComponentsInChildren<HitObject>();
            var length = hitObjects.Length;
            for (var i = 0; i < length; i++)
            {
                var hitObject = hitObjects[i];
                hitObject.SetHitInfo(hitInfo);
            }
        }
    }
}