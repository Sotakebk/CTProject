using System;
using System.Collections.Generic;
using UnityEngine;

namespace CTProject.Unity.Graph
{
    public class GraphElement : MonoBehaviour
    {
        #region fields

        private GraphPresenter owner;
        private GraphicsService graphicsService;
        private GraphDataContainer dataContainer;
        private int startIndex;
        private int length;

        private Mesh mesh;

        private MeshFilter meshFilter;
        private bool isDirty;

        #endregion fields

        #region Unity calls

        private void LateUpdate()
        {
            if (isDirty && owner.GraphElementRerenderCounter > 0)
            {
                RebuildLineSegment();

                owner.GraphElementRerenderCounter--;
                isDirty = false;
            }
        }

        #endregion Unity calls

        #region public methods

        public void Prepare(GraphPresenter owner, GraphDataContainer dataContainer, GraphicsService graphicsService,
            int startIndex, int length)
        {
            this.owner = owner;
            this.graphicsService = graphicsService;
            this.dataContainer = dataContainer;
            this.startIndex = startIndex;
            this.length = length;

            mesh = new Mesh();

            gameObject.AddComponent<MeshFilter>().mesh = mesh;
            gameObject.AddComponent<MeshRenderer>().material = owner.GraphMaterial;
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
            Destroy(mesh);
        }

        public void SetDirty()
        {
            isDirty = true;
        }

        #endregion public methods

        #region private methods

        private void RebuildLineSegment()
        {
            mesh.Clear();

            var start = Math.Max(0, startIndex - 1);
            var end = Mathf.Min(startIndex + length, dataContainer.CurrentIndex);
            var lineCount = (end - start - 1);
            var lineWidth = (graphicsService.LineWidthMultiplier / 2f);

            var points = new List<Vector3>(lineCount * 4);
            var indices = new List<int>(lineCount * 6);
            var zOrder = owner.ZOrder;

            void DoLine(Vector3 a, Vector3 b)
            {
                var normal = Vector3.Cross(b - a, Vector3.forward).normalized * lineWidth;

                Vector3 pA1 = a + normal;
                Vector3 pA2 = a - normal;
                Vector3 pB1 = b + normal;
                Vector3 pB2 = b - normal;

                var i = points.Count;

                points.Add(pA1);
                points.Add(pA2);
                points.Add(pB1);
                points.Add(pB2);

                indices.Add(i + 0);
                indices.Add(i + 1);
                indices.Add(i + 2);
                indices.Add(i + 3);
                indices.Add(i + 2);
                indices.Add(i + 1);
            }

            void DoPoint(Vector3 a, Vector3 b, Vector3 c)
            {
                var normal1 = Vector3.Cross(b - a, Vector3.forward).normalized * lineWidth;
                var normal2 = Vector3.Cross(c - b, Vector3.forward).normalized * lineWidth;

                Vector3 p1 = b + normal1;
                Vector3 p2 = b - normal1;
                Vector3 p3 = b + normal2;
                Vector3 p4 = b - normal2;

                var i = points.Count;

                points.Add(p1);
                points.Add(p2);
                points.Add(p3);
                points.Add(p4);

                indices.Add(i + 0);
                indices.Add(i + 1);
                indices.Add(i + 2);
                indices.Add(i + 3);
                indices.Add(i + 2);
                indices.Add(i + 1);
            }

            for (int x = 0; x < lineCount; x++)
            {
                var pointB = new Vector3(
                    graphicsService.IndexToSeconds(start + x - 1) * graphicsService.TimeScale,
                    dataContainer.GetPoint(start + x - 1) * graphicsService.DiagramScale,
                    zOrder);

                var pointT = new Vector3(
                    graphicsService.IndexToSeconds(start + x) * graphicsService.TimeScale,
                    dataContainer.GetPoint(start + x) * graphicsService.DiagramScale,
                    zOrder);

                var pointN = new Vector3(
                    graphicsService.IndexToSeconds(start + x + 1) * graphicsService.TimeScale,
                    dataContainer.GetPoint(start + x + 1) * graphicsService.DiagramScale,
                    zOrder);
                DoPoint(pointB, pointT, pointN);
                DoLine(pointT, pointN);
            }
            mesh.SetVertices(points);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
        }

        #endregion private methods
    }
}
