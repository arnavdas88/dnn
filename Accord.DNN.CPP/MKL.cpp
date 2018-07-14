#include "stdafx.h"
#include "mkl.h"

#include <math.h>

extern "C" __declspec(dllexport) void WINAPI mkl_sabs(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	a += offa;
	y += offy;

	if (n <= 32)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = ::fabsf(a[i]);
		}
	}
	else
	{
		::vsAbs(n, a, y);
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_sinv(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsInv(n, a + offa, y + offy);
}

extern "C" __declspec(dllexport) float WINAPI mkl_sdot(
	int n,
	const float* x, int offx, int incx,
	const float* y, int offy, int incy)
{
	x += offx;
	y += offy;

	if (n <= 32)
	{
		float res = 0.0f;

		if (incx == 1 && incy == 1)
		{
			for (int i = 0; i < n; i++)
			{
				res += x[i] * y[i];
			}
		}
		else
		{
			for (int i = 0, xi = 0, yi = 0; i < n; i++, xi += incx, yi += incy)
			{
				res += x[xi] * y[yi];
			}
		}

		return res;
	}
	else
	{
		return ::cblas_sdot(n, x, incx, y, incy);
	}
}

extern "C" __declspec(dllexport) float WINAPI mkl_snrm1(
	int n,
	const float* x, int offx, int incx)
{
	x += offx;

	if (n <= 32)
	{
		float sum = 0.0f;

		if (incx == 1)
		{
			for (int i = 0; i < n; i++)
			{
				sum += ::fabsf(x[i]);
			}
		}
		else
		{
			for (int i = 0; i < n; i++, x += incx)
			{
				sum += ::fabsf(*x);
			}
		}

		return sum;
	}
	else
	{
		return ::cblas_sasum(n, x, incx);
	}
}

extern "C" __declspec(dllexport) float WINAPI mkl_snrm2(
	int n,
	const float* x, int offx, int incx)
{
	x += offx;

	if (n <= 32)
	{
		float sum = 0.0f;

		if (incx == 1)
		{
			for (int i = 0; i < n; i++)
			{
				sum += x[i] * x[i];
			}
		}
		else
		{
			for (int i = 0; i < n; i++, x += incx)
			{
				sum += *x * *x;
			}
		}

		return sum;
	}
	else
	{
		return ::cblas_snrm2(n, x, incx);
	}
}
