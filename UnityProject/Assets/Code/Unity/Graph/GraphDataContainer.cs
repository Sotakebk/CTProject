using System;
using System.Collections.Generic;

namespace CTProject.Unity.Graph
{
    public class GraphDataContainer
    {
        #region properties

        public int CurrentIndex { get; private set; }

        #endregion properties

        #region fields

        private List<float[]> dataBlocks;

        #endregion fields

        #region ctor

        public GraphDataContainer()
        {
            CurrentIndex = 0;
            dataBlocks = new List<float[]>();
        }

        #endregion ctor

        #region public methods

        public virtual void Clear()
        {
            if (CurrentIndex == 0)
                return;

            CurrentIndex = 0;
            dataBlocks = new List<float[]>();
        }

        public virtual void InsertData(int index, float[] buffer)
        {
            var bufferSize = buffer.Length;
            for (int x = 0; x < bufferSize; x++)
            {
                SetPoint(x + index, buffer[x]);
            }
            CurrentIndex = index + bufferSize;
        }

        public virtual float GetPoint(int index)
        {
            var bufferID = FastModulo(index);
            if (bufferID >= dataBlocks.Count)
                return 0;

            var block = dataBlocks[bufferID];

            return block[FastIndexToBufferIndex(index)];
        }

        #endregion public methods

        #region private methods

        private const int internalBufferSize = 2048;
        private const int fastModuloValue = 0b11111111111;

        private void SetPoint(int index, float value)
        {
            if (index < 0)
                throw new ArgumentException(nameof(index));

            var bufferID = FastModulo(index);
            while (bufferID >= dataBlocks.Count)
                dataBlocks.Add(new float[internalBufferSize]);

            var block = dataBlocks[bufferID];

            block[FastIndexToBufferIndex(index)] = value;
        }

        private int FastModulo(int value)
        {
            return value & fastModuloValue;
        }

        private int FastIndexToBufferIndex(int value)
        {
            return value >> 11;
        }

        #endregion private methods
    }
}
