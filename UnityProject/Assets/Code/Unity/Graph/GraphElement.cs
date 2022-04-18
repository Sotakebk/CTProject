using System;
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
            for (int x = 0; x < iterationLength; x++)
            {
                // TODO add additional points if the angle is too sharp or the distance is too big
                var position = new Vector3(
                    graphicsService.IndexToSeconds(start + x) * graphicsService.TimeScale,
                    dataContainer.GetPoint(start + x) * graphicsService.DiagramScale,
                    zOrder);
                lineRenderer.SetPosition(x, position);
            }
        }

        internal void UpdateLineRendererSettings()
        {
            lineRenderer.widthMultiplier = graphicsService.LineWidthMultiplier;
            lineRenderer.alignment = LineAlignment.View;
        }

        #endregion private methods
    }
}
