using CTProject.Infrastructure;
using UnityEngine;
using UnityEngine.UIElements;

namespace CTProject.Unity.Presentation
{
    public class DataSourceStatePresenter : BasePresenter
    {
        #region Properties

        public override string ViewName => "Data source state view";

        #endregion Properties

        #region fields

        [SerializeField]
        private DataBroadcaster dataBroadcaster;

        private DataProviderState? dataProviderState;

        #endregion fields

        #region UI elements

        private VisualElement dot;
        private Button playButton;
        private Button pauseButton;

        #endregion UI elements

        #region Unity calls

        protected void Update()
        {
            DataProviderState? newValue = DataProviderState.Uninitialized;
            if (dataBroadcaster != null)
                newValue = dataBroadcaster.DataProvider?.State;

            if (dataProviderState != newValue)
            {
                dataProviderState = newValue;
                IsDirty = true;
            }
        }

        #endregion Unity calls

        #region Presenter

        public override void PrepareView()
        {
            dot = view.Q(name: "statusDot");
            playButton = view.Q<Button>(name: "playButton");
            pauseButton = view.Q<Button>(name: "pauseButton");

            playButton.clicked += PlayCommand;
            pauseButton.clicked += StopCommand;

            IsDirty = true;
        }

        public override void RegenerateView()
        {
            dot.style.backgroundColor = GetDotColor();
        }

        public override void Show()
        {
        }

        public override void Hide()
        {
        }

        #endregion Presenter

        #region private methods

        private Color GetDotColor()
        {
            return dataProviderState switch
            {
                DataProviderState.Uninitialized => Color.magenta,
                DataProviderState.Ready => Color.blue,
                DataProviderState.NotReady => new Color(75f / 255f, 0, 130f / 255f),
                DataProviderState.Working => Color.green,
                DataProviderState.Error => Color.red,
                _ => Color.black,
            };
        }

        #endregion private methods

        #region UI commands and callbacks

        private void PlayCommand()
        {
            if (dataBroadcaster.DataProvider?.State == DataProviderState.Ready)
            {
                dataBroadcaster.DataProvider?.Start();
            }
        }

        private void StopCommand()
        {
            if (dataBroadcaster.DataProvider?.State == DataProviderState.Working)
                dataBroadcaster.DataProvider?.Stop();
        }

        #endregion UI commands and callbacks
    }
}
