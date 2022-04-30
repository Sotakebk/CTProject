using UnityEngine;
using UnityEngine.UIElements;

namespace CTProject.Unity.Presentation
{
    public class DiagramPresenter : BasePresenter
    {
        #region properties

        public override string ViewName => "See diagram";

        #endregion properties

        #region fields

        // set from Unity
        [SerializeField]
        private WorldSpaceController cameraController;

        [SerializeField]
        private GraphicsService graphicsService;

        #endregion fields

        #region UI elements

        private Toggle trackNewDataToggle;

        private Slider lineWidthSlider;
        private Slider timeScaleSlider;
        private Slider timePositionSlider;
        private Slider graphScaleSlider;
        private Slider graphPositionSlider;

        private Label endTimeLabel;

        #endregion UI elements

        #region Unity calls

        private void Awake()
        {
            graphicsService.PropertyChanged += GraphicsService_PropertyChanged;
        }

        #endregion Unity calls

        #region Presenter

        public override void PrepareView()
        {
            trackNewDataToggle = view.Q<Toggle>(name: "trackNewDataToggle");

            lineWidthSlider = view.Q<Slider>(name: "lineWidthSlider");
            timeScaleSlider = view.Q<Slider>(name: "timeScaleSlider");
            timePositionSlider = view.Q<Slider>(name: "timePositionSlider");
            graphScaleSlider = view.Q<Slider>(name: "graphScaleSlider");
            graphPositionSlider = view.Q<Slider>(name: "graphPositionSlider");

            endTimeLabel = view.Q<Label>(name: "endTimeLabel");

            trackNewDataToggle.RegisterValueChangedCallback(OnTrackNewDataValueChangedCallback);

            lineWidthSlider.RegisterValueChangedCallback(OnLineWidthValueChangedCallback);
            timeScaleSlider.RegisterValueChangedCallback(OnTimeScaleValueChangedCallback);
            timePositionSlider.RegisterValueChangedCallback(OnTimePositionValueChangedCallback);
            graphScaleSlider.RegisterValueChangedCallback(OnGraphScaleValueChangedCallback);
            graphPositionSlider.RegisterValueChangedCallback(OnGraphPositionValueChangedCallback);

            trackNewDataToggle.value = true;
        }

        #endregion Presenter

        #region UI commands and callbacks

        private void OnGraphPositionValueChangedCallback(ChangeEvent<float> evt)
        {
            var range = Mathf.Abs(graphicsService.MinValue - graphicsService.MaxValue);
            cameraController.TargetHeightPosition = (graphicsService.MinValue + range * evt.newValue) * graphicsService.DiagramScale;
        }

        private void OnGraphScaleValueChangedCallback(ChangeEvent<float> evt)
        {
            graphicsService.DiagramScale = 1f / Mathf.Exp(-evt.newValue + 5f);
        }

        private void OnTimePositionValueChangedCallback(ChangeEvent<float> evt)
        {
            trackNewDataToggle.value = false;
            cameraController.TargetTimePosition = evt.newValue;
        }

        private void OnTimeScaleValueChangedCallback(ChangeEvent<float> evt)
        {
            graphicsService.TimeScale = Mathf.Clamp(1 / evt.newValue, 0.0001f, 10000f);
        }

        private void OnLineWidthValueChangedCallback(ChangeEvent<float> evt)
        {
            graphicsService.LineWidthMultiplier = evt.newValue;
        }

        private void OnTrackNewDataValueChangedCallback(ChangeEvent<bool> evt)
        {
            cameraController.TrackNewData = evt.newValue;
        }

        #endregion UI commands and callbacks

        #region private methods

        private void GraphicsService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GraphicsService.MaxTime))
            {
                timePositionSlider.highValue = graphicsService.MaxTime;
                if (trackNewDataToggle.value)
                    timePositionSlider.SetValueWithoutNotify(graphicsService.MaxTime);

                endTimeLabel.text = $"{graphicsService.MaxTime:0.00}s";
            }
        }

        #endregion private methods
    }
}
