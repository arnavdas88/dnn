#include "stdafx.h"
#include <math.h>
#include "mkl.h"

GENIXAPI(float, slogSumExp2)(const float a, const float b)
{
	if (a == -INFINITY)
	{
		return b;
	}

	if (b == -INFINITY)
	{
		return a;
	}

	if (a >= b)
	{
		return ::log1pf(::expf(b - a)) + a;
	}
	else
	{
		return ::log1pf(::expf(a - b)) + b;
	}
}

GENIXAPI(float, slogSumExp3)(const float a, const float b, const float c)
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
GENIXAPI(void, sabs)(
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

GENIXAPI(void, sabs_gradient)(
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
GENIXAPI(void, sinv)(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsInv(n, a + offa, y + offy);
}

// Adds a constant value to each element of a vector in-place.
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

GENIXAPI(void, i32addc)(int n, __int32 a, __int32* y, int offy) { __addc(n, a, y, offy); }
GENIXAPI(void, i64addc)(int n, __int64 a, __int64* y, int offy) { __addc(n, a, y, offy); }
GENIXAPI(void, u32addc)(int n, unsigned __int32 a, unsigned __int32* y, int offy) { __addc(n, a, y, offy); }
GENIXAPI(void, u64addc)(int n, unsigned __int64 a, unsigned __int64* y, int offy) { __addc(n, a, y, offy); }
GENIXAPI(void, saddc)(int n, float a, float* y, int offy) { __addc(n, a, y, offy); }

// Adds a constant value to each element of a vector not in-place.
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

GENIXAPI(void, i32addxc)(int n, const __int32* x, int offx, __int32 a, __int32* y, int offy) { __addxc(n, x, offx, a, y, offy); }
GENIXAPI(void, i64addxc)(int n, const __int64* x, int offx, __int64 a, __int64* y, int offy) { __addxc(n, x, offx, a, y, offy); }
GENIXAPI(void, u32addxc)(int n, const unsigned __int32* x, int offx, unsigned __int32 a, unsigned __int32* y, int offy) { __addxc(n, x, offx, a, y, offy); }
GENIXAPI(void, u64addxc)(int n, const unsigned __int64* x, int offx, unsigned __int64 a, unsigned __int64* y, int offy) { __addxc(n, x, offx, a, y, offy); }
GENIXAPI(void, saddxc)(int n, const float* x, int offx, float a, float* y, int offy) { __addxc(n, x, offx, a, y, offy); }

// Subtracts a constant value from each element of a vector in-place.
template<typename T> void __forceinline __subc(
	int n,
	T a,
	T* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] -= a;
	}
}

GENIXAPI(void, i32subc)(int n, __int32 a, __int32* y, int offy) { __subc(n, a, y, offy); }
GENIXAPI(void, i64subc)(int n, __int64 a, __int64* y, int offy) { __subc(n, a, y, offy); }
GENIXAPI(void, u32subc)(int n, unsigned __int32 a, unsigned __int32* y, int offy) { __subc(n, a, y, offy); }
GENIXAPI(void, u64subc)(int n, unsigned __int64 a, unsigned __int64* y, int offy) { __subc(n, a, y, offy); }
GENIXAPI(void, ssubc)(int n, float a, float* y, int offy) { __subc(n, a, y, offy); }

// Subtracts a constant value from each element of a vector not in-place.
template<typename T> void __forceinline __subxc(
	int n,
	const T* x, int offx,
	T a,
	T* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = x[i] - a;
	}
}

GENIXAPI(void, i32subxc)(int n, const __int32* x, int offx, __int32 a, __int32* y, int offy) { __subxc(n, x, offx, a, y, offy); }
GENIXAPI(void, i64subxc)(int n, const __int64* x, int offx, __int64 a, __int64* y, int offy) { __subxc(n, x, offx, a, y, offy); }
GENIXAPI(void, u32subxc)(int n, const unsigned __int32* x, int offx, unsigned __int32 a, unsigned __int32* y, int offy) { __subxc(n, x, offx, a, y, offy); }
GENIXAPI(void, u64subxc)(int n, const unsigned __int64* x, int offx, unsigned __int64 a, unsigned __int64* y, int offy) { __subxc(n, x, offx, a, y, offy); }
GENIXAPI(void, ssubxc)(int n, const float* x, int offx, float a, float* y, int offy) { __subxc(n, x, offx, a, y, offy); }

// Adds the elements of two vectors in-place.
template<typename T> void __forceinline __add(
	int n,
	const T* x, int offx,
	T* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] += x[i];
	}
}

GENIXAPI(void, i32add)(int n, const __int32* x, int offx, __int32* y, int offy) { __add(n, x, offx, y, offy); }
GENIXAPI(void, i64add)(int n, const __int64* x, int offx, __int64* y, int offy) { __add(n, x, offx, y, offy); }
GENIXAPI(void, u32add)(int n, const unsigned __int32* x, int offx, int offb, unsigned __int32* y, int offy) { __add(n, x, offx, y, offy); }
GENIXAPI(void, u64add)(int n, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) { __add(n, x, offx, y, offy); }
GENIXAPI(void, sadd)(int n, const float* x, int offx, float* y, int offy)
{
	if (n <= 32)
	{
		__add(n, x, offx, y, offy);
	}
	else
	{
		x += offx;
		y += offy;
		::vsAdd(n, y, x, y);
	}
}

// Adds the elements of two vectors not in-place.
template<typename T> void __forceinline __addx(
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

GENIXAPI(void, i32addx)(int n, const __int32* a, int offa, const __int32* b, int offb, __int32* y, int offy) { __addx(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, i64addx)(int n, const __int64* a, int offa, const __int64* b, int offb, __int64* y, int offy) { __addx(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, u32addx)(int n, const unsigned __int32* a, int offa, const unsigned __int32* b, int offb, unsigned __int32* y, int offy) { __addx(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, u64addx)(int n, const unsigned __int64* a, int offa, const unsigned __int64* b, int offb, unsigned __int64* y, int offy) { __addx(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, saddx)(int n, const float* a, int offa, const float* b, int offb, float* y, int offy)
{
	if (n <= 32)
	{
		__addx(n, a, offa, b, offb, y, offy);
	}
	else
	{
		::vsAdd(n, a + offa, b + offb, y + offy);
	}
}

// Adds the elements of two vectors with increment not in-place.
GENIXAPI(void, sadd_inc)(
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

GENIXAPI(void, smatchandadd)(
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

// Subtracts the elements of two vectors in-place.
template<typename T> void __forceinline __sub(
	int n,
	const T* x, int offx,
	T* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] -= x[i];
	}
}

GENIXAPI(void, i32sub)(int n, const __int32* x, int offx, __int32* y, int offy) { __sub(n, x, offx, y, offy); }
GENIXAPI(void, i64sub)(int n, const __int64* x, int offx, __int64* y, int offy) { __sub(n, x, offx, y, offy); }
GENIXAPI(void, u32sub)(int n, const unsigned __int32* x, int offx, int offb, unsigned __int32* y, int offy) { __sub(n, x, offx, y, offy); }
GENIXAPI(void, u64sub)(int n, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) { __sub(n, x, offx, y, offy); }
GENIXAPI(void, ssub)(int n, const float* x, int offx, float* y, int offy)
{
	if (n <= 32)
	{
		__sub(n, x, offx, y, offy);
	}
	else
	{
		x += offx;
		y += offy;
		::vsSub(n, y, x, y);
	}
}

// Subtracts the elements of two vectors not in-place.
template<typename T> void __forceinline __subx(
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
		y[i] = a[i] - b[i];
	}
}

GENIXAPI(void, i32subx)(int n, const __int32* a, int offa, const __int32* b, int offb, __int32* y, int offy) { __subx(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, i64subx)(int n, const __int64* a, int offa, const __int64* b, int offb, __int64* y, int offy) { __subx(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, u32subx)(int n, const unsigned __int32* a, int offa, const unsigned __int32* b, int offb, unsigned __int32* y, int offy) { __subx(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, u64subx)(int n, const unsigned __int64* a, int offa, const unsigned __int64* b, int offb, unsigned __int64* y, int offy) { __subx(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, ssubx)(int n, const float* a, int offa, const float* b, int offb, float* y, int offy)
{
	if (n <= 32)
	{
		__subx(n, a, offa, b, offb, y, offy);
	}
	else
	{
		::vsSub(n, a + offa, b + offb, y + offy);
	}
}

// Subtracts the elements of two vectors with increment not in-place.
GENIXAPI(void, ssub_inc)(
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

// Multiplies each element of a vector by a constant value in-place.
template<typename T> void __forceinline __mulc(
	int n,
	T a,
	T* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] *= a;
	}
}

GENIXAPI(void, i32mulc)(int n, __int32 a, __int32* y, int offy) { __mulc(n, a, y, offy); }
GENIXAPI(void, i64mulc)(int n, __int64 a, __int64* y, int offy) { __mulc(n, a, y, offy); }
GENIXAPI(void, ui32mulc)(int n, unsigned __int32 a, unsigned __int32* y, int offy) { __mulc(n, a, y, offy); }
GENIXAPI(void, ui64mulc)(int n, unsigned __int64 a, unsigned __int64* y, int offy) { __mulc(n, a, y, offy); }
GENIXAPI(void, smulc)(int n, float a, float* y, int offy)
{
	if (n <= 256)
	{
		__mulc(n, a, y, offy);
	}
	else
	{
		::cblas_sscal(n, a, y + offy, 1);
	}
}

GENIXAPI(void, smulc_inc)(
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
GENIXAPI(void, smulxc)(int n,
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
GENIXAPI(void, smul)(
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
GENIXAPI(void, smuladd)(
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
GENIXAPI(void, sdiv)(
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
GENIXAPI(void, _saxpy)(
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
GENIXAPI(void, _saxpby)(
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
GENIXAPI(void, ssqr)(
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
GENIXAPI(void, ssqrt)(
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
GENIXAPI(void, spowx)(
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
GENIXAPI(void, powx_gradient)(
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
GENIXAPI(void, slog)(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsLn(n, a + offa, y + offy);
}

// y = exp(a)
GENIXAPI(void, sexp)(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsExp(n, a + offa, y + offy);
}

// y = sin(a)
GENIXAPI(void, ssin)(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsSin(n, a + offa, y + offy);
}

// y += sin(a)'
GENIXAPI(void, ssin_gradient)(
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
GENIXAPI(void, scos)(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsCos(n, a + offa, y + offy);
}

// y += cos(a)'
GENIXAPI(void, scos_gradient)(
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
GENIXAPI(float, _snrm1)(
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
GENIXAPI(float, _snrm2)(
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
template<typename T> T __forceinline __sum(const int n, const T* x, const int offx)
{
	x += offx;

	T sum = T(0);
	for (int i = 0; i < n; i++)
	{
		sum += x[i];
	}

	return sum;
}

GENIXAPI(__int32, sum_u8)(const int n, const unsigned __int8* x, int offx) { return __sum(n, x, offx); }
GENIXAPI(__int32, sum_s32)(const int n, const __int32* x, const int offx) { return __sum(n, x, offx); }
GENIXAPI(__int64, sum_s64)(const int n, const __int64* x, const int offx) { return __sum(n, x, offx); }
GENIXAPI(unsigned __int32, sum_u32)(const int n, const unsigned __int32* x, const int offx) { return __sum(n, x, offx); }
GENIXAPI(unsigned __int64, sum_u64)(const int n, const unsigned __int64* x, const int offx) { return __sum(n, x, offx); }
GENIXAPI(float, sum_f32)(const int n, const float* x, const int offx) { return __sum(n, x, offx); }

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

GENIXAPI(float, svariance)(int n, float* x, int offx) { return __variance(n, x, offx); }
