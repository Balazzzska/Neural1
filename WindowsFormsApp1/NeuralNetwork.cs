using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
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

            weight_ih.Randomize();
            weight_ho.Randomize();
            bias_h.Randomize();
            bias_o.Randomize();
        }

        private static double Sigmoid(double d) => 1 / (1 + Math.Exp(-d));
        private static double dSigmoid(double d) => d * (1 - d);

        public List<double> FeedForward(List<double> inputs)
        {
            var inp = Matrix.FromList(inputs);
            var hidden = Matrix.Multiply(weight_ih, inp);
            hidden = Matrix.Add(hidden, bias_h);
            hidden.Map(Sigmoid);

            var output = Matrix.Multiply(weight_ho, hidden);
            output = Matrix.Add(output, bias_o);
            output.Map(Sigmoid);

            return output.ToList();
        }

        public void Train(List<double> inp, List<double> target)
        {
            var inputs = Matrix.FromList(inp);
            var hidden = Matrix.Multiply(weight_ih, inputs);
            hidden = Matrix.Add(hidden, bias_h);
            hidden.Map(Sigmoid);

            var outputs = Matrix.Multiply(weight_ho, hidden);
            outputs = Matrix.Add(outputs, bias_o);
            outputs.Map(Sigmoid);

            //Cal output layer errors
            var targets = Matrix.FromList(target);
            var output_errors = Matrix.Subtract(targets, outputs);

            //Calculate gradient
            var gradients_output = Matrix.Map(outputs, dSigmoid);
            gradients_output = Matrix.Multiply(gradients_output, output_errors);
            gradients_output = Matrix.Multiply(gradients_output, learningrate);

            //Calulcate deltas
            var hidden_transposed = Matrix.Transpose(hidden);
            var weight_ho_delta = Matrix.Multiply(gradients_output, hidden_transposed);

            //Adjust
            weight_ho = Matrix.Add(weight_ho, weight_ho_delta);
            bias_o = Matrix.Add(bias_o, gradients_output);

            //-------------------------------------------------------------------------

            //Calc hidden layer errors
            var weights_ho_transposed = Matrix.Transpose(weight_ho);
            var hidden_errors = Matrix.Multiply(weights_ho_transposed, output_errors);

            //Calculate gradient
            var gradient_hidden = Matrix.Map(hidden, dSigmoid);
            gradient_hidden = Matrix.Multiply(gradient_hidden, hidden_errors);
            gradient_hidden = Matrix.Multiply(gradient_hidden, learningrate);

            //Calulcate deltas
            var inputs_transposed = Matrix.Transpose(inputs);
            var weight_ih_delta = Matrix.Multiply(gradient_hidden, inputs_transposed);

            weight_ih = Matrix.Add(weight_ih, weight_ih_delta);
            bias_h = Matrix.Add(bias_h, gradient_hidden);

            /*outputs.Print();
            targets.Print();
            output_errors.Print();
            weight_ho.Print();
            weights_ho_transposed.Print();*/
        }
    }
}
