using CTProject.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace CTProject.Unity.Presentation
{
    public class SettingsPresenter : BasePresenter, IDataConsumer
    {
        #region properties

        public override string ViewName => "Settings";

        #endregion properties

        #region fields

        // set from Unity
        [SerializeField]
        private DataSourceContainerAggregate dataSourceContainerAggregate;

        [SerializeField]
        private DataBroadcaster dataBroadcaster;

        private IDataProviderContainer selectedDataProviderContainer;

        #endregion fields

        #region UI elements

        private DropdownField selectedDataSourceDropdown;

        private VisualElement dataSourceSettings;
        private DropdownField selectedChannelDropdown;
        private DropdownField selectedBufferSizeDropdown;
        private DropdownField selectedSamplingRateDropdown;

        #endregion UI elements

        #region Presenter

        public override void PrepareView()
        {
            selectedDataSourceDropdown = view.Q<DropdownField>(name: "dataSourceDropdown");

            dataSourceSettings = view.Q(name: "dataSourceSettings");
            selectedChannelDropdown = view.Q<DropdownField>(name: "selectedChannelDropdown");
            selectedBufferSizeDropdown = view.Q<DropdownField>(name: "selectedBufferSizeDropdown");
            selectedSamplingRateDropdown = view.Q<DropdownField>(name: "selectedSamplingRateDropdown");

            selectedDataSourceDropdown.RegisterValueChangedCallback(OnSelectedDataSourceChangedCallback);
            selectedChannelDropdown.RegisterValueChangedCallback(OnSelectedChannelChangedCallback);
            selectedBufferSizeDropdown.RegisterValueChangedCallback(OnSelectedBufferSizeChangedCallback);
            selectedSamplingRateDropdown.RegisterValueChangedCallback(OnSelectedSamplingRateChangedCallback);

            var availableDataSources = new List<string>(new[] { "None" });
            availableDataSources.AddRange(dataSourceContainerAggregate.DataProviderContainers
                .Select(provider => provider.DataProviderName)
                .ToList());

            selectedDataSourceDropdown.choices = availableDataSources;
            selectedDataSourceDropdown.value = availableDataSources.First();
            IsDirty = true;
        }

        public override void RegenerateView()
        {
            var provider = dataBroadcaster.DataProvider;
            if (provider == null)
            {
                dataSourceSettings.visible = false;
                return;
            }

            dataSourceSettings.visible = true;
            var channels = provider.GetAvailableChannels();
            selectedChannelDropdown.choices = channels?.Select(c => c.UniqueName)?.ToList();
            selectedChannelDropdown.value = channels?.FirstOrDefault(c => c.UniqueName == provider.SelectedChannel?.UniqueName)?.UniqueName ?? string.Empty;

            var bufferSizes = provider.GetAvailableBufferSizes();
            selectedBufferSizeDropdown.choices = bufferSizes?.Select(b => b.ToString())?.ToList();
            selectedBufferSizeDropdown.value = bufferSizes?.FirstOrDefault(b => b.ToString() == provider.SelectedBufferSize.ToString()).ToString() ?? string.Empty;

            var samplingRates = provider.GetAvailableSamplingRates();
            selectedSamplingRateDropdown.choices = samplingRates?.Select(s => s.ToString())?.ToList();
            selectedSamplingRateDropdown.value = samplingRates?.FirstOrDefault(s => s.ToString() == provider.SelectedSamplingRate.ToString()).ToString() ?? string.Empty;
        }

        #endregion Presenter

        #region UI commands and callbacks

        private void OnSelectedDataSourceChangedCallback(ChangeEvent<string> evt)
        {
            var newProviderContainer = dataSourceContainerAggregate.DataProviderContainers
                .FirstOrDefault(p => p.DataProviderName == evt.newValue);

            if (newProviderContainer == selectedDataProviderContainer)
                return;

            selectedDataProviderContainer = newProviderContainer;
            dataBroadcaster.DataProvider = newProviderContainer?.GetDataProvider();

            IsDirty = true;
        }

        private void OnSelectedChannelChangedCallback(ChangeEvent<string> evt)
        {
            var provider = dataBroadcaster.DataProvider;
            if (provider == null)
                return;

            provider.SelectedChannel = provider.GetAvailableChannels()?.FirstOrDefault(c => c.UniqueName == evt.newValue);
        }

        private void OnSelectedBufferSizeChangedCallback(ChangeEvent<string> evt)
        {
            var provider = dataBroadcaster.DataProvider;
            if (provider == null)
                return;

            provider.SelectedBufferSize = provider.GetAvailableBufferSizes()?.FirstOrDefault(c => c.ToString() == evt.newValue) ?? 0;
        }

        private void OnSelectedSamplingRateChangedCallback(ChangeEvent<string> evt)
        {
            var provider = dataBroadcaster.DataProvider;
            if (provider == null)
                return;

            provider.SelectedSamplingRate = provider.GetAvailableSamplingRates()?.FirstOrDefault(c => c.ToString() == evt.newValue) ?? 0;
        }

        #endregion UI commands and callbacks

        #region IDataConsumer

        public void OnSettingsChange(IDataProvider source) => IsDirty = true;

        public void ReceiveData(ulong index, float[] data)
        {
        }

        public void DataStreamStarted(long tickCountOnStreamStart)
        {
        }

        public void DataStreamEnded()
        {
        }

        #endregion IDataConsumer
    }
}
