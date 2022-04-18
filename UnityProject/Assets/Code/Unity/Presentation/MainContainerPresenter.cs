using UnityEngine;
using UnityEngine.UIElements;

namespace CTProject.Unity.Presentation
{
    public class MainContainerPresenter : BasePresenter
    {
        #region properties

        public override string ViewName => "Main UI container";

        #endregion properties

        #region fields

        // set from Unity
        [SerializeField]
        private UIDocument uiRoot;

        [SerializeField]
        private BasePresenter[] subViewPresenters;

        [SerializeField]
        private DataSourceStatePresenter statusPresenter;

        #endregion fields

        #region UI elements

        private VisualElement buttonContainer;
        private VisualElement viewContainer;
        private VisualElement statusContainer;

        #endregion UI elements

        #region Unity calls

        private void Awake()
        {
            InstantiateView(null);
            buttonContainer = view.Q(name: "buttonContainer");
            viewContainer = view.Q(name: "viewContainer");

            foreach (var presenter in subViewPresenters)
            {
                presenter.InstantiateView(viewContainer);

                var showViewButton = new Button();
                showViewButton.text = presenter.ViewName;
                showViewButton.clicked += () => OpenView(presenter);
                buttonContainer.Add(showViewButton);
            }
            OpenView(subViewPresenters[0]);

            statusContainer = view.Q(name: "statusContainer");
            statusPresenter.InstantiateView(statusContainer);
        }

        #endregion Unity calls

        #region Presenter

        public override void InstantiateView(VisualElement rootElement)
        {
            uiRoot.visualTreeAsset = viewAsset;
            view = uiRoot.rootVisualElement;
        }

        public override void Show()
        {
        }

        public override void Hide()
        {
        }

        #endregion Presenter

        #region private methods

        private void OpenView(BasePresenter presenter)
        {
            foreach (var p in subViewPresenters)
            {
                if (p == presenter)
                    p.Show();
                else
                    p.Hide();
            }
        }

        #endregion private methods
    }
}
