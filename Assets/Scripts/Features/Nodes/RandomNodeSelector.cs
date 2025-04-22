using Utils;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace TowerDefense.Nodes
{
	/// <summary>
	///     Randomly selects the next node
	/// </summary>
	public class RandomNodeSelector : NodeSelector
    {
	    /// <summary>
	    ///     The sum of all Node weights in m_LinkedNodes
	    /// </summary>
	    protected int m_WeightSum;

        protected void Awake()
        {
            // cache the linked node weights
            m_WeightSum = TotalLinkedNodeWeights();
        }
#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            base.OnDrawGizmos();
        }
#endif

	    /// <summary>
	    ///     Gets a random node in the list
	    /// </summary>
	    /// <returns>The randomly selected node</returns>
	    public override GameNode GetNextNode()
        {
            if (linkedNodes == null) return null;
            var totalWeight = m_WeightSum;
            return linkedNodes.WeightedSelection(totalWeight, t => t.weight);
        }

	    /// <summary>
	    ///     Sums up the weights of the linked nodes for random selection
	    /// </summary>
	    /// <returns>Weight Sum of Linked Nodes</returns>
	    protected int TotalLinkedNodeWeights()
        {
            var totalWeight = 0;
            var count = linkedNodes.Count;
            for (var i = 0; i < count; i++) totalWeight += linkedNodes[i].weight;
            return totalWeight;
        }
    }
}