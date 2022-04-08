using UnityEngine;

namespace CTProject.Unity
{
    public class CameraController : MonoBehaviour
    {
        #region properties

        // set from Unity
        [SerializeField]
        private new Camera camera;

        #endregion properties

        #region fields

        private Rect? cachedViewRect;

        #endregion fields

        #region Unity calls

        private void Update()
        {
            cachedViewRect = null;
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

        #endregion public methods
    }
}
