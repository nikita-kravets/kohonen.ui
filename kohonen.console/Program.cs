using System;
using System.Collections.Generic;

namespace kohonen.console
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<int, int> cls = new Dictionary<int, int>();
            //training set
            double[] x = { 0.9, 1.1, 0.95, 0.91, 0.97, 1.05, 5.9, 6.1, 5.95, 5.91, 5.97, 6.05, 150, 30 };
            double[] y = { 0.9, 1.1, 0.95, 0.91, 0.97, 1.05, 5.9, 6.1, 5.95, 5.91, 5.97, 6.05, 450, 30 };
            //check set
            double[] x1 = { 0.912, 1.099, 0.945, 0.913, 0.964, 1.02, 5.933, 6.19, 5.9591, 5.904, 4.9, 6.35, 160, 170, 32 };
            double[] y1 = { 0.906, 1.1, 0.955, 0.911, 0.97, 1.03, 5.87, 6.12, 5.952, 5.914, 4.9, 6.35, 500, 540, 32 };
            
            var neuralNetwork = new NeuralNetwork(9, 2, x.Length, 10, true);

            for (int k = 0; k < x.Length; k++)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    neuralNetwork.Learn(new double[] { x[i], y[i] });
                }
                neuralNetwork.StepUp();
            }

            for (int i = 0; i < x1.Length; i++)
            {
                int c = neuralNetwork.Classify(new double[] { x1[i], y1[i] });
                if (cls.ContainsKey(c))
                {
                    cls[c]++;
                }
                else
                {
                    cls[c] = 1;
                }
            }
        }
    }
}
