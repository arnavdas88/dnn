// -----------------------------------------------------------------------
// <copyright file="LibSVMOptimization.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
// The source code presented in this file has been adapted from LibSVM -
// A Library for Support Vector Machines, created by Chih-Chung Chang and
// Chih-Jen Lin. Original license is given below.
//
// Copyright(c) 2000-2018 Chih-Chung Chang and Chih-Jen Lin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// 1. Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
//
// 3. Neither name of copyright holders nor the names of its contributors
// may be used to endorse or promote products derived from this software
// without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE REGENTS OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace Genix.MachineLearning.VectorMachines.Learning
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Genix.Core;

    /// <summary>
    /// An SMO algorithm in Fan et al., JMLR 6(2005), p. 1889--1918.
    /// </summary>
#if false
    /// <remarks>
    /// Solves:
    ///
    /// min 0.5(\\alpha^T Q \\alpha) + p^T \\alpha
    ///
    ///     y^T \\alpha = \\delta
    ///     y_i = +1 or -1
    ///     0 <= alpha_i <= Cp for y_i = 1
    ///     0 <= alpha_i <= Cn for y_i = -1
    ///
    /// Given:
    ///
    /// Q, p, y, Cp, Cn, and an initial feasible point \alpha
    /// l is the size of vectors and matrices
    /// eps is the stopping tolerance
    ///
    /// solution will be put in \\alpha, objective value will be put in obj
    ///</remarks>
#endif
    internal class LibSVMOptimization
    {
        private const float TAU = 1e-12f;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibSVMOptimization"/> class.
        /// </summary>
        public LibSVMOptimization()
        {
        }

        private enum Status
        {
            LOWER_BOUND = 0,
            UPPER_BOUND,
            FREE,
        }

        /// <summary>
        /// Gets or sets a value indicating whether shrinking heuristics should be used.
        /// </summary>
        /// <value>
        /// <b>true</b> to use shrinking heuristics; otherwise, <b>false</b>. Default is false.
        /// </value>
        public bool Shrinking { get; set; } = false;

        /// <summary>
        /// Gets or sets the precision tolerance before the method stops.
        /// </summary>
        /// <value>
        /// The precision tolerance before the method stops. Default is 0.001.
        /// </value>
        public float Tolerance { get; set; } = 0.001f;

        public bool Optimize(
            int numberOfVariables,
            float[] c,
            float[] p,
            int[] y,
            Func<int, int[], int, float[], float[]> q,
            out float[] solution,
            out float rho)
        {
            // make copies, these array will be modified
            p = p.ToArray();
            y = y.ToArray();

            float[] temp = new float[numberOfVariables];

            float[] g = new float[numberOfVariables];      // gradient
            float[] gbar = new float[numberOfVariables];   // gradient, if we treat free variables as 0

            float[] qd = new float[numberOfVariables];
            for (int k = 0; k < qd.Length; k++)
            {
                qd[k] = q(k, new[] { k }, 1, temp)[0];
            }

            float[] qi = new float[numberOfVariables];
            float[] qj = new float[numberOfVariables];

            bool unshrink = false;

            // Lagrange multipliers
            float[] alpha = new float[numberOfVariables];

            // initialize alpha_status
            Status[] alphaStatus = new Status[numberOfVariables];
            for (int i = 0; i < numberOfVariables; i++)
            {
                UpdateAlphaStatus(i);
            }

            // initialize active set (for shrinking)
            int activeSize = numberOfVariables;
            int[] activeSet = Arrays.Indexes(numberOfVariables);

            // initialize index lookup vector
            int[] indices = Arrays.Indexes(numberOfVariables);

            // initialize gradient
            Vectors.Copy(numberOfVariables, p, 0, g, 0);
            Vectors.Set(numberOfVariables, 0.0f, gbar, 0);
            /*for (int i = 0; i < numberOfVariables; i++)
            {
                g[i] = p[i];
                gbar[i] = 0;
            }*/

            for (int i = 0; i < numberOfVariables; i++)
            {
                if (!IsLowerBound(i))
                {
                    q(i, indices, numberOfVariables, qi);

                    Mathematics.AddProductC(numberOfVariables, qi, 0, alpha[i], g, 0);
                    /*float alpha_i = alpha[i];
                    for (int j = 0; j < numberOfVariables; j++)
                    {
                        g[j] += alpha_i * qi[j];
                    }*/

                    if (IsUpperBound(i))
                    {
                        Mathematics.AddProductC(numberOfVariables, qi, 0, c[i], gbar, 0);
                        /*for (int j = 0; j < numberOfVariables; j++)
                        {
                            gbar[j] += c[i] * qi[j];
                        }*/
                    }
                }
            }

            // optimization step
            int iter = 0;
            int max_iter = MinMax.Max(10000000, numberOfVariables > int.MaxValue / 100 ? int.MaxValue : 100 * numberOfVariables);
            int counter = MinMax.Min(numberOfVariables, 1000) + 1;

            while (iter < max_iter)
            {
                // show progress and do shrinking
                if (--counter == 0)
                {
                    counter = MinMax.Min(numberOfVariables, 1000);
                    if (this.Shrinking)
                    {
                        Shrink();
                    }

                    Trace.WriteLine(".");
                }

                if (SelectWorkingSet(out int i, out int j) != 0)
                {
                    // reconstruct the whole gradient
                    ReconstructGradient();

                    // reset active set size and check
                    activeSize = numberOfVariables;
                    Trace.WriteLine("*");

                    if (SelectWorkingSet(out i, out j) != 0)
                    {
                        break;
                    }
                    else
                    {
                        counter = 1;    // do shrinking next iteration
                    }
                }

                iter++;

                // update alpha[i] and alpha[j], handle bounds carefully
                q(i, indices, activeSize, qi);
                q(j, indices, activeSize, qj);

                float ci = c[i];
                float cj = c[j];

                float old_alpha_i = alpha[i];
                float old_alpha_j = alpha[j];

                if (y[i] != y[j])
                {
                    float quad_coef = qd[i] + qd[j] + (2.0f * qi[j]);
                    if (quad_coef <= 0)
                    {
                        quad_coef = TAU;
                    }

                    float delta = (-g[i] - g[j]) / quad_coef;
                    float diff = alpha[i] - alpha[j];
                    alpha[i] += delta;
                    alpha[j] += delta;

                    if (diff > 0)
                    {
                        if (alpha[j] < 0)
                        {
                            alpha[j] = 0;
                            alpha[i] = diff;
                        }
                    }
                    else
                    {
                        if (alpha[i] < 0)
                        {
                            alpha[i] = 0;
                            alpha[j] = -diff;
                        }
                    }

                    if (diff > ci - cj)
                    {
                        if (alpha[i] > ci)
                        {
                            alpha[i] = ci;
                            alpha[j] = ci - diff;
                        }
                    }
                    else
                    {
                        if (alpha[j] > cj)
                        {
                            alpha[j] = cj;
                            alpha[i] = cj + diff;
                        }
                    }
                }
                else
                {
                    float quad_coef = qd[i] + qd[j] - (2.0f * qi[j]);
                    if (quad_coef <= 0)
                    {
                        quad_coef = TAU;
                    }

                    float delta = (g[i] - g[j]) / quad_coef;
                    float sum = alpha[i] + alpha[j];
                    alpha[i] -= delta;
                    alpha[j] += delta;

                    if (sum > ci)
                    {
                        if (alpha[i] > ci)
                        {
                            alpha[i] = ci;
                            alpha[j] = sum - ci;
                        }
                    }
                    else
                    {
                        if (alpha[j] < 0)
                        {
                            alpha[j] = 0;
                            alpha[i] = sum;
                        }
                    }

                    if (sum > cj)
                    {
                        if (alpha[j] > cj)
                        {
                            alpha[j] = cj;
                            alpha[i] = sum - cj;
                        }
                    }
                    else
                    {
                        if (alpha[i] < 0)
                        {
                            alpha[i] = 0;
                            alpha[j] = sum;
                        }
                    }
                }

                // update G
                float delta_alpha_i = alpha[i] - old_alpha_i;
                float delta_alpha_j = alpha[j] - old_alpha_j;

                for (int k = 0; k < activeSize; k++)
                {
                    g[k] += (qi[k] * delta_alpha_i) + (qj[k] * delta_alpha_j);
                }

                // update alpha_status and G_bar
                {
                    bool ui = IsUpperBound(i);
                    bool uj = IsUpperBound(j);
                    UpdateAlphaStatus(i);
                    UpdateAlphaStatus(j);

                    if (ui != IsUpperBound(i))
                    {
                        q(i, indices, numberOfVariables, qi);
                        Mathematics.AddProductC(numberOfVariables, qi, 0, ui ? -ci : ci, gbar, 0);
                        /*if (ui)
                        {
                            for (int k = 0; k < numberOfVariables; k++)
                            {
                                gbar[k] -= ci * qi[k];
                            }
                        }
                        else
                        {
                            for (int k = 0; k < numberOfVariables; k++)
                            {
                                gbar[k] += ci * qi[k];
                            }
                        }*/
                    }

                    if (uj != IsUpperBound(j))
                    {
                        q(j, indices, numberOfVariables, qj);
                        Mathematics.AddProductC(numberOfVariables, qj, 0, uj ? -cj : cj, gbar, 0);
                        /*if (uj)
                        {
                            for (int k = 0; k < numberOfVariables; k++)
                            {
                                gbar[k] -= cj * qj[k];
                            }
                        }
                        else
                        {
                            for (int k = 0; k < numberOfVariables; k++)
                            {
                                gbar[k] += cj * qj[k];
                            }
                        }*/
                    }
                }
            }

            if (iter >= max_iter)
            {
                if (activeSize < numberOfVariables)
                {
                    // reconstruct the whole gradient to calculate objective value
                    ReconstructGradient();
                    activeSize = numberOfVariables;
                    Trace.WriteLine("*");
                }

                Trace.WriteLine("WARNING: reaching max number of iterations.");
            }

            // calculate rho
            rho = CalculateRho();

            // calculate objective value
            /*float v = 0;
            for (int i = 0; i < numberOfVariables; i++)
            {
                v += alpha[i] * (g[i] + p[i]);
            }

            si->obj = v / 2;*/

            // put back the solution
            solution = new float[numberOfVariables];
            for (int i = 0; i < numberOfVariables; i++)
            {
                solution[activeSet[i]] = alpha[i];
            }

            ////si->upper_bound_p = Cp;
            ////si->upper_bound_n = Cn;

            Trace.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "optimization finished, #iter = {0}",
                iter));

            return iter < max_iter;

            // return 1 if already optimal, return 0 otherwise
            int SelectWorkingSet(out int out_i, out int out_j)
            {
                // return i,j such that
                // i: maximizes -y_i * grad(f)_i, i in I_up(\alpha)
                // j: minimizes the decrease of obj value
                //    (if quadratic coefficient <= 0, replace it with tau)
                //    -y_j*grad(f)_j < -y_i*grad(f)_i, j in I_low(\alpha)
                float gmax = float.NegativeInfinity;
                float gmax2 = float.NegativeInfinity;
                int gmax_idx = -1;
                int gmin_idx = -1;
                float obj_diff_min = float.PositiveInfinity;

                for (int t = 0; t < activeSize; t++)
                {
                    if (y[t] == +1)
                    {
                        if (!IsUpperBound(t))
                        {
                            if (-g[t] >= gmax)
                            {
                                gmax = -g[t];
                                gmax_idx = t;
                            }
                        }
                    }
                    else
                    {
                        if (!IsLowerBound(t))
                        {
                            if (g[t] >= gmax)
                            {
                                gmax = g[t];
                                gmax_idx = t;
                            }
                        }
                    }
                }

                int i = gmax_idx;
                if (i != -1)
                {
                    q(i, indices, activeSize, temp); // NULL Q_i not accessed: Gmax=-INF if i=-1
                }

                for (int j = 0; j < activeSize; j++)
                {
                    if (y[j] == +1)
                    {
                        if (!IsLowerBound(j))
                        {
                            float grad_diff = gmax + g[j];
                            if (g[j] >= gmax2)
                            {
                                gmax2 = g[j];
                            }

                            if (grad_diff > 0)
                            {
                                float obj_diff;
                                float quad_coef = qd[i] + qd[j] - (2.0f * y[i] * temp[j]);

                                if (quad_coef > 0)
                                {
                                    obj_diff = -(grad_diff * grad_diff) / quad_coef;
                                }
                                else
                                {
                                    obj_diff = -(grad_diff * grad_diff) / TAU;
                                }

                                if (obj_diff <= obj_diff_min)
                                {
                                    gmin_idx = j;
                                    obj_diff_min = obj_diff;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!IsUpperBound(j))
                        {
                            float grad_diff = gmax - g[j];
                            if (-g[j] >= gmax2)
                            {
                                gmax2 = -g[j];
                            }

                            if (grad_diff > 0)
                            {
                                float obj_diff;
                                float quad_coef = qd[i] + qd[j] + (2.0f * y[i] * temp[j]);

                                if (quad_coef > 0)
                                {
                                    obj_diff = -(grad_diff * grad_diff) / quad_coef;
                                }
                                else
                                {
                                    obj_diff = -(grad_diff * grad_diff) / TAU;
                                }

                                if (obj_diff <= obj_diff_min)
                                {
                                    gmin_idx = j;
                                    obj_diff_min = obj_diff;
                                }
                            }
                        }
                    }
                }

                if (gmax + gmax2 < this.Tolerance)
                {
                    out_i = 0;
                    out_j = 0;
                    return 1;
                }

                out_i = gmax_idx;
                out_j = gmin_idx;
                return 0;
            }

            void Shrink()
            {
                float gmax1 = float.NegativeInfinity;   // max { -y_i * grad(f)_i | i in I_up(\alpha) }
                float gmax2 = float.NegativeInfinity;   // max { y_i * grad(f)_i | i in I_low(\alpha) }

                // find maximal violating pair first
                for (int i = 0; i < activeSize; i++)
                {
                    if (y[i] == 1)
                    {
                        if (!IsUpperBound(i))
                        {
                            if (-g[i] >= gmax1)
                            {
                                gmax1 = -g[i];
                            }
                        }

                        if (!IsLowerBound(i))
                        {
                            if (g[i] >= gmax2)
                            {
                                gmax2 = g[i];
                            }
                        }
                    }
                    else
                    {
                        if (!IsUpperBound(i))
                        {
                            if (-g[i] >= gmax2)
                            {
                                gmax2 = -g[i];
                            }
                        }

                        if (!IsLowerBound(i))
                        {
                            if (g[i] >= gmax1)
                            {
                                gmax1 = g[i];
                            }
                        }
                    }
                }

                if (!unshrink && gmax1 + gmax2 <= this.Tolerance * 10)
                {
                    unshrink = true;
                    ReconstructGradient();
                    activeSize = numberOfVariables;

                    Trace.WriteLine("*");
                }

                for (int i = 0; i < activeSize; i++)
                {
                    if (IsShrunk(i, gmax1, gmax2))
                    {
                        activeSize--;
                        while (activeSize > i)
                        {
                            if (!IsShrunk(activeSize, gmax1, gmax2))
                            {
                                SwapIndex(i, activeSize);
                                break;
                            }

                            activeSize--;
                        }
                    }
                }
            }

            void ReconstructGradient()
            {
                // reconstruct inactive elements of G from G_bar and free variables
                if (activeSize == numberOfVariables)
                {
                    return;
                }

                Mathematics.Add(
                    numberOfVariables - activeSize,
                    gbar,
                    activeSize,
                    p,
                    activeSize,
                    g,
                    activeSize);
                /*for (int j = activeSize; j < numberOfVariables; j++)
                {
                    g[j] = gbar[j] + p[j];
                }*/

                int freeCount = 0;
                for (int j = 0; j < activeSize; j++)
                {
                    if (IsFree(j))
                    {
                        freeCount++;
                    }
                }

                if (2 * freeCount < activeSize)
                {
                    Trace.WriteLine("WARNING: using -h 0 may be faster");
                }

                if (freeCount * numberOfVariables > 2 * activeSize * (numberOfVariables - activeSize))
                {
                    for (int i = activeSize; i < numberOfVariables; i++)
                    {
                        q(indices[i], indices, activeSize, temp);

                        for (int j = 0; j < activeSize; j++)
                        {
                            if (IsFree(j))
                            {
                                g[i] += alpha[j] * temp[j];
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < activeSize; i++)
                    {
                        if (IsFree(i))
                        {
                            q(indices[i], indices, numberOfVariables, temp);

                            Mathematics.AddProductC(
                                numberOfVariables - activeSize,
                                temp,
                                activeSize,
                                alpha[i],
                                g,
                                activeSize);

                            /*float alpha_i = alpha[i];
                            for (int j = activeSize; j < numberOfVariables; j++)
                            {
                                g[j] += alpha_i * temp[j];
                            }*/
                        }
                    }
                }
            }

            void SwapIndex(int i, int j)
            {
                Swap(indices);
                Swap(y);
                Swap(g);
                Swap(alphaStatus);
                Swap(alpha);
                Swap(p);
                Swap(activeSet);
                Swap(gbar);

                void Swap<T>(T[] array)
                {
                    T t = array[i];
                    array[i] = array[j];
                    array[j] = t;
                }
            }

            void UpdateAlphaStatus(int i)
            {
                if (alpha[i] >= c[i])
                {
                    alphaStatus[i] = Status.UPPER_BOUND;
                }
                else if (alpha[i] <= 0)
                {
                    alphaStatus[i] = Status.LOWER_BOUND;
                }
                else
                {
                    alphaStatus[i] = Status.FREE;
                }
            }

            bool IsUpperBound(int i) => alphaStatus[i] == Status.UPPER_BOUND;

            bool IsLowerBound(int i) => alphaStatus[i] == Status.LOWER_BOUND;

            bool IsFree(int i) => alphaStatus[i] == Status.FREE;

            float CalculateRho()
            {
                float ub = float.PositiveInfinity;
                float lb = float.NegativeInfinity;
                int freeCount = 0;
                float freeSum = 0;

                for (int i = 0; i < activeSize; i++)
                {
                    float yg = y[i] * g[i];

                    if (IsUpperBound(i))
                    {
                        if (y[i] == -1)
                        {
                            ub = MinMax.Min(ub, yg);
                        }
                        else
                        {
                            lb = MinMax.Max(lb, yg);
                        }
                    }
                    else if (IsLowerBound(i))
                    {
                        if (y[i] == 1)
                        {
                            ub = MinMax.Min(ub, yg);
                        }
                        else
                        {
                            lb = MinMax.Max(lb, yg);
                        }
                    }
                    else
                    {
                        freeCount++;
                        freeSum += yg;
                    }
                }

                return freeCount > 0 ? freeSum / freeCount : (ub + lb) / 2;
            }

            bool IsShrunk(int i, float gmax1, float gmax2)
            {
                if (IsUpperBound(i))
                {
                    if (y[i] == +1)
                    {
                        return -g[i] > gmax1;
                    }

                    return -g[i] > gmax2;
                }
                else if (IsLowerBound(i))
                {
                    if (y[i] == +1)
                    {
                        return g[i] > gmax2;
                    }

                    return g[i] > gmax1;
                }

                return false;
            }
        }
    }
}
