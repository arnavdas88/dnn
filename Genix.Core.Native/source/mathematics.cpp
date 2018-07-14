#include "stdafx.h"
#include "mkl.h"
#include "math.h"

// adds scalar to vector element-wise in-place
extern "C" __declspec(dllexport) void WINAPI saddc(
	int n,
	float a,
	float* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] += a;
	}
}

// adds scalar to vector element-wise and puts results into another vector
extern "C" __declspec(dllexport) void WINAPI saddxc(
	int n,
	const float* x, int offx,
	float a,
	float* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = x[i] + a;
	}
}

// adds two vectors element-wise
extern "C" __declspec(dllexport) void WINAPI sadd(
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

// adds two vectors element-wise with increment
extern "C" __declspec(dllexport) void WINAPI sadd_inc(
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

// subtracts two vectors element-wise
extern "C" __declspec(dllexport) void WINAPI ssub(
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

// multiplies vector element-wise by a scalar in-place
extern "C" __declspec(dllexport) void WINAPI smulc(
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

// multiplies vector element-wise by a scalar and puts results into another vector
extern "C" __declspec(dllexport) void WINAPI smulxc(int n,
	const float* x, int offx, int incx,
	float a,
	float* y, int offy, int incy)
{
	x += offx;
	y += offy;

	if (incx == 1 && incy == 1)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = x[i] * a;
		}
	}
	else
	{
		for (int i = 0; i < n; i++, x += incx, y += incy)
		{
			*y = *x * a;
		}
	}
}

// multiplies two vectors element-wise
extern "C" __declspec(dllexport) void WINAPI smul(
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

// multiplies two vectors element-wise and adds results to another vector
extern "C" __declspec(dllexport) void WINAPI smuladd(
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

// divides two vectors element-wise
extern "C" __declspec(dllexport) void WINAPI sdiv(
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

// y += a * x
extern "C" __declspec(dllexport) void WINAPI _saxpy(
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

// y = a * x + b * y
extern "C" __declspec(dllexport) void WINAPI _saxpby(
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

// y = a * a
extern "C" __declspec(dllexport) void WINAPI ssqr(
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

// y = a ^ 1/2
extern "C" __declspec(dllexport) void WINAPI ssqrt(
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

// y = a ^ b
extern "C" __declspec(dllexport) void WINAPI spowx(
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

// y += (a ^ b)'
extern "C" __declspec(dllexport) void WINAPI powx_gradient(
	int n,
	const float* x, float* dx, int offx, BOOL cleardx,
	float power,
	const float* dy, int offdy)
{
	x += offx;
	dx += offx;
	dy += offdy;

	if (cleardx)
	{
		if (power == 2.0f)
		{
			for (int i = 0; i < n; i++)
			{
				dx[i] += 2.0f * x[i] * dy[i];
			}
		}
		else
		{
			const float p = power - 1.0f;
			for (int i = 0; i < n; i++)
			{
				dx[i] += power * ::powf(x[i], p) * dy[i];
			}
		}
	}
	else
	{
		if (power == 2.0f)
		{
			for (int i = 0; i < n; i++)
			{
				dx[i] = 2.0f * x[i] * dy[i];
			}
		}
		else
		{
			const float p = power - 1.0f;
			for (int i = 0; i < n; i++)
			{
				dx[i] = power * ::powf(x[i], p) * dy[i];
			}
		}
	}
}

// y = ln(a)
extern "C" __declspec(dllexport) void WINAPI slog(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsLn(n, a + offa, y + offy);
}

// y = exp(a)
extern "C" __declspec(dllexport) void WINAPI sexp(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsExp(n, a + offa, y + offy);
}

// y = sin(a)
extern "C" __declspec(dllexport) void WINAPI ssin(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsSin(n, a + offa, y + offy);
}

// y += sin(a)'
extern "C" __declspec(dllexport) void WINAPI ssin_gradient(
	int n,
	const float* x, float* dx, int offx, BOOL cleardx,
	const float* dy, int offdy)
{
	x += offx;
	dx += offx;
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

// y = cos(a)
extern "C" __declspec(dllexport) void WINAPI scos(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsCos(n, a + offa, y + offy);
}

// y += cos(a)'
extern "C" __declspec(dllexport) void WINAPI scos_gradient(
	int n,
	const float* x, float* dx, int offx, BOOL cleardx,
	const float* dy, int offdy)
{
	x += offx;
	dx += offx;
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
			dx[i] -= ::sinf(x[i]) * dy[i];
		}
	}
}
