// -----------------------------------------------------------------------
// <copyright file="FanChenLinQuadraticOptimization.cs" company="Noname, Inc.">
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
    public class FanChenLinQuadraticOptimization
    {
        private const float TAU = 1e-12f;

        private enum Status
        {
            LOWER_BOUND = 0,
            UPPER_BOUND,
            FREE,
        }

        private readonly int l;
        private readonly Func<int, int[], int, float[], float[]> q;
        private readonly float[] p;
        private readonly int[] y;

        private readonly float[] alpha;
        private readonly Status[] alphaStatus;

        private readonly int[] activeSet;

        private readonly float[] g; // gradient
        private readonly float[] gbar; // gradient, if we treat free variables as 0

        private readonly int[] indices;
        private readonly float[] c;

        private float eps = 0.001f;
        private int activeSize;
        private bool unshrink;
        private bool shrinking = false;

        public FanChenLinQuadraticOptimization(
            int numberOfVariables,
            Func<int, int[], int, float[], float[]> q,
            float[] p,
            int[] y)
        {
            this.l = numberOfVariables;
            this.q = q;
            this.p = p;
            this.y = y;

            this.alpha = new float[numberOfVariables];
            this.alphaStatus = new Status[numberOfVariables];

            this.activeSet = new int[numberOfVariables];

            this.g = new float[numberOfVariables];
            this.gbar = new float[numberOfVariables];

            this.indices = new int[numberOfVariables];
            this.c = Arrays.Create(numberOfVariables, 1.0f);
        }

        public void Solve()
        {
            /*this.Q = &Q;
            QD = Q.get_QD();
            clone(p, p_, l);
            clone(y, y_, l);
            clone(alpha, alpha_, l);
            this.Cp = Cp;
            this.Cn = Cn;
            this.eps = eps;*/

            float[] temp = new float[this.l];
            float[] qd = new float[this.l];
            for (int k = 0; k < qd.Length; k++)
            {
                qd[k] = this.q(k, new[] { k }, 1, temp)[0];
            }

            float[] qi = new float[this.l];
            float[] qj = new float[this.l];

            this.unshrink = false;

            // initialize alpha_status
            for (int i = 0; i < this.l; i++)
            {
                this.UpdateAlphaStatus(i);
            }

            // initialize active set (for shrinking)
            this.activeSize = this.l;
            for (int i = 0; i < this.l; i++)
            {
                this.activeSet[i] = i;
            }

            // initialize index lookup vector
            for (int i = 0; i < this.indices.Length; i++)
            {
                this.indices[i] = i;
            }

            // initialize gradient
            Arrays.Copy(this.l, this.p, 0, this.g, 0);
            Arrays.Set(this.l, 0.0f, this.gbar, 0);

            for (int i = 0; i < this.l; i++)
            {
                if (!this.IsLowerBound(i))
                {
                    this.q(i, this.indices, this.l, qi);

                    Mathematics.MultiplyAndAdd(this.l, this.alpha[i], qi, 0, this.g, 0);

                    if (this.IsUpperBound(i))
                    {
                        Mathematics.MultiplyAndAdd(this.l, this.c[i], qi, 0, this.gbar, 0);
                    }
                }
            }

            // optimization step
            int iter = 0;
            int max_iter = Maximum.Max(10000000, this.l > int.MaxValue / 100 ? int.MaxValue : 100 * this.l);
            int counter = Maximum.Min(this.l, 1000) + 1;

            while (iter < max_iter)
            {
                // show progress and do shrinking
                if (--counter == 0)
                {
                    counter = Maximum.Min(this.l, 1000);
                    if (this.shrinking)
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
                    this.activeSize = this.l;
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
                this.q(i, this.indices, this.activeSize, qi);
                this.q(j, this.indices, this.activeSize, qj);

                float ci = this.c[i];
                float cj = this.c[j];

                float old_alpha_i = this.alpha[i];
                float old_alpha_j = this.alpha[j];

                if (this.y[i] != this.y[j])
                {
                    float quad_coef = qd[i] + qd[j] + (2 * qi[j]);
                    if (quad_coef <= 0)
                    {
                        quad_coef = TAU;
                    }

                    float delta = (-this.g[i] - this.g[j]) / quad_coef;
                    float diff = this.alpha[i] - this.alpha[j];
                    this.alpha[i] += delta;
                    this.alpha[j] += delta;

                    if (diff > 0)
                    {
                        if (this.alpha[j] < 0)
                        {
                            this.alpha[j] = 0;
                            this.alpha[i] = diff;
                        }
                    }
                    else
                    {
                        if (this.alpha[i] < 0)
                        {
                            this.alpha[i] = 0;
                            this.alpha[j] = -diff;
                        }
                    }

                    if (diff > ci - cj)
                    {
                        if (this.alpha[i] > ci)
                        {
                            this.alpha[i] = ci;
                            this.alpha[j] = ci - diff;
                        }
                    }
                    else
                    {
                        if (this.alpha[j] > cj)
                        {
                            this.alpha[j] = cj;
                            this.alpha[i] = cj + diff;
                        }
                    }
                }
                else
                {
                    float quad_coef = qd[i] + qd[j] - (2 * qi[j]);
                    if (quad_coef <= 0)
                    {
                        quad_coef = TAU;
                    }

                    float delta = (this.g[i] - this.g[j]) / quad_coef;
                    float sum = this.alpha[i] + this.alpha[j];
                    this.alpha[i] -= delta;
                    this.alpha[j] += delta;

                    if (sum > ci)
                    {
                        if (this.alpha[i] > ci)
                        {
                            this.alpha[i] = ci;
                            this.alpha[j] = sum - ci;
                        }
                    }
                    else
                    {
                        if (this.alpha[j] < 0)
                        {
                            this.alpha[j] = 0;
                            this.alpha[i] = sum;
                        }
                    }

                    if (sum > cj)
                    {
                        if (this.alpha[j] > cj)
                        {
                            this.alpha[j] = cj;
                            this.alpha[i] = sum - cj;
                        }
                    }
                    else
                    {
                        if (this.alpha[i] < 0)
                        {
                            this.alpha[i] = 0;
                            this.alpha[j] = sum;
                        }
                    }
                }

                // update G
                float delta_alpha_i = this.alpha[i] - old_alpha_i;
                float delta_alpha_j = this.alpha[j] - old_alpha_j;

                for (int k = 0; k < this.activeSize; k++)
                {
                    this.g[k] += (qi[k] * delta_alpha_i) + (qj[k] * delta_alpha_j);
                }

                // update alpha_status and G_bar
                {
                    bool ui = this.IsUpperBound(i);
                    bool uj = this.IsUpperBound(j);
                    this.UpdateAlphaStatus(i);
                    this.UpdateAlphaStatus(j);

                    if (ui != this.IsUpperBound(i))
                    {
                        this.q(i, this.indices, this.l, qi);
                        Mathematics.MultiplyAndAdd(this.l, ui ? -ci : ci, qi, 0, this.gbar, 0);
                    }

                    if (uj != this.IsUpperBound(j))
                    {
                        this.q(j, this.indices, this.l, qj);
                        Mathematics.MultiplyAndAdd(this.l, uj ? -cj : cj, qj, 0, this.gbar, 0);
                    }
                }
            }

            if (iter >= max_iter)
            {
                if (this.activeSize < this.l)
                {
                    // reconstruct the whole gradient to calculate objective value
                    ReconstructGradient();
                    this.activeSize = this.l;
                    Trace.WriteLine("*");
                }

                Trace.WriteLine("WARNING: reaching max number of iterations.");
            }

            // calculate rho

            ////si->rho = this.calculateRho();

            // calculate objective value
            float v = 0;
            for (int i = 0; i < this.l; i++)
            {
                v += this.alpha[i] * (this.g[i] + this.p[i]);
            }

            ////si->obj = v / 2;

            // put back the solution
            for (int i = 0; i < this.l; i++)
            {
                ////alpha_[this.activeSet[i]] = this.alpha[i];
            }

            ////si->upper_bound_p = Cp;
            ////si->upper_bound_n = Cn;

            Trace.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "optimization finished, #iter = {0}",
                iter));

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

                for (int t = 0; t < this.activeSize; t++)
                {
                    if (this.y[t] == +1)
                    {
                        if (!this.IsUpperBound(t))
                        {
                            if (-this.g[t] >= gmax)
                            {
                                gmax = -this.g[t];
                                gmax_idx = t;
                            }
                        }
                    }
                    else
                    {
                        if (!this.IsLowerBound(t))
                        {
                            if (this.g[t] >= gmax)
                            {
                                gmax = this.g[t];
                                gmax_idx = t;
                            }
                        }
                    }
                }

                int i = gmax_idx;
                if (i != -1)
                {
                    this.q(i, this.indices, this.activeSize, temp); // NULL Q_i not accessed: Gmax=-INF if i=-1
                }

                for (int j = 0; j < this.activeSize; j++)
                {
                    if (this.y[j] == +1)
                    {
                        if (!this.IsLowerBound(j))
                        {
                            float grad_diff = gmax + this.g[j];
                            if (this.g[j] >= gmax2)
                            {
                                gmax2 = this.g[j];
                            }

                            if (grad_diff > 0)
                            {
                                float obj_diff;
                                float quad_coef = qd[i] + qd[j] - (2.0f * this.y[i] * temp[j]);

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
                        if (!this.IsUpperBound(j))
                        {
                            float grad_diff = gmax - this.g[j];
                            if (-this.g[j] >= gmax2)
                            {
                                gmax2 = -this.g[j];
                            }

                            if (grad_diff > 0)
                            {
                                float obj_diff;
                                float quad_coef = qd[i] + qd[j] + (2.0f * this.y[i] * temp[j]);

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

                if (gmax + gmax2 < this.eps)
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
                for (int i = 0; i < this.activeSize; i++)
                {
                    if (this.y[i] == 1)
                    {
                        if (!this.IsUpperBound(i))
                        {
                            if (-this.g[i] >= gmax1)
                            {
                                gmax1 = -this.g[i];
                            }
                        }

                        if (!this.IsLowerBound(i))
                        {
                            if (this.g[i] >= gmax2)
                            {
                                gmax2 = this.g[i];
                            }
                        }
                    }
                    else
                    {
                        if (!this.IsUpperBound(i))
                        {
                            if (-this.g[i] >= gmax2)
                            {
                                gmax2 = -this.g[i];
                            }
                        }

                        if (!this.IsLowerBound(i))
                        {
                            if (this.g[i] >= gmax1)
                            {
                                gmax1 = this.g[i];
                            }
                        }
                    }
                }

                if (this.unshrink == false && gmax1 + gmax2 <= this.eps * 10)
                {
                    this.unshrink = true;
                    ReconstructGradient();
                    this.activeSize = this.l;

                    Trace.WriteLine("*");
                }

                for (int i = 0; i < this.activeSize; i++)
                {
                    if (this.IsShrunk(i, gmax1, gmax2))
                    {
                        this.activeSize--;
                        while (this.activeSize > i)
                        {
                            if (!this.IsShrunk(this.activeSize, gmax1, gmax2))
                            {
                                SwapIndex(i, this.activeSize);
                                break;
                            }

                            this.activeSize--;
                        }
                    }
                }
            }

            void ReconstructGradient()
            {
                // reconstruct inactive elements of G from G_bar and free variables
                if (this.activeSize == this.l)
                {
                    return;
                }

                Mathematics.Add(
                    this.l - this.activeSize,
                    this.gbar,
                    this.activeSize,
                    this.p,
                    this.activeSize,
                    this.g,
                    this.activeSize);

                int freeCount = 0;
                for (int j = 0; j < this.activeSize; j++)
                {
                    if (this.IsFree(j))
                    {
                        freeCount++;
                    }
                }

                if (2 * freeCount < this.activeSize)
                {
                    Trace.WriteLine("WARNING: using -h 0 may be faster");
                }

                if (freeCount * this.l > 2 * this.activeSize * (this.l - this.activeSize))
                {
                    for (int i = this.activeSize; i < this.l; i++)
                    {
                        this.q(this.indices[i], this.indices, this.activeSize, temp);

                        for (int j = 0; j < this.activeSize; j++)
                        {
                            if (this.IsFree(j))
                            {
                                this.g[i] += this.alpha[j] * temp[j];
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < this.activeSize; i++)
                    {
                        if (this.IsFree(i))
                        {
                            this.q(this.indices[i], this.indices, this.l, temp);

                            Mathematics.MultiplyAndAdd(
                                this.l - this.activeSize,
                                this.alpha[i],
                                temp,
                                this.activeSize,
                                this.g,
                                this.activeSize);
                        }
                    }
                }
            }

            void SwapIndex(int i, int j)
            {
                Swap(this.indices);
                Swap(this.y);
                Swap(this.g);
                Swap(this.alphaStatus);
                Swap(this.alpha);
                Swap(this.p);
                Swap(this.activeSet);
                Swap(this.gbar);

                void Swap<T>(T[] array)
                {
                    T t = array[i];
                    array[i] = array[j];
                    array[j] = t;
                }
            }
        }

        private void UpdateAlphaStatus(int i)
        {
            if (this.alpha[i] >= this.c[i])
            {
                this.alphaStatus[i] = Status.UPPER_BOUND;
            }
            else if (this.alpha[i] <= 0)
            {
                this.alphaStatus[i] = Status.LOWER_BOUND;
            }
            else
            {
                this.alphaStatus[i] = Status.FREE;
            }
        }

        private bool IsUpperBound(int i) => this.alphaStatus[i] == Status.UPPER_BOUND;

        private bool IsLowerBound(int i) => this.alphaStatus[i] == Status.LOWER_BOUND;

        private bool IsFree(int i) => this.alphaStatus[i] == Status.FREE;

        private float CalculateRho()
        {
            float ub = float.PositiveInfinity;
            float lb = float.NegativeInfinity;
            int freeCount = 0;
            float freeSum = 0;

            for (int i = 0; i < this.activeSize; i++)
            {
                float yg = this.y[i] * this.g[i];

                if (this.IsUpperBound(i))
                {
                    if (this.y[i] == -1)
                    {
                        ub = Maximum.Min(ub, yg);
                    }
                    else
                    {
                        lb = Maximum.Max(lb, yg);
                    }
                }
                else if (this.IsLowerBound(i))
                {
                    if (this.y[i] == 1)
                    {
                        ub = Maximum.Min(ub, yg);
                    }
                    else
                    {
                        lb = Maximum.Max(lb, yg);
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

        private bool IsShrunk(int i, float gmax1, float gmax2)
        {
            if (this.IsUpperBound(i))
            {
                if (this.y[i] == +1)
                {
                    return -this.g[i] > gmax1;
                }

                return -this.g[i] > gmax2;
            }
            else if (this.IsLowerBound(i))
            {
                if (this.y[i] == +1)
                {
                    return this.g[i] > gmax2;
                }

                return this.g[i] > gmax1;
            }

            return false;
        }
    }
}
