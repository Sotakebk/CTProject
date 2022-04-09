using System.Collections.Generic;
using UnityEngine;

namespace CTProject.Unity.Graph
{
    public class GraphPresenter : MonoBehaviour
    {
        #region properties

        public int GraphElementRerenderCounter { get; set; }
        public int ZOrder => zOrder;

        #endregion properties

        #region fields

        //set from Unity
        [SerializeField]
        private GraphicsService graphicsService;

        [SerializeField]
        private Material lineRendererMaterial;

        [SerializeField]
        private int zOrder = 10;

        private GraphDataContainer dataContainer;

        private List<GraphElement> graphElements;

        #endregion fields

        #region Unity calls

        private void Awake()
        {
            dataContainer = new GraphDataContainer();
            graphElements = new List<GraphElement>();

            graphicsService.PropertyChanged += GraphicsService_PropertyChanged;
        }

        private void Update()
        {
            GraphElementRerenderCounter = 5;
        }

        #endregion Unity calls

        #region public methods

        public virtual void RebuildEntireGraph()
        {
            foreach (var graphElement in graphElements)
            {
                graphElement.SetDirty();
            }
        }

        public virtual void Clear()
        {
            dataContainer.Clear();
            foreach (var graphElement in graphElements)
            {
                graphElement.DestroySelf();
            }
            graphElements = new List<GraphElement>();
        }

        public virtual void InsertData(int index, float[] data)
        {
            dataContainer.InsertData(index, data);
            var minIndex = FastIndexToBufferIndex(index);
            var maxIndex = FastIndexToBufferIndex(index + data.Length);

            for (var x = minIndex; x <= maxIndex; x++)
            {
                if (x >= graphElements.Count)
                {
                    var newGraphElement = NewGraphElement();
                    newGraphElement.Prepare(this, dataContainer, graphicsService, x * graphElementLength, graphElementLength);
                    graphElements.Add(newGraphElement);
                }

                graphElements[x].SetDirty();
            }
        }

        #endregion public methods

        #region private methods

        private void GraphicsService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(GraphicsService.LineWidthMultiplier):
                    graphElements.ForEach(g => g.UpdateLineRendererSettings());
                    return;

                case nameof(GraphicsService.TimeScale):
                    RebuildEntireGraph();
                    return;

                case nameof(GraphicsService.DiagramScale):
                    RebuildEntireGraph();
                    return;
            }
        }

        private GraphElement NewGraphElement()
        {
            var gameObject = new GameObject("graph element",
                new System.Type[] { typeof(GraphElement), typeof(LineRenderer) });
            var lineRenderer = gameObject.GetComponent<LineRenderer>();
            lineRenderer.material = lineRendererMaterial;
            gameObject.transform.parent = transform;
            return gameObject.GetComponent<GraphElement>();
        }

        private const int graphElementLength = 1024;
        private const int fastModuloValue = 0b1111111111;

        private int FastModulo(int value)
        {
            return value & fastModuloValue;
        }

        private int FastIndexToBufferIndex(int value)
        {
            return value >> 10;
        }

        #endregion private methods
    }
}
