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

extern "C" __declspec(dllexport) void WINAPI mkl_sadd(
	int n,
	const float* a, int offa,
	const float* b, int offb,
	float* y, int offy)
{
	a += offa;
	b += offb;
	y += offy;

	if (n <= 32)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = a[i] + b[i];
		}
	}
	else
	{
		::vsAdd(n, a, b, y);
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_sadd_inc(
	int n,
	const float* a, int offa, int inca,
	const float* b, int offb, int incb,
	float* y, int offy, int incy)
{
	a += offa;
	b += offb;
	y += offy;

	if (inca == 1 && incb == 1 && incy == 1)
	{
		if (n <= 32)
		{
			for (int i = 0; i < n; i++)
			{
				y[i] = a[i] + b[i];
			}
		}
		else
		{
			::vsAdd(n, a, b, y);
		}
	}
	else
	{
		for (int i = 0; i < n; i++, a += inca, b += incb, y += incy)
		{
			*y = *a + *b;
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_ssub(
	int n,
	const float* a, int offa, int inca,
	const float* b, int offb, int incb,
	float* y, int offy, int incy)
{
	a += offa;
	b += offb;
	y += offy;

	if (inca == 1 && incb == 1 && incy == 1)
	{
		if (n <= 32)
		{
			for (int i = 0; i < n; i++)
			{
				y[i] = a[i] - b[i];
			}
		}
		else
		{
			::vsSub(n, a, b, y);
		}
	}
	else
	{
		for (int i = 0; i < n; i++, a += inca, b += incb, y += incy)
		{
			*y = *a - *b;
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_sscal(
	int n,
	float a,
	float* x, int offx, int incx)
{
	x += offx;

	if (incx == 1)
	{
		if (n <= 256)
		{
			for (int i = 0; i < n; i++)
			{
				x[i] *= a;
			}
		}
		else
		{
			::cblas_sscal(n, a, x, incx);
		}
	}
	else
	{
		// VS provides better performance than MKL
		for (int i = 0, xi = 0; i < n; i++, xi += incx)
		{
			x[xi] *= a;
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_smul(
	int n,
	const float* a, int offa,
	const float* b, int offb,
	float* y, int offy)
{
	a += offa;
	b += offb;
	y += offy;

	if (n <= 32)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = a[i] * b[i];
		}
	}
	else
	{
		::vsMul(n, a, b, y);
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_smuladd(
	int n,
	const float* a, int offa,
	const float* b, int offb,
	float* y, int offy)
{
	a += offa;
	b += offb;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] += a[i] * b[i];
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_sdiv(
	int n,
	const float* a, int offa, int inca,
	const float* b, int offb, int incb,
	float* y, int offy, int incy)
{
	a += offa;
	b += offb;
	y += offy;

	if (inca == 1 && incb == 1 && incy == 1)
	{
		if (n <= 32)
		{
			for (int i = 0; i < n; i++)
			{
				y[i] = a[i] / b[i];
			}
		}
		else
		{
			::vsDiv(n, a, b, y);
		}
	}
	else
	{
		for (int i = 0; i < n; i++, a += inca, b += incb, y += incy)
		{
			*y = *a / *b;
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_ssqr(
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
			y[i] = a[i] * a[i];
		}
	}
	else
	{
		::vsSqr(n, a, y);
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_ssqrt(
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
			y[i] = ::sqrtf(a[i]);
		}
	}
	else
	{
		::vsSqrt(n, a, y);
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_spowx(
	int n,
	const float* a, int offa,
	float b,
	float* y, int offy)
{
	a += offa;
	y += offy;

	if (n <= 32)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = ::powf(a[i], b);
		}
	}
	else
	{
		::vsPowx(n, a, b, y);
	}
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

extern "C" __declspec(dllexport) void WINAPI mkl_saxpy(
	int n,
	float a,
	const float* x, int offx, int incx,
	float* y, int offy, int incy)
{
	x += offx;
	y += offy;

	if (n <= 24)
	{
		if (incx == 1 && incy == 1)
		{
			for (int i = 0; i < n; i++)
			{
				y[i] += x[i] * a;
			}
		}
		else
		{
			for (int i = 0, xi = 0, yi = 0; i < n; i++, xi += incx, yi += incy)
			{
				y[yi] += x[xi] * a;
			}
		}
	}
	else
	{
		::cblas_saxpy(n, a, x, incx, y, incy);
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_saxpby(
	int n,
	float a,
	const float* x, int offx, int incx,
	float b,
	float* y, int offy, int incy)
{
	x += offx;
	y += offy;

	if (n <= 24)
	{
		if (incx == 1 && incy == 1)
		{
			for (int i = 0; i < n; i++)
			{
				y[i] = x[i] * a + y[i] * b;
			}
		}
		else
		{
			for (int i = 0, xi = 0, yi = 0; i < n; i++, xi += incx, yi += incy)
			{
				y[yi] = x[xi] * a + y[yi] * b;
			}
		}
	}
	else
	{
		::cblas_saxpby(n, a, x, incx, b, y, incy);
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_ssin(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsSin(n, a + offa, y + offy);
}

extern "C" __declspec(dllexport) void WINAPI mkl_ssin_grad(
	int n,
	float* dx, int offdx, BOOL cleardx,
	const float* x, int offx,
	const float* dy, int offdy)
{
	dx += offdx;
	x += offx;
	dy += offdy;

	if (cleardx)
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] = ::cosf(x[i]) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += ::cosf(x[i]) * dy[i];
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_scos(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsCos(n, a + offa, y + offy);
}

extern "C" __declspec(dllexport) void WINAPI mkl_scos_grad(
	int n,
	float* dx, int offdx, BOOL cleardx,
	const float* x, int offx,
	const float* dy, int offdy)
{
	dx += offdx;
	x += offx;
	dy += offdy;

	if (cleardx)
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] = -::sinf(x[i]) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += -::sinf(x[i]) * dy[i];
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI mkl_stanh(
	int n,
	const float* x, int offx,
	float* y, int offy)
{
	::vsTanh(n, x + offx, y + offy);
}

extern "C" __declspec(dllexport) void WINAPI mkl_slog(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsLn(n, a + offa, y + offy);
}

extern "C" __declspec(dllexport) void WINAPI mkl_sexp(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsExp(n, a + offa, y + offy);
}

extern "C" __declspec(dllexport) void WINAPI mkl_sfmax(
	int n,
	const float* a, int offa,
	float* b, int offb,
	float* y, int offy)
{
	::vsFmax(n, a + offa, b + offb, y + offy);
}

extern "C" __declspec(dllexport) void WINAPI mkl_sfmin(
	int n,
	const float* a, int offa,
	float* b, int offb,
	float* y, int offy)
{
	::vsFmin(n, a + offa, b + offb, y + offy);
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
