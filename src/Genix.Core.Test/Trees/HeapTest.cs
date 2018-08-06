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
        public void HeapTest1()
        {
            const int Count = 1000000;
            Random random = new Random(0);

            Heap<int> heap = new Heap<int>(100);

            int count = 0;
            while (count < Count)
            {
                // add more elements to the heap
                for (int i = 0, ii = random.Next(10, 100); i < ii; i++, count++)
                {
                    heap.Push(random.Next(0, 100));
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
                    int previous = heap.Pop();

                    for (int i = 0, ii = Math.Max(0, popcount - 1); i < ii; i++)
                    {
                        int value = heap.Pop();
                        Assert.IsTrue(previous >= value);
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

            Heap<int> heap = new Heap<int>(Size);

            stopwatch.Restart();

            for (int i = 0; i < Count; i++)
            {
                for (int j = 0, jj = values.Length; j < jj; j++)
                {
                    heap.Push(values[j]);
                }

                while (heap.Count > 0)
                {
                    heap.Pop();
                }
            }

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
        }
    }
}
