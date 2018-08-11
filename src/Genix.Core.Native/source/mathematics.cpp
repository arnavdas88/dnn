#include "stdafx.h"
#include <cmath>
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

// calculates absolute value of a vector element-wise in-place.
template<typename T> void __forceinline __abs_ip(
	int n,
	T* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = abs(y[i]);
	}
}

GENIXAPI(void, abs_ip_s32)(int n, __int32* y, int offy) { __abs_ip(n, y, offy); }
GENIXAPI(void, abs_ip_s64)(int n, __int64* y, int offy) { __abs_ip(n, y, offy); }
GENIXAPI(void, abs_ip_f32)(int n, float* y, int offy) { __abs_ip(n, y, offy); }
GENIXAPI(void, abs_ip_f64)(int n, double* y, int offy) { __abs_ip(n, y, offy); }

// calculates absolute value of a vector element-wise not-in-place.
template<typename T> void __forceinline __abs(
	int n,
	const T* x, int offx,
	T* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = abs(x[i]);
	}
}

GENIXAPI(void, abs_s32)(int n, const __int32* x, int offx, __int32* y, int offy) { __abs(n, x, offx, y, offy); }
GENIXAPI(void, abs_s64)(int n, const __int64* x, int offx, __int64* y, int offy) { __abs(n, x, offx, y, offy); }
GENIXAPI(void, abs_f32)(int n, const float* x, int offx, float* y, int offy)
{
	if (n <= 32)
	{
		__abs(n, x, offx, y, offy);
	}
	else
	{
		::vsAbs(n, x + offx, y + offy);
	}
}
GENIXAPI(void, abs_f64)(int n, const double* x, int offx, double* y, int offy)
{
	if (n <= 32)
	{
		__abs(n, x, offx, y, offy);
	}
	else
	{
		::vdAbs(n, x + offx, y + offy);
	}
}

GENIXAPI(void, abs_gradient_f32)(
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
template<typename T> void __forceinline __addc_ip(
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

GENIXAPI(void, addc_ip_s32)(int n, __int32 a, __int32* y, int offy) { __addc_ip(n, a, y, offy); }
GENIXAPI(void, addc_ip_s64)(int n, __int64 a, __int64* y, int offy) { __addc_ip(n, a, y, offy); }
GENIXAPI(void, addc_ip_u32)(int n, unsigned __int32 a, unsigned __int32* y, int offy) { __addc_ip(n, a, y, offy); }
GENIXAPI(void, addc_ip_u64)(int n, unsigned __int64 a, unsigned __int64* y, int offy) { __addc_ip(n, a, y, offy); }
GENIXAPI(void, addc_ip_f32)(int n, float a, float* y, int offy) { __addc_ip(n, a, y, offy); }
GENIXAPI(void, addc_ip_f64)(int n, double a, double* y, int offy) { __addc_ip(n, a, y, offy); }

// Adds a constant value to each element of a vector not in-place.
template<typename T> void __forceinline __addc(
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

GENIXAPI(void, addc_s32)(int n, const __int32* x, int offx, __int32 a, __int32* y, int offy) { __addc(n, x, offx, a, y, offy); }
GENIXAPI(void, addc_s64)(int n, const __int64* x, int offx, __int64 a, __int64* y, int offy) { __addc(n, x, offx, a, y, offy); }
GENIXAPI(void, addc_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32 a, unsigned __int32* y, int offy) { __addc(n, x, offx, a, y, offy); }
GENIXAPI(void, addc_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64 a, unsigned __int64* y, int offy) { __addc(n, x, offx, a, y, offy); }
GENIXAPI(void, addc_f32)(int n, const float* x, int offx, float a, float* y, int offy) { __addc(n, x, offx, a, y, offy); }
GENIXAPI(void, addc_f64)(int n, const double* x, int offx, double a, double* y, int offy) { __addc(n, x, offx, a, y, offy); }

// Subtracts a constant value from each element of a vector in-place.
template<typename T> void __forceinline __subc_ip(
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

GENIXAPI(void, subc_ip_s32)(int n, __int32 a, __int32* y, int offy) { __subc_ip(n, a, y, offy); }
GENIXAPI(void, subc_ip_s64)(int n, __int64 a, __int64* y, int offy) { __subc_ip(n, a, y, offy); }
GENIXAPI(void, subc_ip_u32)(int n, unsigned __int32 a, unsigned __int32* y, int offy) { __subc_ip(n, a, y, offy); }
GENIXAPI(void, subc_ip_u64)(int n, unsigned __int64 a, unsigned __int64* y, int offy) { __subc_ip(n, a, y, offy); }
GENIXAPI(void, subc_ip_f32)(int n, float a, float* y, int offy) { __subc_ip(n, a, y, offy); }
GENIXAPI(void, subc_ip_f64)(int n, double a, double* y, int offy) { __subc_ip(n, a, y, offy); }

// Subtracts a constant value from each element of a vector not in-place.
template<typename T> void __forceinline __subc(
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

GENIXAPI(void, subc_s32)(int n, const __int32* x, int offx, __int32 a, __int32* y, int offy) { __subc(n, x, offx, a, y, offy); }
GENIXAPI(void, subc_s64)(int n, const __int64* x, int offx, __int64 a, __int64* y, int offy) { __subc(n, x, offx, a, y, offy); }
GENIXAPI(void, subc_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32 a, unsigned __int32* y, int offy) { __subc(n, x, offx, a, y, offy); }
GENIXAPI(void, subc_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64 a, unsigned __int64* y, int offy) { __subc(n, x, offx, a, y, offy); }
GENIXAPI(void, subc_f32)(int n, const float* x, int offx, float a, float* y, int offy) { __subc(n, x, offx, a, y, offy); }
GENIXAPI(void, subc_f64)(int n, const double* x, int offx, double a, double* y, int offy) { __subc(n, x, offx, a, y, offy); }

// Adds the elements of two vectors in-place.
template<typename T> void __forceinline __add_ip(
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

GENIXAPI(void, add_ip_s32)(int n, const __int32* x, int offx, __int32* y, int offy) { __add_ip(n, x, offx, y, offy); }
GENIXAPI(void, add_ip_s64)(int n, const __int64* x, int offx, __int64* y, int offy) { __add_ip(n, x, offx, y, offy); }
GENIXAPI(void, add_ip_u32)(int n, const unsigned __int32* x, int offx, int offb, unsigned __int32* y, int offy) { __add_ip(n, x, offx, y, offy); }
GENIXAPI(void, add_ip_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) { __add_ip(n, x, offx, y, offy); }
GENIXAPI(void, add_ip_f32)(int n, const float* x, int offx, float* y, int offy)
{
	if (n <= 32)
	{
		__add_ip(n, x, offx, y, offy);
	}
	else
	{
		x += offx;
		y += offy;
		::vsAdd(n, y, x, y);
	}
}
GENIXAPI(void, add_ip_f64)(int n, const double* x, int offx, double* y, int offy)
{
	if (n <= 32)
	{
		__add_ip(n, x, offx, y, offy);
	}
	else
	{
		x += offx;
		y += offy;
		::vdAdd(n, y, x, y);
	}
}

// Adds the elements of two vectors not in-place.
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

GENIXAPI(void, add_s32)(int n, const __int32* a, int offa, const __int32* b, int offb, __int32* y, int offy) { __add(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, add_s64)(int n, const __int64* a, int offa, const __int64* b, int offb, __int64* y, int offy) { __add(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, add_u32)(int n, const unsigned __int32* a, int offa, const unsigned __int32* b, int offb, unsigned __int32* y, int offy) { __add(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, add_u64)(int n, const unsigned __int64* a, int offa, const unsigned __int64* b, int offb, unsigned __int64* y, int offy) { __add(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, add_f32)(int n, const float* a, int offa, const float* b, int offb, float* y, int offy)
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
GENIXAPI(void, add_f64)(int n, const double* a, int offa, const double* b, int offb, double* y, int offy)
{
	if (n <= 32)
	{
		__add(n, a, offa, b, offb, y, offy);
	}
	else
	{
		::vdAdd(n, a + offa, b + offb, y + offy);
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
template<typename T> void __forceinline __sub_ip(
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

GENIXAPI(void, sub_ip_s32)(int n, const __int32* x, int offx, __int32* y, int offy) { __sub_ip(n, x, offx, y, offy); }
GENIXAPI(void, sub_ip_s64)(int n, const __int64* x, int offx, __int64* y, int offy) { __sub_ip(n, x, offx, y, offy); }
GENIXAPI(void, sub_ip_u32)(int n, const unsigned __int32* x, int offx, int offb, unsigned __int32* y, int offy) { __sub_ip(n, x, offx, y, offy); }
GENIXAPI(void, sub_ip_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) { __sub_ip(n, x, offx, y, offy); }
GENIXAPI(void, sub_ip_f32)(int n, const float* x, int offx, float* y, int offy)
{
	if (n <= 32)
	{
		__sub_ip(n, x, offx, y, offy);
	}
	else
	{
		x += offx;
		y += offy;
		::vsSub(n, y, x, y);
	}
}
GENIXAPI(void, sub_ip_f64)(int n, const double* x, int offx, double* y, int offy)
{
	if (n <= 32)
	{
		__sub_ip(n, x, offx, y, offy);
	}
	else
	{
		x += offx;
		y += offy;
		::vdSub(n, y, x, y);
	}
}

// Subtracts the elements of two vectors not in-place.
template<typename T> void __forceinline __sub(
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

GENIXAPI(void, sub_s32)(int n, const __int32* a, int offa, const __int32* b, int offb, __int32* y, int offy) { __sub(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, sub_s64)(int n, const __int64* a, int offa, const __int64* b, int offb, __int64* y, int offy) { __sub(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, sub_u32)(int n, const unsigned __int32* a, int offa, const unsigned __int32* b, int offb, unsigned __int32* y, int offy) { __sub(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, sub_u64)(int n, const unsigned __int64* a, int offa, const unsigned __int64* b, int offb, unsigned __int64* y, int offy) { __sub(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, sub_f32)(int n, const float* a, int offa, const float* b, int offb, float* y, int offy)
{
	if (n <= 32)
	{
		__sub(n, a, offa, b, offb, y, offy);
	}
	else
	{
		::vsSub(n, a + offa, b + offb, y + offy);
	}
}
GENIXAPI(void, sub_f64)(int n, const double* a, int offa, const double* b, int offb, double* y, int offy)
{
	if (n <= 32)
	{
		__sub(n, a, offa, b, offb, y, offy);
	}
	else
	{
		::vdSub(n, a + offa, b + offb, y + offy);
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
template<typename T> void __forceinline __mulc_ip(
	const int n,
	const T a,
	T* y, const int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] *= a;
	}
}

GENIXAPI(void, mulc_ip_s32)(int n, __int32 a, __int32* y, int offy) { __mulc_ip(n, a, y, offy); }
GENIXAPI(void, mulc_ip_s64)(int n, __int64 a, __int64* y, int offy) { __mulc_ip(n, a, y, offy); }
GENIXAPI(void, mulc_ip_u32)(int n, unsigned __int32 a, unsigned __int32* y, int offy) { __mulc_ip(n, a, y, offy); }
GENIXAPI(void, mulc_ip_u64)(int n, unsigned __int64 a, unsigned __int64* y, int offy) { __mulc_ip(n, a, y, offy); }
GENIXAPI(void, mulc_ip_f32)(int n, float a, float* y, int offy)
{
	if (n <= 256)
	{
		__mulc_ip(n, a, y, offy);
	}
	else
	{
		::cblas_sscal(n, a, y + offy, 1);
	}
}
GENIXAPI(void, mulc_ip_f64)(int n, double a, double* y, int offy)
{
	if (n <= 256)
	{
		__mulc_ip(n, a, y, offy);
	}
	else
	{
		::cblas_dscal(n, a, y + offy, 1);
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

// Multiplies each element of a vector by a constant value not-in-place.
template<typename T> void __forceinline __mulc(
	const int n,
	const T* x, const int offx,
	const T a,
	T* y, const int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = x[i] * a;
	}
}

GENIXAPI(void, mulc_s32)(int n, const __int32* x, int offx, __int32 a, __int32* y, int offy) { __mulc(n, x, offx, a, y, offy); }
GENIXAPI(void, mulc_s64)(int n, const __int64* x, int offx, __int64 a, __int64* y, int offy) { __mulc(n, x, offx, a, y, offy); }
GENIXAPI(void, mulc_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32 a, unsigned __int32* y, int offy) { __mulc(n, x, offx, a, y, offy); }
GENIXAPI(void, mulc_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64 a, unsigned __int64* y, int offy) { __mulc(n, x, offx, a, y, offy); }
GENIXAPI(void, mulc_f32)(int n, const float* x, int offx, float a, float* y, int offy) { __mulc(n, x, offx, a, y, offy); }
GENIXAPI(void, mulc_f64)(int n, const double* x, int offx, double a, double* y, int offy) { __mulc(n, x, offx, a, y, offy); }

// Multiplies two vectors element-wise in-place
template<typename T> void __forceinline __mul_ip(
	int n,
	const T* x, int offx,
	T* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] *= x[i];
	}
}

GENIXAPI(void, mul_ip_s32)(int n, const __int32* x, int offx, __int32* y, int offy) { __mul_ip(n, x, offx, y, offy); }
GENIXAPI(void, mul_ip_s64)(int n, const __int64* x, int offx, __int64* y, int offy) { __mul_ip(n, x, offx, y, offy); }
GENIXAPI(void, mul_ip_u32)(int n, const unsigned __int32* x, int offx, int offb, unsigned __int32* y, int offy) { __mul_ip(n, x, offx, y, offy); }
GENIXAPI(void, mul_ip_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) { __mul_ip(n, x, offx, y, offy); }
GENIXAPI(void, mul_ip_f32)(int n, const float* x, int offx, float* y, int offy)
{
	if (n <= 32)
	{
		__mul_ip(n, x, offx, y, offy);
	}
	else
	{
		x += offx;
		y += offy;
		::vsMul(n, y, x, y);
	}
}
GENIXAPI(void, mul_ip_f64)(int n, const double* x, int offx, double* y, int offy)
{
	if (n <= 32)
	{
		__mul_ip(n, x, offx, y, offy);
	}
	else
	{
		x += offx;
		y += offy;
		::vdMul(n, y, x, y);
	}
}

// Multiplies two vectors element-wise not-in-place
template<typename T> void __forceinline __mul(
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
		y[i] = a[i] * b[i];
	}
}

GENIXAPI(void, mul_s32)(int n, const __int32* a, int offa, const __int32* b, int offb, __int32* y, int offy) { __mul(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, mul_s64)(int n, const __int64* a, int offa, const __int64* b, int offb, __int64* y, int offy) { __mul(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, mul_u32)(int n, const unsigned __int32* a, int offa, const unsigned __int32* b, int offb, unsigned __int32* y, int offy) { __mul(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, mul_u64)(int n, const unsigned __int64* a, int offa, const unsigned __int64* b, int offb, unsigned __int64* y, int offy) { __mul(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, mul_f32)(int n, const float* a, int offa, const float* b, int offb, float* y, int offy)
{
	if (n <= 32)
	{
		__mul(n, a, offa, b, offb, y, offy);
	}
	else
	{
		::vsMul(n, a + offa, b + offb, y + offy);
	}
}
GENIXAPI(void, mul_f64)(int n, const double* a, int offa, const double* b, int offb, double* y, int offy)
{
	if (n <= 32)
	{
		__mul(n, a, offa, b, offb, y, offy);
	}
	else
	{
		::vdMul(n, a + offa, b + offb, y + offy);
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

// divides each element of a vector by a constant value in-place.
template<typename T> void __forceinline __divc_ip(
	const int n,
	const T a,
	T* y, const int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] /= a;
	}
}

GENIXAPI(void, divc_ip_s32)(int n, __int32 a, __int32* y, int offy) { __divc_ip(n, a, y, offy); }
GENIXAPI(void, divc_ip_s64)(int n, __int64 a, __int64* y, int offy) { __divc_ip(n, a, y, offy); }
GENIXAPI(void, divc_ip_u32)(int n, unsigned __int32 a, unsigned __int32* y, int offy) { __divc_ip(n, a, y, offy); }
GENIXAPI(void, divc_ip_u64)(int n, unsigned __int64 a, unsigned __int64* y, int offy) { __divc_ip(n, a, y, offy); }
GENIXAPI(void, divc_ip_f32)(int n, float a, float* y, int offy) { __divc_ip(n, a, y, offy); }
GENIXAPI(void, divc_ip_f64)(int n, double a, double* y, int offy) { __divc_ip(n, a, y, offy); }

// divides each element of a vector by a constant value not-in-place.
template<typename T> void __forceinline __divc(
	const int n,
	const T* x, const int offx,
	const T a,
	T* y, const int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = x[i] / a;
	}
}

GENIXAPI(void, divc_s32)(int n, const __int32* x, int offx, __int32 a, __int32* y, int offy) { __divc(n, x, offx, a, y, offy); }
GENIXAPI(void, divc_s64)(int n, const __int64* x, int offx, __int64 a, __int64* y, int offy) { __divc(n, x, offx, a, y, offy); }
GENIXAPI(void, divc_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32 a, unsigned __int32* y, int offy) { __divc(n, x, offx, a, y, offy); }
GENIXAPI(void, divc_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64 a, unsigned __int64* y, int offy) { __divc(n, x, offx, a, y, offy); }
GENIXAPI(void, divc_f32)(int n, const float* x, int offx, float a, float* y, int offy) { __divc(n, x, offx, a, y, offy); }
GENIXAPI(void, divc_f64)(int n, const double* x, int offx, double a, double* y, int offy) { __divc(n, x, offx, a, y, offy); }

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

// Adds product of a vector and a constant to the accumulator vector.
// y += a * x
template<typename T> void __forceinline __addproductc(
	const int n,
	const T* x, const int offx,
	const T a,
	T* y, const int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] += x[i] * a;
	}
}

GENIXAPI(void, addproductc_s32)(int n, const __int32* x, int offx, __int32 a, __int32* y, int offy) { __addproductc(n, x, offx, a, y, offy); }
GENIXAPI(void, addproductc_s64)(int n, const __int64* x, int offx, __int64 a, __int64* y, int offy) { __addproductc(n, x, offx, a, y, offy); }
GENIXAPI(void, addproductc_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32 a, unsigned __int32* y, int offy) { __addproductc(n, x, offx, a, y, offy); }
GENIXAPI(void, addproductc_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64 a, unsigned __int64* y, int offy) { __addproductc(n, x, offx, a, y, offy); }
GENIXAPI(void, addproductc_f32)(int n, const float* x, int offx, float a, float* y, int offy)
{
	if (n <= 24)
	{
		__addproductc(n, x, offx, a, y, offy);
	}
	else
	{
		::cblas_saxpy(n, a, x + offx, 1, y + offy, 1);
	}
}
GENIXAPI(void, addproductc_f64)(int n, const double* x, int offx, double a, double* y, int offy)
{
	if (n <= 24)
	{
		__addproductc(n, x, offx, a, y, offy);
	}
	else
	{
		::cblas_daxpy(n, a, x + offx, 1, y + offy, 1);
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

// y = x * x
template<typename T> void __forceinline __sqr_ip(
	const int n,
	T* y, const int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = y[i] * y[i];
	}
}

GENIXAPI(void, sqr_ip_s32)(int n, __int32* y, int offy) { __sqr_ip(n, y, offy); }
GENIXAPI(void, sqr_ip_s64)(int n, __int64* y, int offy) { __sqr_ip(n, y, offy); }
GENIXAPI(void, sqr_ip_u32)(int n, unsigned __int32* y, int offy) { __sqr_ip(n, y, offy); }
GENIXAPI(void, sqr_ip_u64)(int n, unsigned __int64* y, int offy) { __sqr_ip(n, y, offy); }
GENIXAPI(void, sqr_ip_f32)(int n, float* y, int offy)
{
	if (n <= 32)
	{
		__sqr_ip(n, y, offy);
	}
	else
	{
		::vsSqr(n, y + offy, y + offy);
	}
}
GENIXAPI(void, sqr_ip_f64)(int n, double* y, int offy)
{
	if (n <= 32)
	{
		__sqr_ip(n, y, offy);
	}
	else
	{
		::vdSqr(n, y + offy, y + offy);
	}
}

template<typename T> void __forceinline __sqr(
	const int n,
	const T* x, const int offx,
	T* y, const int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = x[i] * x[i];
	}
}

GENIXAPI(void, sqr_s32)(int n, const __int32* x, int offx, __int32* y, int offy) { __sqr(n, x, offx, y, offy); }
GENIXAPI(void, sqr_s64)(int n, const __int64* x, int offx, __int64* y, int offy) { __sqr(n, x, offx, y, offy); }
GENIXAPI(void, sqr_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32* y, int offy) { __sqr(n, x, offx, y, offy); }
GENIXAPI(void, sqr_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) { __sqr(n, x, offx, y, offy); }
GENIXAPI(void, sqr_f32)(int n, const float* x, int offx, float* y, int offy)
{
	if (n <= 32)
	{
		__sqr(n, x, offx, y, offy);
	}
	else
	{
		::vsSqr(n, x + offx, y + offy);
	}
}
GENIXAPI(void, sqr_f64)(int n, const double* x, int offx, double* y, int offy)
{
	if (n <= 32)
	{
		__sqr(n, x, offx, y, offy);
	}
	else
	{
		::vdSqr(n, x + offx, y + offy);
	}
}

// y = a ^ 1/2
GENIXAPI(void, sqrt_f32)(
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

// y = sqrt(a^2 + b^2)
GENIXAPI(void, hypot_f32)(
	int n,
	const float* a, int offa,
	const float* b, int offb,
	float* y, int offy)
{
	::vsHypot(n, a + offa, b + offb, y + offy);
}

// y = a ^ b
GENIXAPI(void, powx_f32)(
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
GENIXAPI(void, powx_gradient_f32)(
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
GENIXAPI(void, log_f32)(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsLn(n, a + offa, y + offy);
}

// y = exp(a)
GENIXAPI(void, exp_f32)(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsExp(n, a + offa, y + offy);
}

// y = sin(a)
GENIXAPI(void, sin_f32)(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsSin(n, a + offa, y + offy);
}

// y += sin(a)'
GENIXAPI(void, sin_gradient_f32)(
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
GENIXAPI(void, cos_f32)(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsCos(n, a + offa, y + offy);
}

// y += cos(a)'
GENIXAPI(void, cos_gradient_f32)(
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

// y = atan2(a/b)
GENIXAPI(void, atan2_f32)(
	int n,
	const float* a, int offa,
	const float* b, int offb,
	float* y, int offy)
{
	::vsAtan2(n, a + offa, b + offb, y + offy);
}

// L1 normalization
template<typename T> T __forceinline __nrm1(
	int n,
	const T* x, int offx)
{
	x += offx;

	T sum = T(0);

	for (int i = 0; i < n; i++)
	{
		sum += abs(x[i]);
	}

	return sum;
}

GENIXAPI(float, nrm1_f32)(int n, const float* x, int offx)
{
	return n <= 32 ? __nrm1(n, x, offx) : ::cblas_sasum(n, x + offx, 1);
}
GENIXAPI(double, nrm1_f64)(int n, const double* x, int offx)
{
	return n <= 32 ? __nrm1(n, x, offx) : ::cblas_dasum(n, x + offx, 1);
}

// L2 normalization
template<typename T> T __forceinline __nrm2_squared(
	int n,
	const T* x, int offx)
{
	x += offx;

	T sum = T(0);

	for (int i = 0; i < n; i++)
	{
		sum += x[i] * x[i];
	}

	return sum;
}

GENIXAPI(float, nrm2_f32)(int n, const float* x, int offx)
{
	return n <= 32 ? ::sqrtf(__nrm2_squared(n, x, offx)) : ::cblas_snrm2(n, x + offx, 1);
}
GENIXAPI(double, nrm2_f64)(int n, const double* x, int offx)
{
	return n <= 32 ? ::sqrt(__nrm2_squared(n, x, offx)) : ::cblas_dnrm2(n, x + offx, 1);
}

// sum(x)
template<typename T, typename TArg> T __forceinline __sum(
	const int n,
	const TArg* x, const int offx)
{
	x += offx;

	T sum = T(0);
	for (int i = 0; i < n; i++)
	{
		sum += x[i];
	}

	return sum;
}

GENIXAPI(__int32, sum_u8)(const int n, const unsigned __int8* x, int offx)
{
	return __sum<__int32, unsigned __int8>(n, x, offx);
}
GENIXAPI(__int32, sum_s32)(const int n, const __int32* x, const int offx)
{
	return __sum<__int32, __int32>(n, x, offx);
}
GENIXAPI(__int64, sum_s64)(const int n, const __int64* x, const int offx)
{
	return __sum<__int64, __int64>(n, x, offx);
}
GENIXAPI(unsigned __int32, sum_u32)(const int n, const unsigned __int32* x, const int offx)
{
	return __sum<unsigned __int32, unsigned __int32>(n, x, offx);
}
GENIXAPI(unsigned __int64, sum_u64)(const int n, const unsigned __int64* x, const int offx)
{
	return __sum<unsigned __int64, unsigned __int64>(n, x, offx);
}
GENIXAPI(float, sum_f32)(const int n, const float* x, const int offx)
{
	return __sum<float, float>(n, x, offx);
}
GENIXAPI(double, sum_f64)(const int n, const double* x, const int offx)
{
	return __sum<double, double>(n, x, offx);
}

// variance x
template<typename T> T __forceinline __variance(int n, T* x, int offx)
{
	x += offx;

	T mean = __sum<T, T>(n, x, 0) / n;

	T variance = T(0);
	for (int i = 0; i < n; i++)
	{
		T delta = x[i] - mean;
		variance += delta * delta;
	}

	return variance / n;
}

GENIXAPI(float, svariance)(int n, float* x, int offx) { return __variance(n, x, offx); }

// Manhattan distance
template<typename T> T __forceinline __manhattan_distance(
	const int n,
	const T* x, const int offx,
	const T* y, const int offy)
{
	x += offx;
	y += offy;

	T sum = T(0);
	for (int i = 0; i < n; i++)
	{
		sum += ::fabs(x[i] - y[i]);
	}

	return sum;
}

GENIXAPI(float, manhattan_distance_f32)(const int n, const float* x, int offx, const float* y, int offy)
{
	return __manhattan_distance(n, x, offx, y, offy);
}
GENIXAPI(double, manhattan_distance_f64)(const int n, const double* x, int offx, const double* y, int offy)
{
	return __manhattan_distance(n, x, offx, y, offy);
}

template<typename T> T __forceinline __sparse_manhattan_distance(
	const int n,
	const int* xidx, const T* x,
	const T* y, const int offy)
{
	y += offy;

	T sum = T(0);
	for (int i = 0; i < n; i++)
	{
		sum += ::fabs(x[i] - y[xidx[i]]);
	}

	return sum;
}

GENIXAPI(float, sparse_manhattan_distance_f32)(const int n, const int* xidx, const float* x, const float* y, int offy)
{
	return __sparse_manhattan_distance(n, xidx, x, y, offy);
}
GENIXAPI(double, sparse_manhattan_distance_f64)(const int n, const int* xidx, const double* x, const double* y, int offy)
{
	return __sparse_manhattan_distance(n, xidx, x, y, offy);
}

// euclidean distance
template<typename T> T __forceinline __euclidean_distance_squared(
	const int n,
	const T* x, const int offx,
	const T* y, const int offy)
{
	x += offx;
	y += offy;

	T sum = T(0);
	for (int i = 0; i < n; i++)
	{
		T u = x[i] - y[i];
		sum += u * u;
	}

	return sum;
}

GENIXAPI(float, euclidean_distance_f32)(const int n, const float* x, int offx, const float* y, int offy)
{
	return ::sqrtf(__euclidean_distance_squared(n, x, offx, y, offy));
}
GENIXAPI(double, euclidean_distance_f64)(const int n, const double* x, int offx, const double* y, int offy)
{
	return ::sqrt(__euclidean_distance_squared(n, x, offx, y, offy));
}

template<typename T> T __forceinline __sparse_euclidean_distance_squared(
	const int n,
	const int* xidx, const T* x,
	const T* y, const int offy)
{
	y += offy;

	T sum = T(0);
	for (int i = 0; i < n; i++)
	{
		T u = x[i] - y[xidx[i]];
		sum += u * u;
	}

	return sum;
}

GENIXAPI(float, sparse_euclidean_distance_f32)(const int n, const int* xidx, const float* x, const float* y, int offy)
{
	return ::sqrtf(__sparse_euclidean_distance_squared(n, xidx, x, y, offy));
}
GENIXAPI(double, sparse_euclidean_distance_f64)(const int n, const int* xidx, const double* x, const double* y, int offy)
{
	return ::sqrt(__sparse_euclidean_distance_squared(n, xidx, x, y, offy));
}


