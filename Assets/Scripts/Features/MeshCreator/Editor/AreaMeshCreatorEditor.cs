using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Features.MeshCreator.Editor
{
    [CustomEditor(typeof(AreaMeshCreator))]
    public class AreaMeshCreatorEditor : UnityEditor.Editor
    {
        protected AreaMeshCreator m_AreaMeshCreator;
        protected MeshObject m_CurrentMeshObject;
        protected float m_OctagonRadius = 1;

        protected float m_SquareSideLength = 1;

        /// <summary>
        ///     Recreates the mesh when this script becomes active
        /// </summary>
        protected void OnEnable()
        {
            m_AreaMeshCreator = (AreaMeshCreator)target;
            CreateMesh();
        }

        /// <summary>
        ///     Draws and handles input for manipulating the mesh in scene
        /// </summary>
        protected void OnSceneGUI()
        {
            if (m_AreaMeshCreator.pointsTransforms == null || m_AreaMeshCreator.pointsTransforms.Length < 3)
                SetSquare(1);

            if (Event.current.shift && m_AreaMeshCreator.GetPoints().Count > 3)
            {
                var allPoints = m_AreaMeshCreator.GetPoints();
                var plane = new Plane(allPoints[0], allPoints[1], allPoints[2]);
                var mousePos = Event.current.mousePosition;

                var ray = HandleUtility.GUIPointToWorldRay(mousePos);
                float rayDistance;
                if (plane.Raycast(ray, out rayDistance))
                {
                    var closestPoint = GetClosetsPoint(ray.GetPoint(rayDistance));

                    if (DeleteButton(closestPoint.position)) DeletePoint(closestPoint);
                }
            }
            else
            {
                var points = m_AreaMeshCreator.pointsTransforms;
                if (points == null) return;

                var length = points.Length;
                for (var i = 0; i < length; i++)
                {
                    var t = points[i];
                    //	Vector3 newPosition = Handles.PositionHandle(t.position, Quaternion.identity);

                    var size = HandleUtility.GetHandleSize(t.position) * 0.125f;
                    var snap = Vector3.one * 0.5f;
                    var fmh_192_63_638806647511718287 = Quaternion.LookRotation(Vector3.up);
                    var newPosition = Handles.FreeMoveHandle(t.position, size, snap,
                        Handles.RectangleHandleCap);
                    newPosition.y = t.position.y;

                    if (newPosition != t.position)
                    {
                        t.position = newPosition;
                        Undo.RecordObject(t, string.Format("Moved {0}", t.name));
                        EditorUtility.SetDirty(t);
                        CreateMesh();
                    }
                }

                var allPoints = m_AreaMeshCreator.GetPoints();
                var plane = new Plane(allPoints[0], allPoints[1], allPoints[2]);
                var mousePos = Event.current.mousePosition;

                var ray = HandleUtility.GUIPointToWorldRay(mousePos);
                float rayDistance;
                if (plane.Raycast(ray, out rayDistance))
                {
                    var closestPoints = GetClosestTwoPoints(ray.GetPoint(rayDistance));
                    var position = Midpoint(closestPoints[0].position, closestPoints[1].position);

                    if (AddButton(position)) AddPoint(closestPoints[0], closestPoints[1]);
                }

                ForcePointsFlat();
            }

            // maintain selection of object
            Selection.activeGameObject = m_AreaMeshCreator.gameObject;
        }

        /// <summary>
        ///     Inspector GUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            ForcePointsFlat();

            GUILayout.Space(10.0f);

            EditorGUILayout.HelpBox("SHIFT: Delete Points", MessageType.Info);

            GUILayout.Space(10.0f);

            GUILayout.Label("Preset Shapes");
            GUILayout.Label("Square");
            m_SquareSideLength = EditorGUILayout.FloatField("Side Length", m_SquareSideLength);
            if (GUILayout.Button("Square")) SetSquare(m_SquareSideLength);

            GUILayout.Space(10.0f);
            GUILayout.Label("Octagon");
            m_OctagonRadius = EditorGUILayout.FloatField("Radius", m_OctagonRadius);
            if (GUILayout.Button("Octagon")) SetOctagon(m_OctagonRadius);

            GUILayout.EndVertical();
        }

        /// <summary>
        ///     Creates the mesh from the points currently in the mesh creator
        /// </summary>
        protected void CreateMesh()
        {
            var vertices3D = m_AreaMeshCreator.GetPoints();
            var vertices2D = new Vector2[vertices3D.Count];
            for (var i = 0; i < vertices3D.Count; i++)
            {
                var v = m_AreaMeshCreator.transform.InverseTransformPoint(vertices3D[i]);
                vertices2D[i] = new Vector2(v.x, v.z);
            }

            // Use the triangulator to get indices for creating triangles
            var tr = new Triangulator(vertices2D);
            var indices = tr.Triangulate();

            // Create the Vector3 vertices
            var vertices = new Vector3[vertices2D.Length];
            for (var i = 0; i < vertices.Length; i++) vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);

            // Create the mesh
            var msh = new Mesh();
            msh.vertices = vertices;
            msh.triangles = indices;
            msh.RecalculateNormals();
            msh.RecalculateBounds();

            var filter = m_AreaMeshCreator.GetComponent<MeshFilter>();
            if (m_AreaMeshCreator.GetComponent<MeshFilter>() == null)
                filter = m_AreaMeshCreator.gameObject.AddComponent<MeshFilter>();
            filter.mesh = msh;

            var mesh = msh;
            var numberTriangles = mesh.triangles.Length / 3;
            var triangles = mesh.triangles;
            var trianglesList = new List<Triangle>();

            for (var i = 0; i < numberTriangles; i++)
            {
                var v0 = mesh.vertices[triangles[i * 3]];
                var v1 = mesh.vertices[triangles[i * 3 + 1]];
                var v2 = mesh.vertices[triangles[i * 3 + 2]];
                trianglesList.Add(new Triangle(v0, v1, v2));
            }

            m_AreaMeshCreator.meshObject = new MeshObject(trianglesList);
        }

        /// <summary>
        ///     Makes points coplanar
        /// </summary>
        protected void ForcePointsFlat()
        {
            m_AreaMeshCreator.ForcePointsFlat();
        }

        /// <summary>
        ///     Adds a new point at the midpoint of 2 other points
        /// </summary>
        /// <param name="point1">First point</param>
        /// <param name="point2">Second point</param>
        protected void AddPoint(Transform point1, Transform point2)
        {
            Vector3 first = point1.position, last = point2.position, midpoint = Midpoint(first, last);

            var p = Instantiate(m_AreaMeshCreator.pointsTransforms[0].gameObject, midpoint, Quaternion.identity);
            p.name = "point";

            p.transform.SetParent(m_AreaMeshCreator.transform.GetChild(0));
            var index = Mathf.Min(point1.GetSiblingIndex(), point2.GetSiblingIndex()) + 1;
            if (index == 1 && (point1.GetSiblingIndex() == m_AreaMeshCreator.pointsTransforms.Length - 2 ||
                               point2.GetSiblingIndex() == m_AreaMeshCreator.pointsTransforms.Length - 2))
                p.transform.SetAsLastSibling();
            else
                p.transform.SetSiblingIndex(index);
            CreateMesh();

            Undo.RegisterCreatedObjectUndo(p, "Created point");
        }

        /// <summary>
        ///     Gets the 2 closest points to the specified (cursor) position
        /// </summary>
        /// <param name="position">The position of the cursor</param>
        /// <returns>The 2 closest points to the cursor</returns>
        protected Transform[] GetClosestTwoPoints(Vector3 position)
        {
            var points = new Transform[2];
            points[0] = GetClosetsPoint(position);

            int index = points[0].GetSiblingIndex(), length = m_AreaMeshCreator.pointsTransforms.Length;
            var previousIndex = index > 0 ? index - 1 : length - 1;
            var previous = points[0].parent.GetChild(previousIndex);
            var nextIndex = index < length - 1 ? index + 1 : 0;
            var next = points[0].parent.GetChild(nextIndex);

            var previousDistance = Vector3.Distance(previous.position, position);
            var nextDistance = Vector3.Distance(next.position, position);

            points[1] = previousDistance < nextDistance ? previous : next;

            return points;
        }

        /// <summary>
        ///     Finds the closest point to the specified (cursor) position
        /// </summary>
        /// <param name="position">The position of the cursor</param>
        /// <returns>The closest point to the cursor</returns>
        protected Transform GetClosetsPoint(Vector3 position)
        {
            var ordererPoints = m_AreaMeshCreator.pointsTransforms.OrderBy(x => Vector3.Distance(x.position, position))
                .ToArray();
            return ordererPoints[0];
        }

        /// <summary>
        ///     Deletes the selected point Transform.
        /// </summary>
        protected void DeletePoint(Transform point)
        {
            Undo.DestroyObjectImmediate(point.gameObject);
            CreateMesh();
        }

        /// <summary>
        ///     Gets the midpoint between 2 Vector3's
        /// </summary>
        /// <param name="first">First point</param>
        /// <param name="last">Last point</param>
        protected static Vector3 Midpoint(Vector3 first, Vector3 last)
        {
            return (first + last) * 0.5f;
        }

        /// <summary>
        ///     Destroys the current points in the mesh
        /// </summary>
        protected void ClearCurrentPoints()
        {
            var points = m_AreaMeshCreator.pointsTransforms;
            var length = points.Length;
            for (var i = 0; i < length; i++) Undo.DestroyObjectImmediate(points[i].gameObject);
        }

        /// <summary>
        ///     Creates a new point at the specified position
        /// </summary>
        /// <param name="position">Position to create the point at</param>
        protected void CreateNewPoint(Vector3 position)
        {
            var point = new GameObject("point");
            point.transform.position = position;
            point.transform.SetParent(m_AreaMeshCreator.pointsCenter);
            Undo.RegisterCreatedObjectUndo(point, "Created point");
        }

        /// <summary>
        ///     Creates a basic square
        /// </summary>
        /// <param name="sideLength">Length of the sides of the square</param>
        protected void SetSquare(float sideLength)
        {
            ClearCurrentPoints();
            var center = m_AreaMeshCreator.pointsCenter.position;
            var halfSide = sideLength / 2f;
            CreateNewPoint(center + new Vector3(halfSide, 0, halfSide));
            CreateNewPoint(center + new Vector3(-halfSide, 0, halfSide));
            CreateNewPoint(center + new Vector3(-halfSide, 0, -halfSide));
            CreateNewPoint(center + new Vector3(halfSide, 0, -halfSide));
            CreateMesh();
        }

        /// <summary>
        ///     Creates an octagon
        /// </summary>
        /// <param name="radius">Radius of the Octagon</param>
        protected void SetOctagon(float radius)
        {
            ClearCurrentPoints();
            var center = m_AreaMeshCreator.pointsCenter.position;
            for (var i = 0; i < 8; i++)
            {
                var angle = 2 * Mathf.PI * (i + 1) / 8;
                var x = center.x + radius * Mathf.Cos(angle);
                var y = center.z + radius * Mathf.Sin(angle);
                CreateNewPoint(new Vector3(x, 0, y));
            }

            CreateMesh();
        }

        private bool AddButton(Vector3 position)
        {
            return HandleButton(position, "ADD", 50, 25);
        }

        private bool DeleteButton(Vector3 position)
        {
            return HandleButton(position, "DELETE", 50, 25);
        }

        private bool HandleButton(Vector3 position, string text, float width, float height)
        {
            var pos2D = HandleUtility.WorldToGUIPoint(position);

            Handles.BeginGUI();
            var clicked = GUI.Button(new Rect(pos2D.x - width * 0.5f, pos2D.y - height * 0.5f, width, height), text);
            Handles.EndGUI();

            HandleUtility.Repaint();
            return clicked;
        }
    }
}