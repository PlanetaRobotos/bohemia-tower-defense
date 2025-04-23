using System;
using System.Collections.Generic;
using System.Linq;
using Features.Input.Extensions;
using Features.Input.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using Infrastructure.Services.ApplicationObservers.Runtime;
using UnityInput = UnityEngine.Input;

namespace Features.Input.Implementation
{
    /// <summary>
    ///     Class to manage tap/drag/pinch gestures and other controls
    /// </summary>
    public class InputController : MonoBehaviour
    {
        /// <summary>
        ///     How quickly flick velocity is accumulated with movements
        /// </summary>
        private const float k_FlickAccumulationFactor = 0.8f;

        /// <summary>
        ///     How far fingers must move before starting a drag
        /// </summary>
        public float dragThresholdTouch = 5;

        /// <summary>
        ///     How far mouse must move before starting a drag
        /// </summary>
        public float dragThresholdMouse;

        /// <summary>
        ///     How long before a touch can no longer be considered a tap
        /// </summary>
        public float tapTime = 0.2f;

        /// <summary>
        ///     How long before a touch is considered a hold
        /// </summary>
        public float holdTime = 0.8f;

        /// <summary>
        ///     Sensitivity of mouse-wheel based zoom
        /// </summary>
        public float mouseWheelSensitivity = 1.0f;

        /// <summary>
        ///     How many mouse buttons to track
        /// </summary>
        public int trackMouseButtons = 2;

        /// <summary>
        ///     Flick movement threshold
        /// </summary>
        public float flickThreshold = 2f;

        /// <summary>
        ///     Mouse button info
        /// </summary>
        private List<MouseButtonInfo> m_MouseInfo;

        /// <summary>
        ///     All the touches we're tracking
        /// </summary>
        private List<TouchInfo> m_Touches;

        /// <summary>
        ///     Gets the number of active touches
        /// </summary>
        public int activeTouchCount => m_Touches.Count;

        /// <summary>
        ///     Tracks if any of the mouse buttons were pressed this frame
        /// </summary>
        public bool mouseButtonPressedThisFrame { get; private set; }

        /// <summary>
        ///     Tracks if the mouse moved this frame
        /// </summary>
        public bool mouseMovedOnThisFrame { get; private set; }

        /// <summary>
        ///     Tracks if a touch began this frame
        /// </summary>
        public bool touchPressedThisFrame { get; private set; }

        /// <summary>
        ///     Current mouse pointer info
        /// </summary>
        public MouseCursorInfo basicMouseInfo { get; private set; }

        [Inject] private IUpdater Updater { get; }

        protected void Awake()
        {
            m_Touches = new List<TouchInfo>();
            m_MouseInfo = new List<MouseButtonInfo>();
            basicMouseInfo = new MouseCursorInfo { currentPosition = UnityInput.mousePosition };

            for (var i = 0; i < trackMouseButtons; i++)
                m_MouseInfo.Add(new MouseButtonInfo
                {
                    currentPosition = UnityInput.mousePosition,
                    mouseButtonId = i
                });

            // UnityInput.simulateMouseWithTouches = false;
        }

        private void Start()
        {
            Updater.Subscribe(OnUpdate, 0);
        }

        protected void OnDestroy()
        {
            Updater?.Unsubscribe(OnUpdate);
        }

        /// <summary>
        ///     Update all input
        /// </summary>
        private void OnUpdate(float _)
        {
            if (basicMouseInfo != null)
                // Mouse was detected as present
                UpdateMouse();
            // Handle touches
            UpdateTouches();
        }

        /// <summary>
        ///     Event called when a pointer press is detected
        /// </summary>
        public event Action<PointerActionInfo> pressed;

        /// <summary>
        ///     Event called when a pointer is released
        /// </summary>
        public event Action<PointerActionInfo> released;

        /// <summary>
        ///     Event called when a pointer is tapped
        /// </summary>
        public event Action<PointerActionInfo> tapped;

        /// <summary>
        ///     Event called when a drag starts
        /// </summary>
        public event Action<PointerActionInfo> startedDrag;

        /// <summary>
        ///     Event called when a pointer is dragged
        /// </summary>
        public event Action<PointerActionInfo> dragged;

        /// <summary>
        ///     Event called when a pointer starts a hold
        /// </summary>
        public event Action<PointerActionInfo> startedHold;

        /// <summary>
        ///     Event called when the user scrolls the mouse wheel
        /// </summary>
        public event Action<WheelInfo> spunWheel;

        /// <summary>
        ///     Event called when the user performs a pinch gesture
        /// </summary>
        public event Action<PinchInfo> pinched;

        /// <summary>
        ///     Event called whenever the mouse is moved
        /// </summary>
        public event Action<PointerInfo> mouseMoved;

        /// <summary>
        ///     Perform logic to update mouse/pointing device
        /// </summary>
        private void UpdateMouse()
        {
            basicMouseInfo.previousPosition = basicMouseInfo.currentPosition;
            basicMouseInfo.currentPosition = UnityInput.mousePosition;
            basicMouseInfo.delta = basicMouseInfo.currentPosition - basicMouseInfo.previousPosition;
            mouseMovedOnThisFrame = basicMouseInfo.delta.sqrMagnitude >= Mathf.Epsilon;
            mouseButtonPressedThisFrame = false;

            // Move event
            if (basicMouseInfo.delta.sqrMagnitude > Mathf.Epsilon)
            {
                if (mouseMoved != null)
                {
                    mouseMoved(basicMouseInfo);
                }
            }

            // Button events
            for (var i = 0; i < trackMouseButtons; ++i)
            {
                var mouseButton = m_MouseInfo[i];
                mouseButton.delta = basicMouseInfo.delta;
                mouseButton.previousPosition = basicMouseInfo.previousPosition;
                mouseButton.currentPosition = basicMouseInfo.currentPosition;
                if (UnityInput.GetMouseButton(i))
                {
                    if (!mouseButton.isDown)
                    {
                        // First press
                        mouseButtonPressedThisFrame = true;
                        mouseButton.isDown = true;
                        mouseButton.startPosition = UnityInput.mousePosition;
                        mouseButton.startTime = Time.realtimeSinceStartup;
                        mouseButton.startedOverUI =
                            EventSystem.current.IsPointerOverGameObject(-mouseButton.mouseButtonId - 1);

                        // Reset some stuff
                        mouseButton.totalMovement = 0;
                        mouseButton.isDrag = false;
                        mouseButton.wasHold = false;
                        mouseButton.isHold = false;
                        mouseButton.flickVelocity = Vector2.zero;

                        if (pressed != null) pressed(mouseButton);
                    }
                    else
                    {
                        var moveDist = mouseButton.delta.magnitude;
                        // Dragging?
                        mouseButton.totalMovement += moveDist;
                        if (mouseButton.totalMovement > dragThresholdMouse)
                        {
                            var wasDrag = mouseButton.isDrag;

                            mouseButton.isDrag = true;
                            if (mouseButton.isHold)
                            {
                                mouseButton.wasHold = mouseButton.isHold;
                                mouseButton.isHold = false;
                            }

                            // Did it just start now?
                            if (!wasDrag)
                                if (startedDrag != null)
                                    startedDrag(mouseButton);
                            if (dragged != null) dragged(mouseButton);

                            // Flick?
                            if (moveDist > flickThreshold)
                                mouseButton.flickVelocity =
                                    mouseButton.flickVelocity * (1 - k_FlickAccumulationFactor) +
                                    mouseButton.delta * k_FlickAccumulationFactor;
                            else
                                mouseButton.flickVelocity = Vector2.zero;
                        }
                        else
                        {
                            // Stationary?
                            if (!mouseButton.isHold &&
                                !mouseButton.isDrag &&
                                Time.realtimeSinceStartup - mouseButton.startTime >= holdTime)
                            {
                                mouseButton.isHold = true;
                                if (startedHold != null) startedHold(mouseButton);
                            }
                        }
                    }
                }
                else // Mouse button not up
                {
                    if (mouseButton.isDown) // Released
                    {
                        mouseButton.isDown = false;
                        // Quick enough (with no drift) to be a tap?
                        if (!mouseButton.isDrag &&
                            Time.realtimeSinceStartup - mouseButton.startTime < tapTime)
                            if (tapped != null)
                                tapped(mouseButton);
                        if (released != null) released(mouseButton);
                    }
                }
            }

            // Mouse wheel
            if (Mathf.Abs(UnityInput.GetAxis("Mouse ScrollWheel")) > Mathf.Epsilon)
                if (spunWheel != null)
                    spunWheel(new WheelInfo
                    {
                        zoomAmount = UnityInput.GetAxis("Mouse ScrollWheel") * mouseWheelSensitivity
                    });
        }

        /// <summary>
        ///     Update all touches
        /// </summary>
        private void UpdateTouches()
        {
            touchPressedThisFrame = false;

            int count = TouchHelper.Count;
            for (int i = 0; i < count; ++i)
            {
                // fetch a "Touch" either from the helper (WebGL/Editor) or the real Input API
                Touch touch = GetTouch(i);

                // Find existing touch, or create new one
                var existingTouch = m_Touches.FirstOrDefault(t => t.touchId == touch.fingerId);

                if (existingTouch == null)
                {
                    existingTouch = new TouchInfo
                    {
                        touchId = touch.fingerId,
                        startPosition = touch.position,
                        currentPosition = touch.position,
                        previousPosition = touch.position,
                        startTime = Time.realtimeSinceStartup,
                        startedOverUI = EventSystem.current.IsPointerOverGameObject(touch.fingerId)
                    };
                    m_Touches.Add(existingTouch);

                    Debug.Assert(touch.phase == TouchPhase.Began);
                }

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        touchPressedThisFrame = true;
                        pressed?.Invoke(existingTouch);
                        break;

                    case TouchPhase.Moved:
                        var wasDrag = existingTouch.isDrag;
                        UpdateMovingFinger(touch, existingTouch);

                        existingTouch.isDrag = existingTouch.totalMovement >= dragThresholdTouch;
                        if (existingTouch.isDrag)
                        {
                            if (existingTouch.isHold)
                            {
                                existingTouch.wasHold = true;
                                existingTouch.isHold = false;
                            }

                            if (!wasDrag) startedDrag?.Invoke(existingTouch);
                            dragged?.Invoke(existingTouch);

                            if (existingTouch.delta.sqrMagnitude > flickThreshold * flickThreshold)
                                existingTouch.flickVelocity =
                                    existingTouch.flickVelocity * (1 - k_FlickAccumulationFactor) +
                                    existingTouch.delta * k_FlickAccumulationFactor;
                            else
                                existingTouch.flickVelocity = Vector2.zero;
                        }
                        else
                        {
                            UpdateHoldingFinger(existingTouch);
                        }

                        break;

                    case TouchPhase.Stationary:
                        UpdateMovingFinger(touch, existingTouch);
                        UpdateHoldingFinger(existingTouch);
                        existingTouch.flickVelocity = Vector2.zero;
                        break;

                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        UpdateMovingFinger(touch, existingTouch);

                        if (!existingTouch.isDrag &&
                            Time.realtimeSinceStartup - existingTouch.startTime < tapTime)
                            tapped?.Invoke(existingTouch);

                        released?.Invoke(existingTouch);
                        m_Touches.Remove(existingTouch);
                        break;
                }
            }

            if (m_Touches.Count >= 2 &&
                (m_Touches[0].isDrag || m_Touches[1].isDrag))
            {
                pinched?.Invoke(new PinchInfo { touch1 = m_Touches[0], touch2 = m_Touches[1] });
            }
        }

        /// <summary>
        /// Returns a Touch object that comes from TouchHelper on WebGL/Editor,
        /// or from Input.GetTouch on other platforms.
        /// </summary>
        private Touch GetTouch(int index)
        {
#if UNITY_WEBGL || UNITY_EDITOR
            // only a single "mouse-as-touch" on WebGL/Editor
            return new Touch
            {
                fingerId = 0,
                position = TouchHelper.Position,
                phase = TouchHelper.Phase
            };
#else
    return UnityInput.GetTouch(index);
#endif
        }

        /// <summary>
        ///     Update a TouchInfo that might be holding
        /// </summary>
        /// <param name="existingTouch"></param>
        private void UpdateHoldingFinger(PointerActionInfo existingTouch)
        {
            if (!existingTouch.isHold &&
                !existingTouch.isDrag &&
                Time.realtimeSinceStartup - existingTouch.startTime >= holdTime)
            {
                existingTouch.isHold = true;
                if (startedHold != null) startedHold(existingTouch);
            }
        }

        /// <summary>
        ///     Update a TouchInfo with movement
        /// </summary>
        /// <param name="touch">The Unity touch object</param>
        /// <param name="existingTouch">The object that's tracking Unity's touch</param>
        private void UpdateMovingFinger(Touch touch, PointerActionInfo existingTouch)
        {
            var dragDist = touch.deltaPosition.magnitude;

            existingTouch.previousPosition = existingTouch.currentPosition;
            existingTouch.currentPosition = touch.position;
            existingTouch.delta = touch.deltaPosition;
            existingTouch.totalMovement += dragDist;
        }
    }
}