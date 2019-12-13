using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neural1
{
    class Perceptron
    {
        Random random;
        List<double> Weights;


        public Perceptron()
        {
            random = new Random();
            Weights = new List<double>() { (random.NextDouble() - 0.5) * 2, (random.NextDouble() - 0.5) * 2 };

        }

        public double Guess(List<double> inputs)
        {
            return inputs.Zip(Weights, (x, y) => x * y).Sum() >= 0 ? 1 : -1;
        }
    }
}
