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

        private LineRenderer lineRenderer;
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

            lineRenderer = GetComponent<LineRenderer>();
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }

        public void SetDirty()
        {
            isDirty = true;
        }

        #endregion public methods

        #region private methods

        private void RebuildLineSegment()
        {
            var start = Math.Max(0, startIndex - 1);
            var end = Mathf.Min(startIndex + length, dataContainer.CurrentIndex);
            var iterationLength = end - start;
            UpdateLineRendererSettings();
            lineRenderer.positionCount = iterationLength;
            var zOrder = owner.ZOrder;
            var points = new List<Vector3>();
            for (int x = 0; x < iterationLength; x++)
            {
                var position = new Vector3(
                    graphicsService.IndexToSeconds(start + x) * graphicsService.TimeScale,
                    dataContainer.GetPoint(start + x) * graphicsService.DiagramScale,
                    zOrder);
                points.Add(position);
            }

            var pointsAlteredByDistance = new List<Vector3>();
            pointsAlteredByDistance.Add(points[0]);
        
            for (int x = 1; x < points.Count-1; x++)
            {
                var currentPointAltered = false;
                var pointA = points[x];
                var pointB = points[x+1];
                var distance = CountDistanceToNextPoint(pointA, pointB);

                if(distance < 0.01)
                {
                    pointsAlteredByDistance.Add(new Vector3(
                        (pointA.x + pointB.x) / 2,
                        (pointA.y + pointB.y) / 2,
                        (pointA.z + pointB.z) / 2));
                    currentPointAltered = true;
                    x++;
                }

                if(!currentPointAltered)
                {
                    pointsAlteredByDistance.Add(points[x]);
                }
            }
            pointsAlteredByDistance.Add(points[points.Count-1]);

            var pointsAlteredByAngle = new List<Vector3>();
            pointsAlteredByAngle.Add(pointsAlteredByDistance[0]);
            for(int x = 1; x < pointsAlteredByDistance.Count-1; x++)
            {
                var pointA = pointsAlteredByDistance[x-1];
                var pointB = pointsAlteredByDistance[x];
                var pointC = pointsAlteredByDistance[x+1];    
                var angle = CountAngleBetweenPoints(pointA, pointB, pointC);
                var currentPointAltered = false;

                if(angle > 45) 
                {
                    var margin = 1;
                    var scaleAB = (pointB.x - pointA.x) / (pointB.y - pointA.y);

                    pointsAlteredByAngle.Add(new Vector3(
                        (pointB.x - pointA.x) / 2,
                        (pointB.y - pointA.y) / 2,
                        (pointB.z - pointA.z) / 2));
                    pointsAlteredByAngle.Add(new Vector3(
                        (pointC.x - pointB.x) / 2,
                        (pointC.y - pointB.y) / 2,
                        (pointC.z - pointB.z) / 2));
                    
                    currentPointAltered = true;
                }

                if(!currentPointAltered) 
                {
                    pointsAlteredByAngle.Add(pointsAlteredByDistance[0]);
                }
            }
            pointsAlteredByAngle.Add(pointsAlteredByDistance[pointsAlteredByDistance.Count - 1]);

            for (int x = 0; x < pointsAlteredByAngle.Count; x++)
            {
                lineRenderer.SetPosition(x, pointsAlteredByAngle[x]);
            }
        }

        private float CountDistanceToNextPoint(Vector3 position, Vector3 nextPosition) {
            var vectorBetweenPositions = new Vector3(
                nextPosition.x - position.x,
                nextPosition.y - position.y,
                nextPosition.z - position.z);

            return vectorBetweenPositions.magnitude;
        }

        private float CountAngleBetweenPoints(Vector3 pointA, Vector3 pointB, Vector3 pointC) {
            var vectorAB = new Vector3(
                pointB.x - pointA.x,
                pointB.y - pointA.y,
                pointB.z - pointA.z);
            var vectorBC = new Vector3(
                pointC.x - pointB.x,
                pointC.y - pointB.y,
                pointC.z - pointC.z);

            var angle = Mathf.Atan2(vectorBC.y, vectorBC.x) - Mathf.Atan2(vectorAB.y, vectorAB.x);
            return Math.Abs(angle * Mathf.Rad2Deg);
        }
        internal void UpdateLineRendererSettings()
        {
            lineRenderer.widthMultiplier = graphicsService.LineWidthMultiplier;
            lineRenderer.alignment = LineAlignment.View;
        }

        #endregion private methods
    }
}
