#include "stdafx.h"
#include "mkl.h"

extern "C" __declspec(dllexport) void WINAPI mkl_copyf(
	int n,
	const float* x, int offx, int incx,
	float* y, int offy, int incy)
{
	x += offx;
	y += offy;

	if (n <= 64)
	{
		if (incx == 1 && incy == 1)
		{
			::memcpy(y, x, n * sizeof(float));
		}
		else
		{
			for (int i = 0, xi = 0, yi = 0; i < n; i++, xi += incx, yi += incy)
			{
				y[yi] = x[xi];
			}
		}
	}
	else
	{
		::cblas_scopy(n, x, incx, y, incy);
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_copyi16(
	int n,
	const wchar_t* x, int offx,
	wchar_t* y, int offy)
{
	::memcpy(y + offy, x + offx, n * sizeof(wchar_t));
}

extern "C" __declspec(dllexport) void WINAPI mkl_copyi32(
	int n,
	const __int32* x, int offx,
	__int32* y, int offy)
{
	::memcpy(y + offy, x + offx, n * sizeof(__int32));
}

extern "C" __declspec(dllexport) void WINAPI mkl_copyi64(
	int n,
	const __int64* x, int offx,
	__int64* y, int offy)
{
	::memcpy(y + offy, x + offx, n * sizeof(__int64));
}

extern "C" __declspec(dllexport) void WINAPI mkl_setf(
	int n,
	float a,
	float* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = a;
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_setf_inc(
	int n,
	float a,
	float* y, int offy, int incy)
{
	y += offy;

	if (incy == 1)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = a;
		}
	}
	else
	{
		for (int i = 0; i < n; i++, y += incy)
		{
			*y = a;
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_seti32(
	int n,
	__int32 a,
	__int32* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = a;
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_seti64(
	int n,
	__int64 a,
	__int64* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = a;
	}
}
