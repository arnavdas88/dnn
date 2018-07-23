#include "stdafx.h"
#include "mkl.h"
#include "math.h"

extern "C" __declspec(dllexport) float WINAPI slogSumExp2(const float a, const float b)
{
	if (a == -INFINITY)
	{
		return b;
	}

	if (b == -INFINITY)
	{
		return a;
	}

	return ::log1pf(::expf(-::fabs(a - b))) + __max(a, b);
}

extern "C" __declspec(dllexport) float WINAPI slogSumExp3(const float a, const float b, const float c)
{
	if (a == -INFINITY)
	{
		return slogSumExp2(b, c);
	}

	if (b == -INFINITY)
	{
		return slogSumExp2(a, c);
	}

	if (c == -INFINITY)
	{
		return slogSumExp2(a, b);
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

// calculates absolute value of a vector element-wise
extern "C" __declspec(dllexport) void WINAPI sabs(
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

extern "C" __declspec(dllexport) void WINAPI sabs_gradient(
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
			dx[i] = (x[i] == y[i] ? 1.0f : -1.0f) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += (x[i] == y[i] ? 1.0f : -1.0f) * dy[i];
		}
	}
}

// inverts vector element-wise
extern "C" __declspec(dllexport) void WINAPI sinv(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsInv(n, a + offa, y + offy);
}

// adds scalar to vector element-wise in-place
template<typename T> void __forceinline __addc(
	int n,
	T a,
	T* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] += a;
	}
}

extern "C" __declspec(dllexport) void WINAPI i32addc(
	int n,
	__int32 a,
	__int32* y, int offy)
{
	__addc(n, a, y, offy);
}

extern "C" __declspec(dllexport) void WINAPI ui32addc(
	int n,
	unsigned __int32 a,
	unsigned __int32* y, int offy)
{
	__addc(n, a, y, offy);
}

extern "C" __declspec(dllexport) void WINAPI i64addc(
	int n,
	__int64 a,
	__int64* y, int offy)
{
	__addc(n, a, y, offy);
}

extern "C" __declspec(dllexport) void WINAPI ui64addc(
	int n,
	unsigned __int64 a,
	unsigned __int64* y, int offy)
{
	__addc(n, a, y, offy);
}

extern "C" __declspec(dllexport) void WINAPI saddc(
	int n,
	float a,
	float* y, int offy)
{
	__addc(n, a, y, offy);
}

// adds scalar to vector element-wise and puts results into another vector
template<typename T> void __forceinline __addxc(
	int n,
	const T* x, int offx,
	T a,
	T* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = x[i] + a;
	}
}

extern "C" __declspec(dllexport) void WINAPI i32addxc(
	int n,
	const __int32* x, int offx,
	__int32 a,
	__int32* y, int offy)
{
	__addxc(n, x, offx, a, y, offy);
}

extern "C" __declspec(dllexport) void WINAPI ui32addxc(
	int n,
	const unsigned __int32* x, int offx,
	unsigned __int32 a,
	unsigned __int32* y, int offy)
{
	__addxc(n, x, offx, a, y, offy);
}

extern "C" __declspec(dllexport) void WINAPI i64addxc(
	int n,
	const __int64* x, int offx,
	__int64 a,
	__int64* y, int offy)
{
	__addxc(n, x, offx, a, y, offy);
}

extern "C" __declspec(dllexport) void WINAPI ui64addxc(
	int n,
	const unsigned __int64* x, int offx,
	unsigned __int64 a,
	unsigned __int64* y, int offy)
{
	__addxc(n, x, offx, a, y, offy);
}

extern "C" __declspec(dllexport) void WINAPI saddxc(
	int n,
	const float* x, int offx,
	float a,
	float* y, int offy)
{
	__addxc(n, x, offx, a, y, offy);
}

// adds two vectors element-wise
template<typename T> void __forceinline __add(
	int n,
	const T* a, int offa,
	const T* b, int offb,
	T* y, int offy)
{
	a += offa;
	b += offb;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = a[i] + b[i];
	}
}

extern "C" __declspec(dllexport) void WINAPI i32add(
	int n,
	const __int32* a, int offa,
	const __int32* b, int offb,
	__int32* y, int offy)
{
	__add(n, a, offa, b, offb, y, offy);
}

extern "C" __declspec(dllexport) void WINAPI ui32add(
	int n,
	const unsigned __int32* a, int offa,
	const unsigned __int32* b, int offb,
	unsigned __int32* y, int offy)
{
	__add(n, a, offa, b, offb, y, offy);
}

extern "C" __declspec(dllexport) void WINAPI i64add(
	int n,
	const __int64* a, int offa,
	const __int64* b, int offb,
	__int64* y, int offy)
{
	__add(n, a, offa, b, offb, y, offy);
}

extern "C" __declspec(dllexport) void WINAPI ui64add(
	int n,
	const unsigned __int64* a, int offa,
	const unsigned __int64* b, int offb,
	unsigned __int64* y, int offy)
{
	__add(n, a, offa, b, offb, y, offy);
}

extern "C" __declspec(dllexport) void WINAPI sadd(
	int n,
	const float* a, int offa,
	const float* b, int offb,
	float* y, int offy)
{
	if (n <= 32)
	{
		__add(n, a, offa, b, offb, y, offy);
	}
	else
	{
		::vsAdd(n, a + offa, b + offb, y + offy);
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

extern "C" __declspec(dllexport) void WINAPI smatchandadd(
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

// L1 normalization
extern "C" __declspec(dllexport) float WINAPI _snrm1(
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

// L2 normalization
extern "C" __declspec(dllexport) float WINAPI _snrm2(
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

// sum(x)
template<typename T> T __forceinline __sum(int n, T* x, int offx)
{
	x += offx;

	T sum = T(0);
	for (int i = 0; i < n; i++)
	{
		sum += x[i];
	}

	return sum;
}

extern "C" __declspec(dllexport) __int32 WINAPI i32sum(int n, __int32* x, int offx) { return __sum(n, x, offx); }
extern "C" __declspec(dllexport) __int64 WINAPI i64sum(int n, __int64* x, int offx) { return __sum(n, x, offx); }
extern "C" __declspec(dllexport) unsigned __int32 WINAPI ui32sum(int n, unsigned __int32* x, int offx) { return __sum(n, x, offx); }
extern "C" __declspec(dllexport) unsigned __int64 WINAPI ui64sum(int n, unsigned __int64* x, int offx) { return __sum(n, x, offx); }
extern "C" __declspec(dllexport) float WINAPI ssum(int n, float* x, int offx) { return __sum(n, x, offx); }

// variance x
template<typename T> T __forceinline __variance(int n, T* x, int offx)
{
	x += offx;

	T mean = __sum(n, x, 0) / n;

	T variance = T(0);
	for (int i = 0; i < n; i++)
	{
		T delta = x[i] - mean;
		variance += delta * delta;
	}

	return variance / n;
}

extern "C" __declspec(dllexport) float WINAPI svariance(int n, float* x, int offx) { return __variance(n, x, offx); }
