namespace Genix.Core.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HeapTest
    {
        [TestMethod]
        public void BinaryHeapTest1()
        {
            const int Count = 1000000;
            Random random = new Random(0);

            BinaryHeap<int, int> heap = new BinaryHeap<int, int>(100);

            int count = 0;
            while (count < Count)
            {
                // add more elements to the heap
                for (int i = 0, ii = random.Next(10, 100); i < ii; i++, count++)
                {
                    heap.Push(random.Next(0, 100), 1000);
                }

                // pop some elements
                Pop(random.Next(heap.Count / 2, 3 * heap.Count / 4));
            }

            // pop remaining element
            Pop(heap.Count);

            void Pop(int popcount)
            {
                if (popcount > 0)
                {
                    int previous = heap.Pop().key;

                    for (int i = 0, ii = Math.Max(0, popcount - 1); i < ii; i++)
                    {
                        int value = heap.Pop().key;
                        Assert.IsTrue(previous <= value);
                        previous = value;
                    }
                }
            }
        }

        [TestMethod]
        public void FibonacciHeapTest1()
        {
            const int Count = 1000000;
            Random random = new Random(0);

            FibonacciHeap<int, int> heap = new FibonacciHeap<int, int>(100);

            int count = 0;
            while (count < Count)
            {
                // add more elements to the heap
                for (int i = 0, ii = random.Next(10, 100); i < ii; i++, count++)
                {
                    heap.Push(random.Next(0, 100), 1000);
                }

                // pop some elements
                Pop(random.Next(heap.Count / 2, 3 * heap.Count / 4));
            }

            // pop remaining element
            Pop(heap.Count);

            void Pop(int popcount)
            {
                if (popcount > 0)
                {
                    int previous = heap.Pop().key;

                    for (int i = 0, ii = Math.Max(0, popcount - 1); i < ii; i++)
                    {
                        int value = heap.Pop().key;
                        Assert.IsTrue(previous <= value);
                        previous = value;
                    }
                }
            }
        }

        [TestMethod]
        public void HeapTest2()
        {
            const int Count = 100000;
            const int Size = 100;
            Random random = new Random(0);
            Stopwatch stopwatch = new Stopwatch();
            int[] values = Enumerable.Range(0, Size).Select(x => random.Next(0, 1000000)).ToArray();

            BinaryHeap<int, int> binaryHeap = new BinaryHeap<int, int>(Size);

            stopwatch.Restart();

            for (int i = 0; i < Count; i++)
            {
                for (int j = 0, jj = random.Next(10, 100); j < jj; j++)
                {
                    binaryHeap.Push(random.Next(0, 100), 1000);
                }

                Pop(binaryHeap, random.Next(binaryHeap.Count / 2, 3 * binaryHeap.Count / 4));
            }

            Pop(binaryHeap, binaryHeap.Count);

            stopwatch.Stop();

            Console.WriteLine("{0:F4} ms", stopwatch.ElapsedMilliseconds/* / Count*/);

            FibonacciHeap<int, int> fibonacciHeap = new FibonacciHeap<int, int>(Size);

            stopwatch.Restart();

            for (int i = 0; i < Count; i++)
            {
                for (int j = 0, jj = random.Next(10, 100); j < jj; j++)
                {
                    fibonacciHeap.Push(random.Next(0, 100), 1000);
                }

                Pop(fibonacciHeap, random.Next(fibonacciHeap.Count / 2, 3 * fibonacciHeap.Count / 4));
            }

            Pop(fibonacciHeap, binaryHeap.Count);

            stopwatch.Stop();

            Console.WriteLine("{0:F4} ms", stopwatch.ElapsedMilliseconds/* / Count*/);

            SortedList<int, int> list = new SortedList<int, int>(Size, Comparer<int>.Default);

            stopwatch.Restart();

            for (int i = 0; i < Count; i++)
            {
                for (int j = 0, jj = values.Length; j < jj; j++)
                {
                    list.Add(values[j], values[j]);
                }

                while (list.Count > 0)
                {
                    list.RemoveAt(0);
                }
            }

            stopwatch.Stop();

            Console.WriteLine("{0:F4} ms", stopwatch.ElapsedMilliseconds/* / Count*/);

            void Pop(IHeap<int, int> heap, int popcount)
            {
                if (popcount > 0)
                {
                    int previous = heap.Pop().key;

                    for (int i = 0, ii = Math.Max(0, popcount - 1); i < ii; i++)
                    {
                        int value = heap.Pop().key;
                        Assert.IsTrue(previous <= value);
                        previous = value;
                    }
                }
            }
        }
    }
}
