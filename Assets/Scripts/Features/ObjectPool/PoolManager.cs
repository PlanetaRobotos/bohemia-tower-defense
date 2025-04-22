using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	/// <summary>
	///     Managers a dictionary of pools, getting and returning
	/// </summary>
	public class PoolManager : MonoBehaviour
    {
	    /// <summary>
	    ///     List of poolables that will be used to initialize corresponding pools
	    /// </summary>
	    public List<Poolable> poolables;

	    /// <summary>
	    ///     Dictionary of pools, key is the prefab
	    /// </summary>
	    protected Dictionary<Poolable, AutoComponentPrefabPool<Poolable>> m_Pools;

	    /// <summary>
	    ///     Initializes the dicionary of pools
	    /// </summary>
	    protected void Start()
        {
            m_Pools = new Dictionary<Poolable, AutoComponentPrefabPool<Poolable>>();

            foreach (var poolable in poolables)
            {
                if (poolable == null) continue;
                m_Pools.Add(poolable, new AutoComponentPrefabPool<Poolable>(poolable, Initialize, null,
                    poolable.initialPoolCapacity));
            }
        }

	    /// <summary>
	    ///     Gets a poolable component from the corresponding pool
	    /// </summary>
	    /// <param name="poolablePrefab"></param>
	    /// <returns></returns>
	    public Poolable GetPoolable(Poolable poolablePrefab)
        {
            if (!m_Pools.ContainsKey(poolablePrefab))
                m_Pools.Add(poolablePrefab, new AutoComponentPrefabPool<Poolable>(poolablePrefab, Initialize, null,
                    poolablePrefab.initialPoolCapacity));

            var pool = m_Pools[poolablePrefab];
            var spawnedInstance = pool.Get();

            spawnedInstance.pool = pool;
            return spawnedInstance;
        }

	    /// <summary>
	    ///     Returns the poolable component to its component pool
	    /// </summary>
	    /// <param name="poolable"></param>
	    public void ReturnPoolable(Poolable poolable)
        {
            poolable.pool.Return(poolable);
        }

        private void Initialize(Component poolable)
        {
            poolable.transform.SetParent(transform, false);
        }
    }
}