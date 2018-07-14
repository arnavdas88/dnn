#include "stdafx.h"
#include "mkl.h"

extern "C" __declspec(dllexport) int WINAPI fcompare(int n,
	const float* x, int offx,
	const float* y, int offy)
{
	return ::memcmp(x + offx, y + offy, n * sizeof(float));
}

extern "C" __declspec(dllexport) int WINAPI i32compare(int n,
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

extern "C" __declspec(dllexport) void WINAPI copyf(
	int n,
	const float* x, int offx,
	float* y, int offy)
{
	x += offx;
	y += offy;

	::memcpy(y, x, n * sizeof(float));
}

extern "C" __declspec(dllexport) void WINAPI copyf_inc(
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

extern "C" __declspec(dllexport) void WINAPI copyi16(
	int n,
	const __int16* x, int offx,
	__int16* y, int offy)
{
	::memcpy(y + offy, x + offx, n * sizeof(__int16));
}

extern "C" __declspec(dllexport) void WINAPI copyi32(
	int n,
	const __int32* x, int offx,
	__int32* y, int offy)
{
	::memcpy(y + offy, x + offx, n * sizeof(__int32));
}

extern "C" __declspec(dllexport) void WINAPI copyi64(
	int n,
	const __int64* x, int offx,
	__int64* y, int offy)
{
	::memcpy(y + offy, x + offx, n * sizeof(__int64));
}

extern "C" __declspec(dllexport) void WINAPI setf(
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

extern "C" __declspec(dllexport) void WINAPI setf_inc(
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

extern "C" __declspec(dllexport) void WINAPI seti32(
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

extern "C" __declspec(dllexport) void WINAPI seti64(
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

extern "C" __declspec(dllexport) void WINAPI pack(
	int n,
	const float* a, int offa, int inca,
	float* y, int offy)
{
	::vsPackI(n, a + offa, inca, y + offy);
}

extern "C" __declspec(dllexport) void WINAPI unpack(
	int n,
	const float* a, int offa,
	float* y, int offy, int incy)
{
	::vsUnpackI(n, a + offa, y + offy, incy);
}
