using System;
using Features.Input.Extensions;
using Features.Input.Models;
using UnityEngine;
using UnityInput = UnityEngine.Input;
using Infrastructure.Services.ApplicationObservers.Runtime;

namespace Features.Input.Declaration
{
	/// <summary>
	///     Base control scheme for touch devices, which performs CameraRig control
	/// </summary>
	public class TouchInput : CameraInputScheme
    {
	    /// <summary>
	    ///     Configuration of the pan speed
	    /// </summary>
	    public float panSpeed = 5;

	    /// <summary>
	    ///     How quickly flicks decay
	    /// </summary>
	    public float flickDecayFactor = 0.2f;

	    /// <summary>
	    ///     Flick direction
	    /// </summary>
	    private Vector3 m_FlickDirection;

	    /// <summary>
	    ///     Gets whether the scheme should be activated or not
	    /// </summary>
	    public override bool shouldActivate => TouchHelper.Count > 0;

	    /// <summary>
	    ///     This default scheme on IOS and Android devices
	    /// </summary>
	    public override bool isDefault
        {
            get
            {
#if UNITY_IOS || UNITY_ANDROID
				return true;
#else
                return false;
#endif
            }
        }

	    [Inject] private IUpdater Updater { get; }

	    protected virtual void Awake()
        {
        }

	    private void Start()
	    {
		    Updater.Subscribe(OnUpdate, 0);
	    }

	    protected virtual void OnDestroy()
        {
            Updater?.Unsubscribe(OnUpdate);
        }

	    /// <summary>
	    ///     Perform flick and zoom
	    /// </summary>
	    protected virtual void OnUpdate(float _)
        {
            if (cameraRig != null)
            {
                UpdateFlick();
                DecayZoom();
            }
        }

	    /// <summary>
	    ///     Register input events
	    /// </summary>
	    protected virtual void OnEnable()
        {
            if (!inputController)
            {
                Debug.LogError("[UI] Keyboard and Mouse UI requires InputController");
                return;
            }

            // Register drag event
            inputController.pressed += OnPress;
            inputController.released += OnRelease;
            inputController.dragged += OnDrag;
            inputController.pinched += OnPinch;
        }

	    /// <summary>
	    ///     Deregister input events
	    /// </summary>
	    protected virtual void OnDisable()
        {
            if (inputController)
            {
                inputController.pressed -= OnPress;
                inputController.released -= OnRelease;
                inputController.dragged -= OnDrag;
                inputController.pinched -= OnPinch;
            }
        }

	    /// <summary>
	    ///     Called on input press
	    /// </summary>
	    protected virtual void OnPress(PointerActionInfo pointer)
        {
            if (cameraRig != null) DoFlickCatch(pointer);
        }

	    /// <summary>
	    ///     Called on input release
	    /// </summary>
	    protected virtual void OnRelease(PointerActionInfo pointer)
        {
            if (cameraRig != null) DoReleaseFlick(pointer);
        }

	    /// <summary>
	    ///     Called when we drag
	    /// </summary>
	    protected virtual void OnDrag(PointerActionInfo pointer)
        {
            // Drag panning for touch input
            if (cameraRig != null) DoDragPan(pointer);
        }

	    /// <summary>
	    ///     Called on pinch gestures
	    /// </summary>
	    protected virtual void OnPinch(PinchInfo pinch)
        {
            if (cameraRig != null) DoPinchZoom(pinch);
        }

	    /// <summary>
	    ///     Update current flick velocity
	    /// </summary>
	    protected void UpdateFlick()
        {
            // Flick?
            if (m_FlickDirection.sqrMagnitude > Mathf.Epsilon)
            {
                cameraRig.PanCamera(m_FlickDirection * Time.deltaTime);
                m_FlickDirection *= flickDecayFactor;
            }
        }

	    /// <summary>
	    ///     Decay the zoom if no touches are active
	    /// </summary>
	    protected void DecayZoom()
        {
            if (inputController.activeTouchCount == 0) cameraRig.ZoomDecay();
        }

	    /// <summary>
	    ///     "Catch" flicks on press, to stop the panning momentum
	    /// </summary>
	    /// <param name="pointer">The press pointer event</param>
	    protected void DoFlickCatch(PointerActionInfo pointer)
        {
            var touchInfo = pointer as TouchInfo;
            // Stop flicks on touch
            if (touchInfo != null)
            {
                m_FlickDirection = Vector2.zero;
                cameraRig.StopTracking();
            }
        }

	    /// <summary>
	    ///     Do flicks, on release only
	    /// </summary>
	    /// <param name="pointer">The release pointer event</param>
	    protected void DoReleaseFlick(PointerActionInfo pointer)
        {
            var touchInfo = pointer as TouchInfo;

            if (touchInfo != null && touchInfo.flickVelocity.sqrMagnitude > Mathf.Epsilon)
            {
                // We have a flick!
                // Work out velocity from motion
                var prevRay = cameraRig.cachedCamera.ScreenPointToRay(pointer.currentPosition -
                                                                      pointer.flickVelocity);
                var currRay = cameraRig.cachedCamera.ScreenPointToRay(pointer.currentPosition);

                var startPoint = Vector3.zero;
                var endPoint = Vector3.zero;
                float dist;

                if (cameraRig.floorPlane.Raycast(prevRay, out dist)) startPoint = prevRay.GetPoint(dist);
                if (cameraRig.floorPlane.Raycast(currRay, out dist)) endPoint = currRay.GetPoint(dist);

                // Work out that movement in units per second
                m_FlickDirection = (startPoint - endPoint) / Time.deltaTime;
            }
        }

	    /// <summary>
	    ///     Controls the pan with a drag
	    /// </summary>
	    protected void DoDragPan(PointerActionInfo pointer)
        {
            var touchInfo = pointer as TouchInfo;
            if (touchInfo != null)
            {
                // Work out movement amount by raycasting onto floor plane from delta positions
                // and getting that distance
                var currRay = cameraRig.cachedCamera.ScreenPointToRay(touchInfo.currentPosition);

                var endPoint = Vector3.zero;
                float dist;
                if (cameraRig.floorPlane.Raycast(currRay, out dist)) endPoint = currRay.GetPoint(dist);
                // Pan
                var prevRay = cameraRig.cachedCamera.ScreenPointToRay(touchInfo.previousPosition);
                var startPoint = Vector3.zero;

                if (cameraRig.floorPlane.Raycast(prevRay, out dist)) startPoint = prevRay.GetPoint(dist);
                var panAmount = startPoint - endPoint;
                // If this is a touch, we divide the pan amount by the number of touches
                if (TouchHelper.Count > 0) panAmount /= TouchHelper.Count;

                PanCamera(panAmount);
            }
        }

	    /// <summary>
	    ///     Perform a zoom with the given pinch
	    /// </summary>
	    protected void DoPinchZoom(PinchInfo pinch)
        {
            var currentDistance = (pinch.touch1.currentPosition - pinch.touch2.currentPosition).magnitude;
            var prevDistance = (pinch.touch1.previousPosition - pinch.touch2.previousPosition).magnitude;

            var zoomChange = prevDistance / currentDistance;
            var prevZoomDist = cameraRig.zoomDist;

            cameraRig.SetZoom(zoomChange * cameraRig.rawZoomDist);

            // Calculate actual zoom change after clamping
            zoomChange = cameraRig.zoomDist / prevZoomDist;

            // First get floor position of middle of gesture
            var averageScreenPos = (pinch.touch1.currentPosition + pinch.touch2.currentPosition) * 0.5f;
            var ray = cameraRig.cachedCamera.ScreenPointToRay(averageScreenPos);

            var worldPos = Vector3.zero;
            float dist;

            if (cameraRig.floorPlane.Raycast(ray, out dist)) worldPos = ray.GetPoint(dist);

            // Vector from our current look pos to this point 
            var offsetValue = worldPos - cameraRig.lookPosition;

            // Pan towards or away from our zoom center
            PanCamera(offsetValue * (1 - zoomChange));
        }

	    /// <summary>
	    ///     Pans the camera
	    /// </summary>
	    /// <param name="panAmount">
	    ///     The vector to pan
	    /// </param>
	    protected void PanCamera(Vector3 panAmount)
        {
            cameraRig.StopTracking();
            cameraRig.PanCamera(panAmount);
        }
    }
}