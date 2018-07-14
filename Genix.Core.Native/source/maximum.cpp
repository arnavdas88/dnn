#include "stdafx.h"
#include "mkl.h"

#undef min
#undef max

extern "C" __declspec(dllexport) void WINAPI min(
	int n,
	const float* a, int offa,
	float* b, int offb,
	float* y, int offy)
{
	::vsFmin(n, a + offa, b + offb, y + offy);
}

extern "C" __declspec(dllexport) void WINAPI max(
	int n,
	const float* a, int offa,
	float* b, int offb,
	float* y, int offy)
{
	::vsFmax(n, a + offa, b + offb, y + offy);
}

extern "C" __declspec(dllexport) void WINAPI minmax_gradient(
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

extern "C" __declspec(dllexport) int WINAPI argmin(int n, const float* x, int offx)
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

extern "C" __declspec(dllexport) int WINAPI argmax(int n, const float* x, int offx)
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

extern "C" __declspec(dllexport) void WINAPI argminmax(int n, const float* x, int offx, int& winmin, int& winmax)
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
