using UnityEngine;

namespace Features.Input.Models
{
	/// <summary>
	///     Class to track information about an active pointer input
	/// </summary>
	public class PointerActionInfo : PointerInfo
    {
	    /// <summary>
	    ///     Flick velocity is a moving average of deltas
	    /// </summary>
	    public Vector2 flickVelocity;

	    /// <summary>
	    ///     Has this input been dragged?
	    /// </summary>
	    public bool isDrag;

	    /// <summary>
	    ///     Is this input holding?
	    /// </summary>
	    public bool isHold;

	    /// <summary>
	    ///     Position where the input started
	    /// </summary>
	    public Vector2 startPosition;

	    /// <summary>
	    ///     Time hold started
	    /// </summary>
	    public float startTime;

	    /// <summary>
	    ///     Total movement for this pointer, since being held down
	    /// </summary>
	    public float totalMovement;

	    /// <summary>
	    ///     Was this input previously holding, then dragged?
	    /// </summary>
	    public bool wasHold;

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return $"PointerActionInfo[Position: {currentPosition}, Previous: {previousPosition}, Delta: {delta}, " +
                   $"FlickVel: {flickVelocity}, IsDrag: {isDrag}, IsHold: {isHold}, WasHold: {wasHold}, " +
                   $"StartPos: {startPosition}, StartTime: {startTime:F2}, TotalMovement: {totalMovement:F2}]";
        }
    }
}