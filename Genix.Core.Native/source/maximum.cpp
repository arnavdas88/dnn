#include "stdafx.h"
#include "mkl.h"

#undef min
#undef max

// Returns the smaller of each element of an array and a scalar value.
extern "C" __declspec(dllexport) void WINAPI sminc(
	int n,
	const float* a, int offa,
	const float b,
	float* y, int offy)
{
	a += offa;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = __min(a[i], b);
	}
}

// Returns the smaller of each pair of elements of the two vector arguments.
extern "C" __declspec(dllexport) void WINAPI smin(
	int n,
	const float* a, int offa,
	float* b, int offb,
	float* y, int offy)
{
	::vsFmin(n, a + offa, b + offb, y + offy);
}

// Returns the larger of each element of an array and a scalar value.
extern "C" __declspec(dllexport) void WINAPI smaxc(
	int n,
	const float* a, int offa,
	const float b,
	float* y, int offy)
{
	a += offa;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = __max(a[i], b);
	}
}

// Returns the larger of each pair of elements of the two vector arguments.
extern "C" __declspec(dllexport) void WINAPI smax(
	int n,
	const float* a, int offa,
	float* b, int offb,
	float* y, int offy)
{
	::vsFmax(n, a + offa, b + offb, y + offy);
}

extern "C" __declspec(dllexport) void WINAPI sminmax_gradient(
	int n,
	const float* x, float* dx, int offx, BOOL cleardx,
	const float* y, const float* dy, int offy)
{
	x += offx;
	dx += offx;
	y += offy;
	dy += offy;

	if (cleardx)
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] = (x[i] == y[i] ? 1.0f : 0.0f) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += (x[i] == y[i] ? 1.0f : 0.0f) * dy[i];
		}
	}
}

extern "C" __declspec(dllexport) int WINAPI sargmin(int n, const float* x, int offx)
{
	int win = offx;
	float min = x[offx];

	for (const int last = n + offx++; offx < last; offx++)
	{
		const float value = x[offx];
		if (value < min)
		{
			win = offx;
			min = value;
		}
	}

	return win;
}

extern "C" __declspec(dllexport) int WINAPI sargmax(int n, const float* x, int offx)
{
	int win = offx;
	float max = x[offx];

	for (const int last = n + offx++; offx < last; offx++)
	{
		const float value = x[offx];
		if (value > max)
		{
			win = offx;
			max = value;
		}
	}

	return win;
}

extern "C" __declspec(dllexport) void WINAPI sargminmax(int n, const float* x, int offx, int& winmin, int& winmax)
{
	winmin = winmax = offx;
	float min = x[offx];
	float max = x[offx];

	for (const int last = n + offx++; offx < last; offx++)
	{
		const float value = x[offx];
		if (value < min)
		{
			winmin = offx;
			min = value;
		}
		else if (value > max)
		{
			winmax = offx;
			max = value;
		}
	}
}
