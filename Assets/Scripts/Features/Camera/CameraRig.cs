using Features.Input.Models;
using UnityEngine;
using Infrastructure.Services.ApplicationObservers.Runtime;

namespace Features.Camera
{
	/// <summary>
	///     Class to control the camera's behaviour. Camera rig currently operates best on terrain that is mostly on
	///     a single plane
	/// </summary>
	public class CameraRig : MonoBehaviour
    {
	    /// <summary>
	    ///     Look dampening factor
	    /// </summary>
	    public float lookDampFactor;

	    /// <summary>
	    ///     Movement dampening factor
	    /// </summary>
	    public float movementDampFactor;

	    /// <summary>
	    ///     Nearest zoom level - can go a bit further than this on touch, for springiness
	    /// </summary>
	    public float nearestZoom = 15;

	    /// <summary>
	    ///     Furthest zoom level - can go a bit further than this on touch, for springiness
	    /// </summary>
	    public float furthestZoom = 40;

	    /// <summary>
	    ///     True maximum zoom level
	    /// </summary>
	    public float maxZoom = 60;

	    /// <summary>
	    ///     Logarithm used to decay zoom beyond furthest
	    /// </summary>
	    public float zoomLogFactor = 10;

	    /// <summary>
	    ///     How fast zoom recovers to normal
	    /// </summary>
	    public float zoomRecoverSpeed = 20;

	    /// <summary>
	    ///     Y-height of the floor the camera is assuming
	    /// </summary>
	    public float floorY;

	    /// <summary>
	    ///     Camera angle when fully zoomed in
	    /// </summary>
	    public Transform zoomedCamAngle;

	    /// <summary>
	    ///     Map size, edited through the CameraRigEditor script in edit mode
	    /// </summary>
	    [HideInInspector] public Rect mapSize = new(-10, -10, 20, 20);

	    /// <summary>
	    ///     Is the zoom able to exceed its normal zoom extents with a rubber banding effect
	    /// </summary>
	    public bool springyZoom = true;

	    /// <summary>
	    ///     Current camera velocity
	    /// </summary>
	    private Vector3 m_CurrentCamVelocity;

	    /// <summary>
	    ///     Current look velocity of camera
	    /// </summary>
	    private Vector3 m_CurrentLookVelocity;

	    /// <summary>
	    ///     Current reusable floor plane
	    /// </summary>
	    private Plane m_FloorPlane;

        private Quaternion m_MaxZoomRotation;

        /// <summary>
        ///     Rotations of camera at various zoom levels
        /// </summary>
        private Quaternion m_MinZoomRotation;

        public Plane floorPlane => m_FloorPlane;

        /// <summary>
        ///     Target position on the grid that we're looking at
        /// </summary>
        public Vector3 lookPosition { get; private set; }

        /// <summary>
        ///     Current look position of camera
        /// </summary>
        public Vector3 currentLookPosition { get; private set; }

        /// <summary>
        ///     Target position of the camera
        /// </summary>
        public Vector3 cameraPosition { get; private set; }

        /// <summary>
        ///     Bounds of our look area, related to map size, zoom level and aspect ratio/screen size
        /// </summary>
        public Rect lookBounds { get; private set; }

        /// <summary>
        ///     Gets our current zoom distance
        /// </summary>
        public float zoomDist { get; private set; }

        /// <summary>
        ///     Gets our current internal zoom distance, before clamping and scaling is applied
        /// </summary>
        public float rawZoomDist { get; private set; }

        /// <summary>
        ///     Gets the unit we're tracking if any
        /// </summary>
        public GameObject trackingObject { get; private set; }

        /// <summary>
        ///     Cached camera component
        /// </summary>
        public UnityEngine.Camera cachedCamera { get; private set; }

        [Inject] private IUpdater Updater { get; }

        /// <summary>
        ///     Initialize references and floor plane
        /// </summary>
        protected virtual void Awake()
        {
            cachedCamera = GetComponent<UnityEngine.Camera>();
            m_FloorPlane = new Plane(Vector3.up, new Vector3(0.0f, floorY, 0.0f));

            // Set initial values
            var lookRay = new Ray(cachedCamera.transform.position, cachedCamera.transform.forward);

            float dist;
            if (m_FloorPlane.Raycast(lookRay, out dist)) currentLookPosition = lookPosition = lookRay.GetPoint(dist);
            cameraPosition = cachedCamera.transform.position;

            m_MinZoomRotation = Quaternion.FromToRotation(Vector3.up, -cachedCamera.transform.forward);
            m_MaxZoomRotation = Quaternion.FromToRotation(Vector3.up, -zoomedCamAngle.transform.forward);
            rawZoomDist = zoomDist = (currentLookPosition - cameraPosition).magnitude;

            Updater.Subscribe(OnUpdate, 0);
        }

        /// <summary>
        ///     Setup initial zoom level and camera bounds
        /// </summary>
        protected virtual void Start()
        {
            RecalculateBoundingRect();
        }

        /// <summary>
        ///     Handle camera behaviour
        /// </summary>
        protected virtual void OnUpdate(float _)
        {
            RecalculateBoundingRect();

            // Tracking?
            if (trackingObject != null)
            {
                PanTo(trackingObject.transform.position);

                if (!trackingObject.activeInHierarchy) StopTracking();
            }

            // Approach look position
            currentLookPosition = Vector3.SmoothDamp(currentLookPosition, lookPosition, ref m_CurrentLookVelocity,
                lookDampFactor);

            var worldPos = transform.position;
            worldPos = Vector3.SmoothDamp(worldPos, cameraPosition, ref m_CurrentCamVelocity,
                movementDampFactor);

            transform.position = worldPos;
            transform.LookAt(currentLookPosition);
        }

        protected virtual void OnDestroy()
        {
            Updater?.Unsubscribe(OnUpdate);
        }

#if UNITY_EDITOR
	    /// <summary>
	    ///     Debug bounds area gizmo
	    /// </summary>
	    private void OnDrawGizmosSelected()
        {
            // We dont want to display this in edit mode
            if (!Application.isPlaying) return;
            if (cachedCamera == null) cachedCamera = GetComponent<UnityEngine.Camera>();
            RecalculateBoundingRect();

            Gizmos.color = Color.red;

            Gizmos.DrawLine(
                new Vector3(lookBounds.xMin, 0.0f, lookBounds.yMin),
                new Vector3(lookBounds.xMax, 0.0f, lookBounds.yMin));
            Gizmos.DrawLine(
                new Vector3(lookBounds.xMin, 0.0f, lookBounds.yMin),
                new Vector3(lookBounds.xMin, 0.0f, lookBounds.yMax));
            Gizmos.DrawLine(
                new Vector3(lookBounds.xMax, 0.0f, lookBounds.yMax),
                new Vector3(lookBounds.xMin, 0.0f, lookBounds.yMax));
            Gizmos.DrawLine(
                new Vector3(lookBounds.xMax, 0.0f, lookBounds.yMax),
                new Vector3(lookBounds.xMax, 0.0f, lookBounds.yMin));

            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(transform.position, currentLookPosition);
        }
#endif

	    /// <summary>
	    ///     Pans the camera to a specific position
	    /// </summary>
	    /// <param name="position">The look target</param>
	    public void PanTo(Vector3 position)
        {
            var pos = position;

            // Look position is floor height
            pos.y = floorY;

            // Clamp to look bounds
            pos.x = Mathf.Clamp(pos.x, lookBounds.xMin, lookBounds.xMax);
            pos.z = Mathf.Clamp(pos.z, lookBounds.yMin, lookBounds.yMax);
            lookPosition = pos;

            // Camera position calculated from look position with view vector and zoom dist
            cameraPosition = lookPosition + GetToCamVector() * zoomDist;
        }

	    /// <summary>
	    ///     Cause the camera to follow a unit
	    /// </summary>
	    /// <param name="objectToTrack"></param>
	    public void TrackObject(GameObject objectToTrack)
        {
            trackingObject = objectToTrack;
            PanTo(trackingObject.transform.position);
        }

	    /// <summary>
	    ///     Stop tracking a unit
	    /// </summary>
	    public void StopTracking()
        {
            trackingObject = null;
        }

	    /// <summary>
	    ///     Pan the camera
	    /// </summary>
	    /// <param name="panDelta">How far to pan the camera, in world space units</param>
	    public void PanCamera(Vector3 panDelta)
        {
            var pos = lookPosition;
            pos += panDelta;

            // Clamp to look bounds
            pos.x = Mathf.Clamp(pos.x, lookBounds.xMin, lookBounds.xMax);
            pos.z = Mathf.Clamp(pos.z, lookBounds.yMin, lookBounds.yMax);
            lookPosition = pos;

            // Camera position calculated from look position with view vector and zoom dist
            cameraPosition = lookPosition + GetToCamVector() * zoomDist;
        }

	    /// <summary>
	    ///     Zoom the camera by a specified value
	    /// </summary>
	    /// <param name="zoomDelta">How far to zoom the camera</param>
	    public void ZoomCameraRelative(float zoomDelta)
        {
            SetZoom(rawZoomDist + zoomDelta);
        }

	    /// <summary>
	    ///     Zoom the camera to a specified value
	    /// </summary>
	    /// <param name="newZoom">The absolute zoom value</param>
	    public void SetZoom(float newZoom)
        {
            if (springyZoom)
            {
                rawZoomDist = newZoom;

                if (newZoom > furthestZoom)
                {
                    zoomDist = furthestZoom;
                    zoomDist += Mathf.Log(Mathf.Min(rawZoomDist, maxZoom) - furthestZoom + 1, zoomLogFactor);
                }
                else if (rawZoomDist < nearestZoom)
                {
                    zoomDist = nearestZoom;
                    zoomDist -= Mathf.Log(nearestZoom - rawZoomDist + 1, zoomLogFactor);
                }
                else
                {
                    zoomDist = rawZoomDist;
                }
            }
            else
            {
                zoomDist = rawZoomDist = Mathf.Clamp(newZoom, nearestZoom, furthestZoom);
            }

            // Update bounding rectangle, which is based on our zoom level
            RecalculateBoundingRect();

            // Force recalculated CameraPosition
            PanCamera(Vector3.zero);
        }

	    /// <summary>
	    ///     Calculates the ray for a specified pointer in 3d space
	    /// </summary>
	    /// <param name="pointer">The pointer info</param>
	    /// <returns>The ray representing a screen-space pointer in 3D space</returns>
	    public Ray GetRayForPointer(PointerInfo pointer)
        {
            return cachedCamera.ScreenPointToRay(pointer.currentPosition);
        }

	    /// <summary>
	    ///     Gets the screen position of a given world position
	    /// </summary>
	    /// <param name="worldPos">The world position</param>
	    /// <returns>The screen position of that point</returns>
	    public Vector3 GetScreenPos(Vector3 worldPos)
        {
            return cachedCamera.WorldToScreenPoint(worldPos);
        }

	    /// <summary>
	    ///     Decay the zoom if it's beyond its zoom limits, for springiness
	    /// </summary>
	    public void ZoomDecay()
        {
            if (springyZoom)
            {
                if (rawZoomDist > furthestZoom)
                {
                    var recover = rawZoomDist - furthestZoom;
                    SetZoom(Mathf.Max(furthestZoom, rawZoomDist - recover * zoomRecoverSpeed * Time.deltaTime));
                }
                else if (rawZoomDist < nearestZoom)
                {
                    var recover = nearestZoom - rawZoomDist;
                    SetZoom(Mathf.Min(nearestZoom, rawZoomDist + recover * zoomRecoverSpeed * Time.deltaTime));
                }
            }
        }

	    /// <summary>
	    ///     Returns our normalized zoom ratio
	    /// </summary>
	    public float CalculateZoomRatio()
        {
            return Mathf.Clamp01(Mathf.InverseLerp(nearestZoom, furthestZoom, zoomDist));
        }

	    /// <summary>
	    ///     Gets the to camera vector based on our current zoom level
	    /// </summary>
	    private Vector3 GetToCamVector()
        {
            var t = Mathf.Clamp01((zoomDist - nearestZoom) / (furthestZoom - nearestZoom));
            t = 1 - (1 - t) * (1 - t);
            var interpolatedRotation = Quaternion.Slerp(
                m_MaxZoomRotation, m_MinZoomRotation,
                t);
            return interpolatedRotation * Vector3.up;
        }

	    /// <summary>
	    ///     Update the size of our camera's bounding rectangle
	    /// </summary>
	    private void RecalculateBoundingRect()
        {
            var mapsize = mapSize;

            // Get some world space projections at this zoom level
            // Temporarily move camera to final look position
            var prevCameraPos = transform.position;
            transform.position = cameraPosition;
            transform.LookAt(lookPosition);

            // Project screen corners and center
            var bottomLeftScreen = new Vector3(0, 0);
            var topLeftScreen = new Vector3(0, Screen.height);
            var centerScreen = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);

            var bottomLeftWorld = Vector3.zero;
            var topLeftWorld = Vector3.zero;
            var centerWorld = Vector3.zero;
            float dist;

            var ray = cachedCamera.ScreenPointToRay(bottomLeftScreen);
            if (m_FloorPlane.Raycast(ray, out dist)) bottomLeftWorld = ray.GetPoint(dist);

            ray = cachedCamera.ScreenPointToRay(topLeftScreen);
            if (m_FloorPlane.Raycast(ray, out dist)) topLeftWorld = ray.GetPoint(dist);

            ray = cachedCamera.ScreenPointToRay(centerScreen);
            if (m_FloorPlane.Raycast(ray, out dist)) centerWorld = ray.GetPoint(dist);

            var toTopLeft = topLeftWorld - centerWorld;
            var toBottomLeft = bottomLeftWorld - centerWorld;

            lookBounds = new Rect(
                mapsize.xMin - toBottomLeft.x,
                mapsize.yMin - toBottomLeft.z,
                Mathf.Max(mapsize.width + toBottomLeft.x * 2, 0),
                Mathf.Max(mapsize.height - toTopLeft.z + toBottomLeft.z, 0));

            // Restore camera position
            transform.position = prevCameraPos;
            transform.LookAt(currentLookPosition);
        }
    }
}