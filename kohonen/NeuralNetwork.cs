using System;
using System.Collections.Generic;
using System.Linq;

namespace kohonen
{
    public class NeuralNetwork
    {
        private double learnStep;
        private List<Neuron> neurons;
        private double[,] neighborhood;

        private int madeSteps;
        private int totalSteps;
        private int dimension;
        private bool wta;

        public NeuralNetwork(int dimension, int inputs, int steps, int gridWidth, bool winnerTakesAll)
        {
            wta = winnerTakesAll;
            this.dimension = dimension;
            totalSteps = steps;
            madeSteps = 0;
            learnStep = 1.0 / steps;
            neighborhood = new double[dimension * dimension, dimension * dimension];
            neurons = new List<Neuron>();
            double coeff = 1.0 / dimension;

            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    //Neurons with random weights
                    neurons.Add(new Neuron(inputs));

                    //Neurons with fixed weights
                    /*var codebook = new double[inputs];
                    codebook[0] = i * coeff + coeff / 2;
                    codebook[1] = j * coeff + coeff / 2;
                    neurons.Add(new Neuron(codebook));*/

                }
            }
            UpdateNeigborhood();
        }

        private void UpdateNeigborhood()
        {
            //populate neightbors distance map (0<=d<=1), 1 - diagonal
            for (int i = 0; i < dimension * dimension; i++)
            {
                for (int j = 0; j < dimension * dimension; j++)
                {
                    neighborhood[i, j] = 1.0 / (neurons[i].Distance(neurons[j]) + 1.0);
                }
            }
        }

        public int Classify(double[] x)
        {
            double[] distances = new double[neurons.Count];
            double min = 0;
            int minIdx = 0;

            for (int i = 0; i < neurons.Count; i++)
            {
                distances[i] = neurons[i].Distance(x);

                if (i == 0)
                {
                    min = distances[i];
                }
                else if (distances[i] < min)
                {
                    min = distances[i];
                    minIdx = i;
                }
            }

            return minIdx;
        }

        public void StepUp()
        {
            if (madeSteps < totalSteps)
            {
                madeSteps++;
            }
        }

        private int IdxToRow(int idx)
        {
            return idx / dimension;
        }

        private int IdxToCol(int idx)
        {
            return idx % dimension;
        }

        public int Learn(double[] x)
        {
            int minIdx = Classify(x);
            //learnCoeff is gradually decreasing after stepUp
            double learnCoeff = (totalSteps - madeSteps) * learnStep;

            if (wta)
            {
                //WTA
                neurons[minIdx].Update(x, 1, learnCoeff);
            }
            else
            {
                //WTM
                for (int i = 0; i < dimension * dimension; i++)
                {
                    neurons[i].Update(x, neighborhood[minIdx, i], learnCoeff);
                }

                //not good results
                //UpdateNeigborhood();
            }

            return minIdx;
        }
    }
}