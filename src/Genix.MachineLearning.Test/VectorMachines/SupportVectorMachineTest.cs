namespace Genix.MachineLearning.VectorMachines.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    ////using Accord.MachineLearning.VectorMachines.Learning;
    using Genix.Core;
    using Genix.Lab;
    using Genix.MachineLearning.Kernels;
    using Genix.MachineLearning.VectorMachines.Learning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class SupportVectorMachineTest
    {
        [TestMethod]
        public void SerializeTest()
        {
            SupportVectorMachine machine1 = new SupportVectorMachine(
                new ChiSquare(),
                new float[][]
                {
                    new float[] {1, 2},
                    new float[] {3, 4},
                    new float[] {5, 6},
                },
                new float[] { 0.1f, 0.2f, 0.3f },
                0.4f);
            string s1 = JsonConvert.SerializeObject(machine1);
            SupportVectorMachine machine2 = JsonConvert.DeserializeObject<SupportVectorMachine>(s1);
            string s2 = JsonConvert.SerializeObject(machine2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void TrainTest1()
        {
            const int LearnCount = 100;
            const int TestCount = 1000;
            const int Length = 300;
            const double PositiveRatio = 0.1;

            // create samples
            List<(float[] x, bool y, float weight)> samples = SupportVectorMachineTest.GenerateSamples(LearnCount + TestCount, Length, PositiveRatio)
                .Select(x => (x.x.Select(w => (float)w).ToArray(), x.y, (float)x.weight))
                .ToList();

            // learn
            SequentualMinimalOptimization smo = new SequentualMinimalOptimization(new ChiSquare())
            {
                Algorithm = SMOAlgorithm.LibSVM,
                Tolerance = 0.01f,
            };

            SupportVectorMachine machine = SupportVectorMachine.Learn(
                smo,
                samples.Take(LearnCount).Select(x => x.x).ToList(),
                samples.Take(LearnCount).Select(x => x.y).ToList(),
                samples.Take(LearnCount).Select(x => x.weight).ToList(),
                CancellationToken.None);

            // test
            List<ClassificationResult<bool?>> results = samples
                .Skip(LearnCount)
                .Select(x => new ClassificationResult<bool?>(null, machine.Classify(x.x) > 0.5f, x.y, 1.0f, true))
                .ToList();

            ClassificationReport<bool?> report = new ClassificationReport<bool?>(results);
        }

        /*[TestMethod]
        public void TrainTest2()
        {
            const int LearnCount = 100;
            const int TestCount = 1000;
            const int Length = 300;
            const double PositiveRatio = 0.1;

            // create samples
            List<(double[] x, bool y, double weight)> samples = SupportVectorMachineTest.GenerateSamples(LearnCount + TestCount, Length, PositiveRatio).ToList();

            // learn
            SequentialMinimalOptimization<Accord.Statistics.Kernels.IKernel> optimization = new SequentialMinimalOptimization<Accord.Statistics.Kernels.IKernel>()
            {
                Kernel = new Accord.Statistics.Kernels.ChiSquare(),
                Tolerance = 0.01f,
                Strategy = SelectionStrategy.SecondOrder
            };

            optimization.Learn(
                samples.Take(LearnCount).Select(x => x.x.Select(w => (double)w).ToArray()).ToArray(),
                samples.Take(LearnCount).Select(x => x.y).ToArray());
        }*/

        private static IEnumerable<(double[] x, bool y, double weight)> GenerateSamples(int count, int length, double positiveRatio)
        {
            Random random = new Random(0);
            BinarySampleGenerator sampleGenerator = new BinarySampleGenerator(positiveRatio, random);

            double[] weights = new RandomRangeGenerator(-1, 1, random).Generate(length).Select(x => (double)x).ToArray();

            while (count > 0)
            {
                double[] sample = sampleGenerator.Generate(length).Select(x => x ? 1.0 : 0.0).ToArray();

                double res = Matrix.DotProduct(length, sample, 0, weights, 0);
                if (res >= 1)
                {
                    yield return (sample, true, 1.0f);
                    count--;
                }
                else if (res <= -1)
                {
                    yield return (sample, false, 1.0f);
                    count--;
                }
            }
        }
    }
}
