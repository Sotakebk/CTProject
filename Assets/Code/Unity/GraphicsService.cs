using CTProject.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CTProject.Unity
{
    public class GraphicsService : MonoBehaviour, IDataConsumer, INotifyPropertyChanged
    {
        #region properties

        public float MaxValue
        {
            get => maxValue;
            private set => SetField(ref maxValue, value);
        }

        public float MinValue
        {
            get => minValue;
            private set => SetField(ref minValue, value);
        }

        public uint SamplingRate
        {
            get => samplingRate;
            private set => SetField(ref samplingRate, value);
        }

        public uint BufferSize
        {
            get => bufferSize;
            private set => SetField(ref bufferSize, value);
        }

        public float LineWidthMultiplier
        {
            get => lineWidthMultiplier;
            set => SetField(ref lineWidthMultiplier, value);
        }

        public float TimeScale
        {
            get => timeScale;
            set => SetField(ref timeScale, value);
        }

        public float DiagramScale
        {
            get => diagramScale;
            set => SetField(ref diagramScale, value);
        }

        public float MaxTime
        {
            get => maxTime;
            private set => SetField(ref maxTime, value);
        }

        #endregion properties

        #region fields

        private float maxValue;
        private float minValue;
        private uint samplingRate;
        private uint bufferSize;
        private float lineWidthMultiplier = 0.3f;
        private float timeScale = 1f;
        private float diagramScale = 1f;
        private float maxTime = 0f;

        #endregion fields

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion INotifyPropertyChanged

        #region public methods

        public float IndexToSeconds(int index)
        {
            return (float)(index / (double)SamplingRate) * TimeScale;
        }

        #endregion public methods

        #region IDataConsumer

        public void DataStreamEnded()
        {
        }

        public void DataStreamStarted(long tickCountOnStreamStart)
        {
            MaxTime = 0;
        }

        public void OnSettingsChange(IDataProvider source)
        {
            MaxValue = source.GetMaxValue();
            MinValue = source.GetMinValue();
            SamplingRate = source.SelectedSamplingRate;
            BufferSize = source.SelectedBufferSize;
        }

        public void ReceiveData(ulong index, float[] data)
        {
            MaxTime = ((int)index + data.Length) / ((float)SamplingRate);
        }

        #endregion IDataConsumer
    }
}
