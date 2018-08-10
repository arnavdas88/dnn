// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Threading;

    /// <summary>
    /// Defines a base class for testing command-line applications.
    /// </summary>
    public abstract class Program
    {
        // Locker for log operations.
        private object locker = new object();
        private StreamWriter programLogFile = null;

        private PerformanceCounter performanceCounterNetTotalHeap = null;
        private PerformanceCounter performanceCounterNetGCHandles = null;
        private ConcurrentDictionary<int, Stopwatch> threadStopwatches = new ConcurrentDictionary<int, Stopwatch>();

        /// <summary>
        /// The main program function. Should be called from parent class <c>Main</c> function.
        /// </summary>
        /// <param name="args">The program arguments.</param>
        /// <returns>The return code.</returns>
        [HandleProcessCorruptedStateExceptions]
        public int Run(string[] args)
        {
            int retCode = 0;
            try
            {
                // show application information
                Assembly assembly = Assembly.GetEntryAssembly();

                AssemblyTitleAttribute title = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute));
                Console.WriteLine(title.Title + " Version " + assembly.GetName().Version.ToString());

                AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute));
                Console.WriteLine(copyright.Copyright);
                Console.WriteLine();

                // configure application
                if (this.OnConfigure(args))
                {
                    // create performance counters
                    this.performanceCounterNetTotalHeap = Program.CreatePerformanceCounter(".NET CLR Memory", "# bytes in all heaps");
                    this.performanceCounterNetGCHandles = Program.CreatePerformanceCounter(".NET CLR Memory", "# GC Handles");

                    // run application
                    DateTime dateStarted = DateTime.Now;
                    this.WriteLine(null, string.Format(CultureInfo.InvariantCulture, "Started: {0}", dateStarted.ToString("G", CultureInfo.InvariantCulture)));

                    // run application
                    retCode = this.OnRun();

                    // report finish time and processing interval
                    this.WriteLine(null, string.Empty);
                    DateTime dateFinished = DateTime.Now;
                    this.WriteLine(null, string.Format(CultureInfo.InvariantCulture, "Finished: {0:G}", dateFinished));
                    this.WriteLine(null, string.Format(CultureInfo.InvariantCulture, "Total time: {0:g}", TimeSpan.FromSeconds((dateFinished - dateStarted).TotalSeconds)));

                    // wrap everything up
                    this.OnFinish();
                }
            }
            catch (Exception e)
            {
                // write error report
                this.WriteException(null, e);
                retCode = -1;
            }
            finally
            {
                if (this.performanceCounterNetGCHandles != null)
                {
                    this.performanceCounterNetGCHandles.Dispose();
                    this.performanceCounterNetGCHandles = null;
                }

                if (this.performanceCounterNetTotalHeap != null)
                {
                    this.performanceCounterNetTotalHeap.Dispose();
                    this.performanceCounterNetTotalHeap = null;
                }

                if (this.programLogFile != null)
                {
                    this.programLogFile.Flush();
                    this.programLogFile.Dispose();
                    this.programLogFile = null;
                }
            }

            return retCode;
        }

        /// <summary>
        /// The program configuration callback to be overritten by a parent class.
        /// </summary>
        /// <param name="args">The program arguments.</param>
        /// <returns>
        /// <b>true</b> if the application was configured and is ready to run; otherwise, <b>false</b>.
        /// </returns>
        protected abstract bool OnConfigure(string[] args);

        /// <summary>
        /// The main program callback to be overritten by a parent class.
        /// </summary>
        /// <returns>The return code.</returns>
        protected abstract int OnRun();

        /// <summary>
        /// The final program callback that runs after the program has finished to be overritten by a parent class.
        /// </summary>
        protected abstract void OnFinish();

        /// <summary>
        /// Opens the log file.
        /// </summary>
        /// <param name="fileName">The log file name.</param>
        // open log file
        protected void OpenLogFile(string fileName)
        {
            this.programLogFile = new StreamWriter(fileName, true)
            {
                AutoFlush = true,
            };
        }

        /// <summary>
        /// Writes a message to event log.
        /// </summary>
        /// <param name="logFile">The log file to write message to.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="arg">The arguments to write.</param>
        protected void Write(StreamWriter logFile, string message, params object[] arg)
        {
            lock (this.locker)
            {
                Console.Write(message, arg);

                if (logFile != null)
                {
                    logFile.Write(message, arg);
                }

                if (this.programLogFile != null)
                {
                    this.programLogFile.Write(message, arg);
                }
            }
        }

        /// <summary>
        /// Writes a message followed by a new line to event log.
        /// </summary>
        /// <param name="logFile">The log file to write message to.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="arg">The arguments to write.</param>
        protected void WriteLine(StreamWriter logFile, string message, params object[] arg)
        {
            lock (this.locker)
            {
                Console.WriteLine(message, arg);

                if (logFile != null)
                {
                    logFile.WriteLine(message, arg);
                }

                if (this.programLogFile != null)
                {
                    this.programLogFile.WriteLine(message, arg);
                }
            }
        }

        /// <summary>
        /// Writes an exception to event log.
        /// </summary>
        /// <param name="logFile">The log file to write message to.</param>
        /// <param name="exception">The exception to write.</param>
        protected void WriteException(StreamWriter logFile, Exception exception)
        {
            this.WriteLine(
                logFile,
                string.Join(Environment.NewLine, Program.AggregateMessages(exception).Concat(Program.AggregateStackTrace(exception))));
        }

        /// <summary>
        /// Writes program debug information to event log.
        /// </summary>
        /// <param name="logFile">The log file to write message to.</param>
        protected void WriteDebugInformation(StreamWriter logFile)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            long workingSet = Process.GetCurrentProcess().WorkingSet64 / 1000;
            long virtualSize = Process.GetCurrentProcess().VirtualMemorySize64 / 1000;
            long peakWorkingSet = Process.GetCurrentProcess().PeakWorkingSet64 / 1000;
            long netTotalHeap = (long)this.performanceCounterNetTotalHeap.NextValue() / 1000;
            long netGCHandles = (long)this.performanceCounterNetGCHandles.NextValue();
            long unmanaged = workingSet - netTotalHeap;

            string s = string.Format(
                CultureInfo.InvariantCulture,
                " {0:N0} KB, ({1:N0} KB managed, {2} GC handles, {3:N0} KB unmanaged, {4:N0} KB Peak Working Set, {5:N0} KB Virtual Memory)",
                workingSet,
                netTotalHeap,
                netGCHandles,
                unmanaged > 0 ? unmanaged : 0,
                peakWorkingSet,
                virtualSize);

            this.Write(logFile, s);
        }

        /// <summary>
        /// Restarts the current thread stopwatch.
        /// </summary>
        protected void StopwatchRestart()
        {
            Stopwatch stopwatch = this.threadStopwatches.GetOrAdd(
                Thread.CurrentThread.ManagedThreadId,
                new Stopwatch());

            stopwatch.Restart();
        }

        /// <summary>
        /// Stops the current thread stopwatch and reports the elapsed time.
        /// </summary>
        /// <returns>
        /// The total elapsed time measured since last restart, in milliseconds.
        /// </returns>
        protected long StopwatchStop()
        {
            long duration = 0;

            if (this.threadStopwatches.TryGetValue(Thread.CurrentThread.ManagedThreadId, out Stopwatch stopwatch))
            {
                stopwatch.Stop();
                duration = stopwatch.ElapsedMilliseconds;
            }

            return duration;
        }

        /// <summary>
        /// Aggregates messages from the exception and all its inner exceptions.
        /// </summary>
        /// <param name="exception">The exception to aggregate the messages from.</param>
        /// <returns>The collection of strings that contains error messages.</returns>
        private static string[] AggregateMessages(Exception exception)
        {
            List<string> messages = new List<string>();

            // aggregate messages
            for (; exception != null; exception = exception.InnerException)
            {
                if (!string.IsNullOrEmpty(exception.Message))
                {
                    messages.Add(exception.Message);

                    // add aggregate exception messages
                    if (exception is AggregateException aggregateException)
                    {
                        foreach (Exception e in aggregateException.InnerExceptions)
                        {
                            messages.AddRange(Program.AggregateMessages(e));
                        }

                        // all inner exception have been processed
                        break;
                    }
                }
            }

            // filter out duplicate messages
            for (int i = 1; i < messages.Count; i++)
            {
                if (messages[i - 1] == messages[i])
                {
                    messages.RemoveAt(i--);
                }
            }

            // filter out meaningless messages
            if (messages.Count > 1)
            {
                messages.RemoveAll(x => x == "Exception has been thrown by the target of an invocation.");
            }

            return messages.ToArray();
        }

        /// <summary>
        /// Aggregates stack trace from the exception and all its inner exceptions.
        /// </summary>
        /// <param name="exception">The exception to aggregate the stack trace from.</param>
        /// <returns>The collection of strings that contains stack trace.</returns>
        private static string[] AggregateStackTrace(Exception exception)
        {
            List<string> messages = new List<string>();

            // aggregate messages
            for (; exception != null; exception = exception.InnerException)
            {
                // add aggregate exception messages
                if (exception is AggregateException aggregateException)
                {
                    foreach (Exception e in aggregateException.InnerExceptions)
                    {
                        messages.AddRange(Program.AggregateStackTrace(e));
                    }

                    // all inner exception have been processed
                    break;
                }
                else if (exception.InnerException == null)
                {
                    if (!string.IsNullOrEmpty(exception.StackTrace))
                    {
                        messages.Add(exception.StackTrace);
                    }
                }
            }

            return messages.ToArray();
        }

        /// <summary>
        /// Create a new instance of the <see cref="PerformanceCounter"/> class.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <returns>The <see cref="PerformanceCounter"/> this method creates.</returns>
        private static PerformanceCounter CreatePerformanceCounter(string categoryName, string counterName)
        {
            PerformanceCounter performanceCounter = null;
            for (int i = 0; i < 10 && performanceCounter == null; i++)
            {
                try
                {
                    performanceCounter = new PerformanceCounter(categoryName, counterName, Process.GetCurrentProcess().ProcessName, true);
                }
                catch
                {
                    Thread.Sleep(5000);
                }
            }

            if (performanceCounter == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Failed to create performance counter <{0} : {1}>.", categoryName, counterName));
            }

            return performanceCounter;
        }
    }
}
