#include "stdafx.h"

#include <stdlib.h> 
#include <math.h> 
#include <numeric>

extern "C" __declspec(dllexport) float __forceinline WINAPI logSumExp2(const float a, const float b)
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

extern "C" __declspec(dllexport) float __forceinline WINAPI logSumExp3(const float a, const float b, const float c)
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
