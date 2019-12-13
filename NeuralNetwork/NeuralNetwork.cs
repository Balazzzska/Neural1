using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neural
{
    public class NeuralNetwork
    {
        Matrix weight_ih;
        Matrix weight_ho;
        Matrix bias_h;
        Matrix bias_o;

        double learningrate = 0.1;

        public NeuralNetwork(int input, int hidden, int output)
        {
            weight_ih = new Matrix(hidden, input);
            weight_ho = new Matrix(output, hidden);
            bias_h = new Matrix(hidden, 1);
            bias_o = new Matrix(output, 1);

            Reset();
        }

        private static double Sigmoid(double d) => 1 / (1 + Math.Exp(-d));
        private static double dSigmoid(double d) => d * (1 - d);

        public List<double> FeedForward(List<double> inputs)
        {
            var inp = Matrix.FromList(inputs);
            var hidden = weight_ih * inp;
            hidden += bias_h;
            hidden.Map(Sigmoid);

            var output = weight_ho * hidden;
            output += bias_o;
            output.Map(Sigmoid);

            return output.ToList();
        }

        public void Train(List<double> inp, List<double> target)
        {
            var inputs = Matrix.FromList(inp);
            var hidden = weight_ih * inputs;
            hidden += bias_h;
            hidden.Map(Sigmoid);

            var outputs = weight_ho * hidden;
            outputs += bias_o;
            outputs.Map(Sigmoid);

            //Cal output layer errors
            var targets = Matrix.FromList(target);
            var output_errors = targets - outputs;

            //Calculate gradient
            var gradients_output = Matrix.Map(outputs, dSigmoid);
            gradients_output *= output_errors;
            gradients_output *= learningrate;

            //Calulcate deltas
            var hidden_transposed = Matrix.Transpose(hidden);
            var weight_ho_delta = gradients_output * hidden_transposed;

            //Adjust
            weight_ho += weight_ho_delta;
            bias_o += gradients_output;

            //-------------------------------------------------------------------------

            //Calc hidden layer errors
            var weights_ho_transposed = Matrix.Transpose(weight_ho);
            var hidden_errors = weights_ho_transposed * output_errors;

            //Calculate gradient
            var gradient_hidden = Matrix.Map(hidden, dSigmoid);
            gradient_hidden = gradient_hidden * hidden_errors;
            gradient_hidden = gradient_hidden * learningrate;

            //Calulcate deltas
            var inputs_transposed = Matrix.Transpose(inputs);
            var weight_ih_delta = gradient_hidden * inputs_transposed;

            weight_ih += weight_ih_delta;
            bias_h += gradient_hidden;
        }

        public void Reset()
        {
            weight_ih.Randomize();
            weight_ho.Randomize();
            bias_h.Randomize();
            bias_o.Randomize();
        }
    }
}
