using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace CTProject.Unity.Presentation
{
    public class NetworkConfigurationPresenter : BasePresenter
    {
        #region Properties

        public override string ViewName => "Network settings";

        private bool isValidPort;

        private bool IsValidPort
        {
            get => isValidPort;
            set
            {
                if (value)
                    portTextField.RemoveFromClassList("invalidTextField");
                else
                    portTextField.AddToClassList("invalidTextField");
            }
        }

        #endregion Properties

        #region fields

        [SerializeField]
        private DAQDataProviderContainer dataProviderContainer;

        #endregion fields

        #region UI elements

        private Button applyButton;
        private TextField portTextField;

        #endregion UI elements

        #region Presenter

        public override void PrepareView()
        {
            applyButton = view.Q<Button>(name: "applyButton");
            portTextField = view.Q<TextField>(name: "portTextField");

            applyButton.clicked += ApplyCommand;
            portTextField.RegisterValueChangedCallback(PortTextFieldValueChanged);
        }

        public override void RegenerateView()
        {
        }

        public override void Show()
        {
            base.Show();
            portTextField.value = dataProviderContainer.Port.ToString();
        }

        private string StripNonNumbers(string text)
        {
            return Regex.Replace(text, "[^0-9]", "");
        }

        private bool ValidatePortFromString(string value, out short port)
        {
            port = 0;
            if (!int.TryParse(value, out int i))
                return false;

            if (i < 0 || i > 65535)
                return false;

            port = (short)i;
            return true;
        }

        #endregion Presenter

        #region UI commands and callbacks

        private void PortTextFieldValueChanged(ChangeEvent<string> evt)
        {
            portTextField.value = StripNonNumbers(portTextField.value);
            IsValidPort = ValidatePortFromString(portTextField.value, out short port);
        }

        private void ApplyCommand()
        {
            if (ValidatePortFromString(portTextField.value, out short port))
            {
                dataProviderContainer.Port = port;
            }
        }

        #endregion UI commands and callbacks
    }
}
