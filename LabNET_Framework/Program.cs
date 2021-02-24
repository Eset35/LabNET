using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace LabNET_Framework
{

    class Program
    {
        static void Main()
        {
            // AMP.list_all_accelerators();


              Test.SumVectors(1000000);


               // for (int i = 0; i < 10; i++)
                   // Test.TransMatrix(1000, 1000);

          /*  for(int i=0; i<10; i++)
                Test.ProductMatrix(1000, 1000, 1000, 50);*/
        }
    }

    public class Test
    {
        public static void ProductMatrix(int oneSizeN, int oneSizeM, int twoSizeN, int twoSizeM)
        {
            AMP.set_device();
            Console.WriteLine("ProductMatrix");
            Console.WriteLine("=========================");
            Console.WriteLine("Preparing data...");
            var one = Tools.RandomMatrix(oneSizeN, oneSizeM);
            var two = Tools.RandomMatrix(twoSizeN, twoSizeM);
            Console.WriteLine("=========================");
            Tools.ShowTime("GPU", () => GPU.MatrixesMultipy(one, two));
            Tools.ShowTime("CPU(seq)", () => CPU.MatrixesMultipy_Seq(one, two));
            Tools.ShowTime("CPU(par)", () => CPU.MatrixesMultipy_Par(one, two));
            Console.WriteLine();
        }
        public static void TransMatrix(int sizeN, int sizeM)
        {
            AMP.set_device();
            Console.WriteLine("TransMatrix");
            Console.WriteLine("=========================");
            Console.WriteLine("Preparing data...");
            var matrix = Tools.RandomMatrix(sizeN, sizeM);
            Tools.ShowTime("GPU", () => GPU.TransMatrix(matrix, sizeN, sizeM));
            Tools.ShowTime("CPU(seq)", () => CPU.TransMatrix_Seq(matrix, sizeN, sizeM));
            Tools.ShowTime("CPU(par)", () => CPU.TransMatrix_Par(matrix, sizeN, sizeM));
            Console.WriteLine();
        }

        public static void SumVectors(int size)
        {
            AMP.set_device();
            Console.WriteLine("Sum Vectors");
            Console.WriteLine("=========================");
            Console.WriteLine("Preparing data...");
            var one = Tools.RandomVector(size);
            var two = Tools.RandomVector(size);
            Console.WriteLine("=========================");

            Tools.ShowTime("GPU", () => GPU.SumVectors(one, two, size));
            Tools.ShowTime("CPU(seq)", () => CPU.SumVectors_Seq(one, two, size));
            Tools.ShowTime("GPU(par)", () => CPU.SumVectors_Par(one, two, size));
            Console.WriteLine();
        }


        public static void MultipyMatrixNumber(int sizeN, int sizeM)
        {
            AMP.set_device();

            Console.WriteLine("Multiply Matrix by number");
            Console.WriteLine("=========================");
            Console.WriteLine("Preparing data...");
            var matrix = Tools.RandomMatrix(sizeN, sizeM);
            Console.WriteLine("=========================");


            Tools.ShowTime("GPU", () => GPU.MultiplyMatrixByNumber(matrix, 5));
            Tools.ShowTime("CPU(seq)", () => CPU.MultiplyMatrixByNumber_Seq(matrix, 5));
            Tools.ShowTime("GPU(par)", () => CPU.MultiplyMatrixByNumber_Par(matrix, 5));
            Console.WriteLine();
        }
    }

    public class AMP
    {
        /// <summary>
        /// Суммирование двух векторов
        /// </summary>
        [DllImport("LabDLL", CallingConvention = CallingConvention.StdCall)]
        public extern unsafe static void sum_vectors(int* one, int* two, int* result, int size);

        /// <summary>
        /// Умножении матрицы на число
        /// </summary>
        [DllImport("LabDLL", CallingConvention = CallingConvention.StdCall)]
        public extern unsafe static void multiply_matrix_by_number(int* matrix, int number, int* result, int sizeN, int sizeM);

        /// <summary>
        /// Транспонирование матрицы
        /// </summary>

        [DllImport("LabDLL", CallingConvention = CallingConvention.StdCall)]
        public extern unsafe static void trans_matrix(int* matrix_one, int* result, int sizeN, int sizeM);


        /// <summary>
        /// Транспонирование матрицы
        /// </summary>

        [DllImport("LabDLL", CallingConvention = CallingConvention.StdCall)]
        public extern unsafe static void multiply_matrix(int* matrix_one, int* matrix_two, int* size_matrixes, int* result);


        /// <summary>
        /// Получение информации о ускорителях
        /// </summary>

        [DllImport("LabDLL", CallingConvention = CallingConvention.StdCall)]
        public extern unsafe static void list_all_accelerators();

        /// <summary>
        /// Установка самого мощного ускорителя по умолчанию
        /// </summary>

        [DllImport("LabDLL", CallingConvention = CallingConvention.StdCall)]
        public extern unsafe static void set_device();
    }

    public class Tools
    {
        public static void ShowTime(string type, Action action)
        {
            var sw = new Stopwatch();
            sw = new Stopwatch();
            sw.Start();
            action.Invoke();
            sw.Stop();
            Console.WriteLine(type+ ": " + sw.Elapsed.TotalMilliseconds);
        }
        public static int[] RandomVector(int size)
        {
            Random rand = new Random();
            var res = new int[size];
            for (int i = 0; i < size; i++)
                res[i] = rand.Next(-100000, 1000001);

            return res;
        }
        public static int[,] RandomMatrix(int sizeN, int sizeM)
        {
            Random rand = new Random();
            var res = new int[sizeN, sizeM];
            for (int i = 0; i < sizeN; i++)
                for (int j = 0; j < sizeM; j++)
                    res[i, j] = rand.Next(1, 4);

            return res;
        }

        public static void ShowMatrix(int[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                    Console.Write(matrix[i, j] + " ");
                Console.WriteLine();
            }
        }
    }

    public class CPU
    {
        static int[,] tempMatrix;
        static int[,] resultMatrix;
        static int[] resultVector;

        public static int[,] MultiplyMatrixByNumber_Seq(int[,] inputMartix, int k)
        {
            var resultMatrix = new int[inputMartix.GetUpperBound(0) + 1, inputMartix.GetUpperBound(1) + 1];

            for (var i = 0; i < inputMartix.GetUpperBound(0) + 1; i++)
            {
                for (var j = 0; j < inputMartix.GetUpperBound(1) + 1; j++)
                {
                    resultMatrix[i, j] = inputMartix[i, j] * k;
                }
            }

            return resultMatrix;
        }

        public static int[,] MultiplyMatrixByNumber_Par(int[,] inputMartix, int k)
        {
            resultMatrix = new int[inputMartix.GetUpperBound(0) + 1, inputMartix.GetUpperBound(1) + 1];

            int rows = inputMartix.GetLength(0);
            Thread t1 = new Thread(MultipyProcess);
            t1.Start(new object[] { inputMartix, k, 0, rows / 2 });
            Thread t2 = new Thread(MultipyProcess);
            t2.Start(new object[] { inputMartix, k, rows / 2, rows });
            t1.Join();
            t2.Join();

            return resultMatrix;
        }

        static void MultipyProcess(object a)
        {
            int[,] arr = (a as object[])[0] as int[,];
            int k = (int)((a as object[])[1] as int?);
            int start = (int)((a as object[])[2] as int?);
            int end = (int)((a as object[])[3] as int?);

            for (int i = start; i < end; i++)
                for (int j = 0; j < arr.GetUpperBound(1) + 1; j++)
                    resultMatrix[i, j] = arr[i, j] * k;
        }

        public static int[] SumVectors_Seq(int[] one, int[] two, int size)
        {
            var res = new int[size];

            for (int i = 0; i < size; i++)
                res[i] = one[i] + two[i];

            return res;
        }

        public static int[] SumVectors_Par(int[] one, int[] two, int size)
        {
            resultVector = new int[size];
            Thread t1 = new Thread(ProcessVectors);
            t1.Start(new object[] { one, two, 0, size / 2 });
            Thread t2 = new Thread(ProcessVectors);
            t2.Start(new object[] { one, two, size / 2, size });

            t1.Join();
            t2.Join();

            return resultVector;
        }

        static void ProcessVectors(object a)
        {
            int[] one = (a as object[])[0] as int[];
            int[] two = (a as object[])[1] as int[];
            int start = (int)((a as object[])[2] as int?);
            int end = (int)((a as object[])[3] as int?);

            for (int i = start; i < end; i++)
                resultVector[i] = one[i] + two[i];
        }

        public static int[,] TransMatrix_Seq(int[,] matrix, int sizeN, int sizeM)
        {
            int[,] resultMatrix = new int[sizeM, sizeN];

            for (int i = 0; i < sizeN; i++)
                for (int j = 0; j < sizeM; j++)
                    resultMatrix[j, i] = matrix[i, j];

            return resultMatrix;
        }

        public static int[,] TransMatrix_Par(int[,] matrix, int sizeN, int sizeM)
        {
            tempMatrix = matrix;
            resultMatrix = new int[sizeM, sizeN];

            Thread t1 = new Thread(TransProcess);
            t1.Start(new object[] { 0, sizeN / 2 });
            Thread t2 = new Thread(TransProcess);
            t2.Start(new object[] { sizeN / 2, sizeN });
            t1.Join();
            t2.Join();

            return resultMatrix;
        }

        static void TransProcess(object a)
        {
            int start = (int)((a as object[])[0] as int?);
            int end = (int)((a as object[])[1] as int?);
            int size = tempMatrix.GetUpperBound(1) + 1;

            for (int i = start; i < end; i++)
                for (int j = 0; j < size; j++)
                    resultMatrix[j, i] = tempMatrix[i, j];
        }

        public static int[,] MatrixesMultipy_Seq(int[,] matrixA, int[,] matrixB)
        {
            int aRows = matrixA.GetUpperBound(0) + 1; int aCols = matrixA.GetUpperBound(1) + 1;
            int bRows = matrixB.GetUpperBound(0) + 1; int bCols = matrixB.GetUpperBound(1) + 1;
            if (aCols != bRows)
                throw new Exception("Bad matrixes");

            int[,] result = new int[aRows, bCols];

            for (int i = 0; i < aRows; ++i)
                for (int j = 0; j < bCols; ++j) 
                    for (int k = 0; k < aCols; ++k)
                        result[i,j] += matrixA[i,k] * matrixB[k,j];
            return result;
        }

        public static int[,] MatrixesMultipy_Par(int[,] matrixA, int[,] matrixB)
        {
            int aRows = matrixA.GetUpperBound(0) + 1; int aCols = matrixA.GetUpperBound(1) + 1;
            int bRows = matrixB.GetUpperBound(0) + 1; int bCols = matrixB.GetUpperBound(1) + 1;
            if (aCols != bRows)
                throw new Exception("Bad matrixes");

           int[,] result = new int[aRows, bCols];

            Parallel.For(0, aRows, i =>
            {
                for (int j = 0; j < bCols; ++j) 
                    for (int k = 0; k < aCols; ++k) 
                        result[i,j] += matrixA[i,k] * matrixB[k,j];
            }
            );

            return result;
        }
    }

    public class GPU
    {
        public static unsafe int[] SumVectors(int[] one, int[] two, int size)
        {
            int[] res = new int[size];

            fixed (int* ptrOne = one)
            fixed (int* ptrTwo = two)
            fixed (int* ptrRes = res)
                AMP.sum_vectors(ptrOne, ptrTwo, ptrRes, size);

            return res;
        }

        public static unsafe int[,] MultiplyMatrixByNumber(int[,] matrix, int number)
        {
            int n = matrix.GetLength(0);
            int m = matrix.GetLength(1);
            int[,] res = new int[n, m];

            fixed (int* ptrMatrix = matrix)
            fixed (int* ptrRes = res)
                AMP.multiply_matrix_by_number(ptrMatrix, 5, ptrRes, n, m);


            return res;
        }

        public static unsafe int[,] TransMatrix(int[,] matrix, int sizeN, int sizeM)
        {
            int[,] res = new int[sizeN, sizeM];

            fixed (int* ptrMatrix = matrix)
            fixed (int* ptrRes = res)
                AMP.trans_matrix(ptrMatrix, ptrRes, sizeN, sizeM);

            return res;
        }

        public static unsafe int[,] MatrixesMultipy(int[,] matrix_one, int[,] matrix_two)
        {
            var size = new int[] { matrix_one.GetLength(0), matrix_one.GetLength(1), matrix_two.GetLength(0), matrix_two.GetLength(1) };
            int[,] res = new int[size[0], size[3]];

            fixed (int* ptrMatrixOne = matrix_one)
            fixed (int* ptrMatrixTwo = matrix_two)
            fixed (int* ptrSize = size)
            fixed (int* ptrRes = res)
                AMP.multiply_matrix(ptrMatrixOne, ptrMatrixTwo, ptrSize, ptrRes);

            return res;
        }
    }

}
