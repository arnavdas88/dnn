#include "stdafx.h"

#include <stdlib.h> 
#include <math.h> 
#include <numeric>

extern "C" __declspec(dllexport) float __forceinline WINAPI logSumExp2(
	const float a,
	const float b)
{
	if (a == -INFINITY)
	{
		return b;
	}

	if (b == -INFINITY)
	{
		return a;
	}

	return ::log1pf(::expf(-::abs(a - b))) + __max(a, b);
}

extern "C" __declspec(dllexport) float __forceinline WINAPI logSumExp3(
	const float a,
	const float b,
	const float c)
{
	if (a == -INFINITY)
	{
		return logSumExp2(b, c);
	}

	if (b == -INFINITY)
	{
		return logSumExp2(a, c);
	}

	if (c == -INFINITY)
	{
		return logSumExp2(a, b);
	}

	if (a >= b && a >= c)
	{
		return ::log1pf(::expf(b - a) + ::expf(c - a)) + a;
	}
	else if (b >= a && b >= c)
	{
		return ::log1pf(::expf(a - b) + ::expf(c - b)) + b;
	}
	else
	{
		return ::log1pf(::expf(a - c) + ::expf(b - c)) + c;
	}
}

GENIXAPI(void, CTCComputeAlphas)(
	int T,
	int A,
	int S,
	const float* py,
	const int* labels,
	float* pa)
{
	std::fill(pa, pa + (ptrdiff_t(S) * T), -INFINITY);

	int start = __max(0, S - (2 * T));
	int end = __min(2, S);

	for (int i = start; i < end; i++)
	{
		pa[i] = py[labels[i]];
	}

	const float* paprev = pa;
	pa += S;
	py += A;

	for (int t = 1; t < T; t++, pa += S, paprev += S, py += A)
	{
		start = __max(0, S - (2 * (T - t)));
		end = __min(2 * (t + 1), S);

		int i = start;

		if (i == 0)
		{
			pa[0] = paprev[0] + py[labels[0]];
			i++;
		}

		if (i == 1)
		{
			pa[1] = logSumExp2(paprev[1], paprev[0]) + py[labels[1]];
			i++;
		}

		for (; i < end; i++)
		{
			if ((i % 2) != 0 && labels[i] != labels[i - 2])
			{
				pa[i] = logSumExp3(paprev[i], paprev[i - 1], paprev[i - 2]) + py[labels[i]];
			}
			else
			{
				pa[i] = logSumExp2(paprev[i], paprev[i - 1]) + py[labels[i]];
			}
		}
	}
}

GENIXAPI(void, CTCComputeBetas)(
	int T,
	int A,
	int S,
	const float* py,
	const int* labels,
	float* pb)
{
	std::fill(pb, pb + (ptrdiff_t(S) * T), -INFINITY);

	int start = __max(0, S - 2);
	int end = __min(2 * T, S);

	pb += ptrdiff_t(S) * (ptrdiff_t(T) - 1);
	py += ptrdiff_t(A) * (ptrdiff_t(T) - 1);

	for (int i = start; i < end; i++)
	{
		pb[i] = py[labels[i]];
	}

	const float* pbnext = pb;
	pb -= S;
	py -= A;

	for (int t = T - 2; t >= 0; t--, pb -= S, pbnext -= S, py -= A)
	{
		start = __max(0, S - (2 * (T - t)));
		end = __min(2 * (t + 1), S);

		int i = end - 1;

		if (i == S - 1)
		{
			pb[i] = pbnext[i] + py[labels[i]];
			i--;
		}

		if (i == S - 2)
		{
			pb[i] = logSumExp2(pbnext[i], pbnext[i + 1]) + py[labels[i]];
			i--;
		}

		for (; i >= start; i--)
		{
			if ((i % 2) != 0 && labels[i] != labels[i + 2])
			{
				pb[i] = logSumExp3(pbnext[i], pbnext[i + 1], pbnext[i + 2]) + py[labels[i]];
			}
			else
			{
				pb[i] = logSumExp2(pbnext[i], pbnext[i + 1]) + py[labels[i]];
			}
		}
	}
}

GENIXAPI(void, CTCReduceAlphasBetas)(
	int T,
	int A,
	int S,
	const float* pab,
	const int* labels,
	float* pdy)
{
	std::fill(pdy, pdy + (ptrdiff_t(A) * T), -INFINITY);

	for (int t = 0; t < T; t++, pab += S, pdy += A)
	{
		const int start = __max(0, S - (2 * (T - t)));
		const int end = __min(2 * (t + 1), S);

		for (int i = start; i < end; i++)
		{
			const int li = labels[i];
			pdy[li] = logSumExp2(pdy[li], pab[i]);
		}
	}
}
