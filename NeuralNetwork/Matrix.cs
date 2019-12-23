using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neural
{
    public class Matrix
    {
        public int RowCount;
        public int ColumnCount;
        public double[,] Data;

        public Matrix(int r, int c)
        {
            RowCount = r;
            ColumnCount = c;
            Data = new double[r, c];
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            var result = new Matrix(a.RowCount, b.ColumnCount);
            for (int i = 0; i < result.RowCount; i++)
                for (int k = 0; k < result.ColumnCount; k++)
                {
                    //Dot product of values in col
                    double sum = 0;
                    for (int q = 0; q < a.ColumnCount; q++)
                        sum += a.Data[i, q] * b.Data[q, k];

                    result.Data[i, k] = sum;
                }
            return result;
        }
        public static Matrix operator *(Matrix m, double d)
        {
            var result = new Matrix(m.RowCount, m.ColumnCount);
            for (int i = 0; i < m.RowCount; i++)
                for (int k = 0; k < m.ColumnCount; k++)
                    result.Data[i, k] = m.Data[i, k] * d;
            return result;
        }

        public static Matrix operator +(Matrix m, double d)
        {
            var result = new Matrix(m.RowCount, m.ColumnCount);
            for (int i = 0; i < m.RowCount; i++)
                for (int k = 0; k < m.ColumnCount; k++)
                    result.Data[i, k] = m.Data[i, k] + d;
            return result;
        }
        public static Matrix operator +(Matrix a, Matrix b)
        {
            var result = new Matrix(a.RowCount, a.ColumnCount);
            for (int i = 0; i < a.RowCount; i++)
                for (int k = 0; k < a.ColumnCount; k++)
                    result.Data[i, k] = a.Data[i, k] + b.Data[i, k];
            return result;
        }
        public static Matrix operator -(Matrix a, Matrix b)
        {
            return a + (b * -1);

            var result = new Matrix(a.RowCount, a.ColumnCount);
            for (int i = 0; i < a.RowCount; i++)
                for (int k = 0; k < a.ColumnCount; k++)
                    result.Data[i, k] = a.Data[i, k] - b.Data[i, k];
            return result;
        }

        public static Matrix DotProduct(Matrix a, Matrix b)
        {
            if (a.RowCount != b.RowCount)
                throw new Exception("WTF");
            if (a.ColumnCount != b.ColumnCount)
                throw new Exception("WTF");

            Matrix result = new Matrix(a.RowCount, a.ColumnCount);
            for (int i = 0; i < result.RowCount; i++)
                for (int k = 0; k < result.ColumnCount; k++)
                    result.Data[i, k] = a.Data[i, k] * b.Data[i, k];

            return result;
        }


        public static Matrix FromList(List<double> value)
        {
            var b = new Matrix(value.Count, 1);
            for (int i = 0; i < value.Count; i++)
                b.Data[i, 0] = value[i];

            return b;
        }
        public static Matrix FromList(List<float> value)
        {
            var b = new Matrix(value.Count, 1);
            for (int i = 0; i < value.Count; i++)
                b.Data[i, 0] = value[i];

            return b;
        }

        public double SUM()
        {
            double d = 0;
            for (int i = 0; i < RowCount; i++)
                for (int k = 0; k < ColumnCount; k++)
                    d += Data[i, k];
            return d;
        }
        public List<double> ToList()
        {
            List<double> d = new List<double>();
            for (int k = 0; k < ColumnCount; k++)
                for (int i = 0; i < RowCount; i++)
                    d.Add(Data[i, k]);
            return d;
        }

        public delegate double Func(double x);
        public void Map(Func func)
        {
            for (int i = 0; i < RowCount; i++)
                for (int k = 0; k < ColumnCount; k++)
                    Data[i, k] = func(Data[i, k]);
        }
        public static Matrix Map(Matrix m, Func func)
        {
            var result = new Matrix(m.RowCount, m.ColumnCount);
            for (int i = 0; i < m.RowCount; i++)
                for (int k = 0; k < m.ColumnCount; k++)
                    result.Data[i, k] = func(m.Data[i, k]);
            return result;
        }

        public static Matrix Transpose(Matrix m)
        {
            var result = new Matrix(m.ColumnCount, m.RowCount);
            for (int i = 0; i < m.RowCount; i++)
                for (int k = 0; k < m.ColumnCount; k++)
                    result.Data[k, i] = m.Data[i, k];
            return result;
        }
        public void Randomize()
        {
            Random random = new Random();
            for (int i = 0; i < RowCount; i++)
                for (int k = 0; k < ColumnCount; k++)
                    Data[i, k] = 2 * random.NextDouble() - 1;
        }

        public void Print()
        {
            Console.WriteLine("-------------------");
            for (int i = 0; i < RowCount; i++)
            {
                string s = "";
                for (int k = 0; k < ColumnCount; k++)
                    s += Data[i, k].ToString("0.000").PadLeft(8);
                Console.WriteLine(s);
            }
        }
    }
}
