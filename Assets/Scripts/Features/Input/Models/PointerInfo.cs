using UnityEngine;

namespace Features.Input.Models
{
	/// <summary>
	///     Class to track information about a passive pointer input
	/// </summary>
	public abstract class PointerInfo
    {
	    /// <summary>
	    ///     Current pointer position
	    /// </summary>
	    public Vector2 currentPosition;

	    /// <summary>
	    ///     Movement delta for this frame
	    /// </summary>
	    public Vector2 delta;

	    /// <summary>
	    ///     Previous frame's pointer position
	    /// </summary>
	    public Vector2 previousPosition;

	    /// <summary>
	    ///     Tracks if this pointer began over UI
	    /// </summary>
	    public bool startedOverUI;
    }
}