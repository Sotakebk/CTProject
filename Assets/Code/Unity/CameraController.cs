using CTProject.Infrastructure;
using UnityEngine;

namespace CTProject.Unity
{
    public class CameraController : MonoBehaviour, IDataConsumer
    {
        #region properties

        public bool TrackNewData { get; set; }

        public float TargetTimePosition { get; set; }
        public float TargetHeightPosition { get; set; }

        #endregion properties

        #region fields

        // set from Unity
        [SerializeField]
        private new Camera camera;

        [SerializeField]
        private GraphicsService graphicsService;

        private Rect? cachedViewRect;
        private bool going;

        #endregion fields

        #region Unity calls

        private void Update()
        {
            cachedViewRect = null;

            if (TrackNewData && going)
                TargetTimePosition += Time.deltaTime * graphicsService.TimeScale;

            transform.position = new Vector3(
                Mathf.Lerp(transform.position.x, TargetTimePosition, 0.1f),
                Mathf.Lerp(transform.position.y, TargetHeightPosition, 0.1f),
                transform.position.z);
        }

        #endregion Unity calls

        #region public methods

        public Rect GetCameraVisibilityRect()
        {
            if (cachedViewRect != null)
                return cachedViewRect.Value;

            var a = camera.ViewportToWorldPoint(new Vector3(0, 0));
            var b = camera.ViewportToWorldPoint(new Vector3(1, 1));

            var min = new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
            var max = new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));

            cachedViewRect = new Rect(min, max);
            return cachedViewRect.Value;
        }

        public void ResetIndex()
        {
        }

        public void OnSettingsChange(IDataProvider source)
        {
        }

        public void ReceiveData(ulong index, float[] data)
        {
            if (TrackNewData)
                TargetTimePosition = graphicsService.IndexToSeconds((int)index + data.Length);
        }

        public void DataStreamStarted(long tickCountOnStreamStart)
        {
            going = true;
        }

        public void DataStreamEnded()
        {
            going = false;
        }

        #endregion public methods
    }
}
