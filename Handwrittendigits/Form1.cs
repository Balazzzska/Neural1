using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MNIST.IO;
using Neural;

namespace Handwrittendigits
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static List<double> ImageToList(TestCase t)
        {
            List<double> res = new List<double>();
            for (int i = 0; i < 28; i++)
                for (int k = 0; k < 28; k++)
                    res.Add(t.Image[i, k]);

            double max = 255;
            return res.Select(p => p / max).ToList();
        }

        NeuralNetwork nn = new NeuralNetwork(28 * 28, 64, 10);
        IEnumerable<TestCase> traindata, testdata;
        private void Form1_Load(object sender, EventArgs e)
        {
            var data = FileReaderMNIST.LoadImagesAndLables(
                "train-labels-idx1-ubyte.gz",
                "train-images-idx3-ubyte.gz");

            traindata = data;

            var data2 = FileReaderMNIST.LoadImagesAndLables(
                "t10k-labels-idx1-ubyte.gz",
                "t10k-images-idx3-ubyte.gz");

            testdata = data2;
        }

        void TrainEpoch()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (var d in traindata)
            {
                var input = ImageToList(d);

                var output = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                output[d.Label] = 1;

                nn.Train(input, output);
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        void TestData()
        {
            int correct = 0;
            int cnt = 0;
            foreach (var d in testdata)
            {
                double max = 0;
                int k = -1;
                var guess = nn.FeedForward(ImageToList(d)).ToArray();
                for (int i = 0; i < guess.Length; i++)
                {
                    if (guess[i] > max)
                    {
                        max = guess[i];
                        k = i;
                    }
                }

                if (k == d.Label)
                {
                    correct++;
                }

                cnt++;
            }

            Console.WriteLine(correct.ToString() + "/" + cnt.ToString());
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space)
            {
                TestData();
            }

            if (keyData == Keys.R)
            {
                TrainEpoch();
            }

            if (keyData == Keys.L)
            {
                while (true)
                {
                    TrainEpoch();
                    TestData();
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
