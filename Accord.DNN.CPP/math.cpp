#include "stdafx.h"

#include <math.h> 

extern "C" __declspec(dllexport) void WINAPI matchandadd(
	int n,
	const float* x, const float* xmask, int offx,
	float* y, const float* ymask, int offy)
{
	x += offx;
	y += offy;
	xmask += offx;
	ymask += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] += (xmask[i] == ymask[i] ? 1.0f : 0.0f) * x[i];
	}
}

extern "C" __declspec(dllexport) int WINAPI fcompare(int n,
	const float* x, int offx,
	const float* y, int offy)
{
	return ::memcmp(x + offx, y + offy, n * sizeof(float));
}

extern "C" __declspec(dllexport) int WINAPI icompare(int n,
	const int* x, int offx,
	const int* y, int offy)
{
	return ::memcmp(x + offx, y + offy, n * sizeof(int));
}

extern "C" __declspec(dllexport) int WINAPI ccompare(int n,
	const wchar_t* x, int offx,
	const wchar_t* y, int offy)
{
	return ::memcmp(x + offx, y + offy, n * sizeof(wchar_t));
}

extern "C" __declspec(dllexport) void WINAPI abs_derivative(
	int n,
	const float* x, float* dx, int offx,
	const float* y, const float* dy, int offy)
{
	x += offx;
	dx += offx;
	y += offy;
	dy += offy;

	for (int i = 0; i < n; i++)
	{
		dx[i] = (x[i] == y[i] ? 1.0f : -1.0f) * dy[i];
	}
}
