namespace Genix.DNN.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MathTest
    {
        /*[TestMethod]
        public void AddTest1()
        {
            int repeat = 10000;
            Stopwatch stopwatch = new Stopwatch();

            Volume v1 = new Volume(new Shape(100, 100, 3));
            Volume v2 = new Volume(new Shape(100, 100, 3));

            stopwatch.Restart();

            for (int i = 0; i < repeat; i++)
            {
                v1.Add(v2);
            }

            stopwatch.Stop();
            Console.WriteLine("{0:F4} ms", stopwatch.GetHiresElapsedMilliseconds() / repeat);
        }*/

        /*[TestMethod]
        public void DotTest1()
        {
            int repeat = 100;
            Stopwatch stopwatch = new Stopwatch();

            Volume v1 = new Volume(new Shape(100, 100, 3));
            Volume v2 = new Volume(new Shape(100, 100, 3, 100));
            Volume v3 = new Volume(new Shape(1, 1, 100));

            stopwatch.Restart();

            for (int i = 0; i < repeat; i++)
            {
                Volume.Dot(v1, v2, v3);
            }

            stopwatch.Stop();
            Console.WriteLine("{0:F4} ms", stopwatch.GetHiresElapsedMilliseconds() / repeat);
        }

        [TestMethod]
        public void DotGradientTest1()
        {
            int repeat = 100;
            Stopwatch stopwatch = new Stopwatch();

            Volume v1 = new Volume(new Shape(100, 100, 3));
            Volume v2 = new Volume(new Shape(100, 100, 3, 100));
            Volume v3 = new Volume(new Shape(100, 100, 3, 100));
            Volume v4 = new Volume(new Shape(100, 100, 3));
            Volume v5 = new Volume(new Shape(1, 1, 100));

            stopwatch.Restart();

            for (int i = 0; i < repeat; i++)
            {
                Volume.DotGradient(v1, v2, v3, v4, v5);
            }

            stopwatch.Stop();
            Console.WriteLine("{0:F4} ms", stopwatch.GetHiresElapsedMilliseconds() / repeat);
        }*/
    }
}
