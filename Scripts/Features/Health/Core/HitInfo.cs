using UnityEngine;

namespace Features.Health.Core
{
	/// <summary>
	///     Damage info - a class required by some damage listeners
	/// </summary>
	public struct HitInfo
    {
	    /// <summary>
	    ///     Gets or sets the health change info.
	    /// </summary>
	    /// <value>The health change info.</value>
	    public HealthChangeInfo healthChangeInfo { get; }

	    /// <summary>
	    ///     Gets or sets the damage point.
	    /// </summary>
	    /// <value>The damage point.</value>
	    public Vector3 damagePoint { get; }

	    /// <summary>
	    ///     Initializes a new instance of the <see cref="HitInfo" /> struct.
	    /// </summary>
	    /// <param name="info">The health change info</param>
	    /// <param name="damageLocation">Damage point.</param>
	    public HitInfo(HealthChangeInfo info, Vector3 damageLocation)
        {
            damagePoint = damageLocation;
            healthChangeInfo = info;
        }
    }
}