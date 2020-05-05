using System;

namespace kohonen
{
    public class Neuron
    {
        private double[] w;

        public Neuron(int count)
        {
            w = new double[count];
            Random r = new Random();
            for (int j = 0; j < count; j++)
            {
                w[j] = r.NextDouble();
            }
        }

        public Neuron(double[] w)
        {
            this.w = w;
        }

        public double Distance(double[] x)
        {
            if (x.Length != w.Length)
            {
                throw new Exception("Dimesion mismatch");
            }

            double sum = 0;

            for (int j = 0; j < w.Length; j++)
            {
                sum += Math.Pow(x[j] - w[j], 2);
            }
            
            return Math.Sqrt(sum);
        }

        public void Update(double[] x, double neigbourToWinner, double learnCoeff)
        {
            for (int j = 0; j < w.Length; j++)
            {
                w[j] = w[j] * (1 - neigbourToWinner * learnCoeff)
                    + x[j] * neigbourToWinner * learnCoeff;
            }
        }

        internal double Distance(Neuron neuron)
        {
            return Distance(neuron.w);
        }
    }
}