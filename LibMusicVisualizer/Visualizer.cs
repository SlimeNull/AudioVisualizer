using FftSharp;

namespace LibMusicVisualizer
{
    public class Visualizer
    {
        private int _m;
        private double[] _sampleData;

        public double[] SampleData => _sampleData;

        public Visualizer(int waveDataSize)
        {
            if (!(Get2Flag(waveDataSize)))
                throw new ArgumentException("长度必须是 2 的 n 次幂");
            _m = (int)Math.Log2(waveDataSize);
            _sampleData = new double[waveDataSize];
        }

        private bool Get2Flag(int num)
        {
            if (num < 1)
                return false;
            return (num & num - 1) == 0;
        }

        public void PushSampleData(double[] waveData)
        {
            if (waveData.Length > _sampleData.Length)
            {
                Array.Copy(waveData, waveData.Length - _sampleData.Length, _sampleData, 0, _sampleData.Length);
            }
            else
            {
                Array.Copy(_sampleData, waveData.Length, _sampleData, 0, _sampleData.Length - waveData.Length);
                Array.Copy(waveData, 0, _sampleData, _sampleData.Length - waveData.Length, waveData.Length);
            }
        }

        public double[] GetSpectrumData()
        {
            int len = _sampleData.Length;
            Complex[] data = new Complex[len];

            for (int i = 0; i < len; i++)
                data[i] = new Complex(_sampleData[i], 0);

            Transform.FFT(data);

            int halfLen = len / 2;
            double[] result = new double[halfLen];
            for (int i = 0; i < halfLen; i++)
                result[i] = data[i].Magnitude / len;

            var window = new FftSharp.Windows.Bartlett();
            window.Create(halfLen);
            window.ApplyInPlace(result, false);

            return result;
        }
    }
}