using CTProject.Infrastructure;
using System.Threading.Tasks;
using UnityEngine;

namespace CTProject.Unity
{
    public class WorldSpaceController : MonoBehaviour, IDataConsumer
    {
        #region properties

        public bool TrackNewData { get; set; }
        public float TargetTimePosition { get; set; }
        public float TargetHeightPosition { get; set; }

        #endregion properties

        #region fields

        // set from Unity
        [SerializeField]
        private Transform horizontalScroller;

        [SerializeField]
        private Transform verticalScroller;

        [SerializeField]
        private Transform realValueRangeElement;

        [SerializeField]
        private Transform expectedValueRangeElement;

        [SerializeField]
        private GraphicsService graphicsService;

        private bool going;

        public float maxValue = 0;
        public float minValue = 0;

        #endregion fields

        #region Unity calls

        private void Update()
        {
            if (TrackNewData && going)
                TargetTimePosition += Time.deltaTime;

            horizontalScroller.localPosition = new Vector3()
            {
                x = Mathf.Lerp(horizontalScroller.localPosition.x, graphicsService.SecondsToPosition(TargetTimePosition), 0.3f)
            };

            verticalScroller.localPosition = new Vector3()
            {
                y = Mathf.Lerp(verticalScroller.localPosition.y, TargetHeightPosition, 0.1f)
            };

            if (minValue != float.MaxValue && maxValue != float.MinValue)
            {
                var average = (minValue + maxValue) / 2f;
                var range = Mathf.Abs(maxValue - minValue);

                realValueRangeElement.localPosition = new Vector3()
                {
                    y = average * graphicsService.DiagramScale,
                    z = 90f
                };
                realValueRangeElement.localScale = new Vector3()
                {
                    x = 9999f,
                    y = range * graphicsService.DiagramScale
                };
            }

            {
                var average = (graphicsService.MinValue + graphicsService.MaxValue) / 2f;
                var range = Mathf.Abs(graphicsService.MaxValue - graphicsService.MinValue);
                expectedValueRangeElement.localPosition = new Vector3()
                {
                    y = average * graphicsService.DiagramScale,
                    z = 100f
                };

                expectedValueRangeElement.localScale = new Vector3()
                {
                    x = 9999f,
                    y = range * graphicsService.DiagramScale
                };
            }
        }

        #endregion Unity calls

        #region IDataConsumer

        public void OnSettingsChange(IDataProvider source)
        {
        }

        public void ReceiveData(ulong index, float[] data)
        {
            if (TrackNewData)
                TargetTimePosition = graphicsService.IndexToSeconds((int)index - data.Length);

            var min = Mathf.Min(data);
            var max = Mathf.Max(data);
            minValue = Mathf.Min(min, minValue);
            maxValue = Mathf.Max(max, maxValue);
        }

        public void DataStreamStarted(long tickCountOnStreamStart)
        {
            minValue = float.MaxValue;
            maxValue = float.MinValue;
            going = true;
        }

        public void DataStreamEnded()
        {
            going = false;
        }

        #endregion IDataConsumer
    }
}
