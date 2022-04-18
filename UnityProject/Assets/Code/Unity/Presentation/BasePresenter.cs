using UnityEngine;
using UnityEngine.UIElements;

namespace CTProject.Unity.Presentation
{
    public class BasePresenter : MonoBehaviour
    {
        #region properties

        public virtual string ViewName => "Unnamed view";

        protected virtual bool IsVisible => view?.visible ?? false;
        protected virtual bool IsDirty { get; set; }

        #endregion properties

        #region fields

        // set from Unity
        [SerializeField]
        protected VisualTreeAsset viewAsset;

        protected VisualElement view;

        #endregion fields

        #region Unity calls

        private void LateUpdate()
        {
            if (IsVisible && IsDirty)
            {
                IsDirty = false;
                RegenerateView();
            }
        }

        #endregion Unity calls

        #region Presenter methods

        public virtual void InstantiateView(VisualElement rootElement)
        {
            view = viewAsset.CloneTree().Q(className: "viewContainer");
            rootElement.Add(view);
            PrepareView();
        }

        public virtual void PrepareView()
        {
        }

        public virtual void RegenerateView()
        {
        }

        public virtual void Show()
        {
            view.visible = true;
            view.style.display = DisplayStyle.Flex;
        }

        public virtual void Hide()
        {
            view.visible = false;
            view.style.display = DisplayStyle.None;
        }

        #endregion Presenter methods
    }
}
