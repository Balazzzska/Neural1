using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApp1
{
    class Perceptron
    {
        Random random;
        public List<double> Weights;
        double learningrate = 0.001;

        public Perceptron(int n)
        {
            random = new Random();
            Weights = new List<double>();

            for (int i = 0; i < n; i++)
                Weights.Add((random.NextDouble() - 0.5) * 2);
        }

        public double Guess(List<double> inputs)
        {
            return inputs.Zip(Weights, (x, y) => x * y).Sum() >= 0 ? 1 : -1;
        }

        public double GuessY(double x)
        {
            var w = Weights;
            return -(w[2] / w[1]) - (w[0] / w[1]) * x;
        }

        public void Train(List<double> inputs, double target)
        {
            double guess = Guess(inputs);
            double error = target - guess;

            for (int i = 0; i < Weights.Count; i++)
                Weights[i] += error * inputs[i] * learningrate;
        }
    }
}
