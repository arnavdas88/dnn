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

GENIXAPI(void, abs_ip_s8)(int n, __int8* y, int offy) { __abs_ip(n, y, offy); }
GENIXAPI(void, abs_ip_s16)(int n, __int16* y, int offy) { __abs_ip(n, y, offy); }
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

GENIXAPI(void, abs_s8)(int n, const __int8* x, int offx, __int8* y, int offy) { __abs(n, x, offx, y, offy); }
GENIXAPI(void, abs_s16)(int n, const __int16* x, int offx, __int16* y, int offy) { __abs(n, x, offx, y, offy); }
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

template<typename T> void __forceinline __abs_gradient(
	int n,
	const T* x, T* dx, int offx, BOOL cleardx,
	const T* y, const T* dy, int offy)
{
	x += offx;
	dx += offx;
	y += offy;
	dy += offy;

	if (cleardx)
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] = (x[i] == y[i] ? (T)1 : (T)-1) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += (x[i] == y[i] ? (T)1 : (T)-1) * dy[i];
		}
	}
}

GENIXAPI(void, abs_gradient_f32)(
	int n,
	const float* x, float* dx, int offx, BOOL cleardx,
	const float* y, const float* dy, int offy)
{
	__abs_gradient(n, x, dx, offx, cleardx, y, dy, offy);
}
GENIXAPI(void, abs_gradient_f64)(
	int n,
	const double* x, double* dx, int offx, BOOL cleardx,
	const double* y, const double* dy, int offy)
{
	__abs_gradient(n, x, dx, offx, cleardx, y, dy, offy);
}

// inverts vector element-wise
GENIXAPI(void, sinv)(
	int n,
	const float* a, int offa,
	float* y, int offy)
{
	::vsInv(n, a + offa, y + offy);
}

// Generic operation between a constant and a vector in-place.
template<typename T, T OP(T, T)> void __forceinline __opc_ip(int n, T a, T* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = OP(y[i], a);
	}
}
template<typename T, T OP(T, T)> void __forceinline __opc_inc_ip(int n, T a, T* y, int offy, int incy)
{
	if (incy == 1)
	{
		__opc_ip<T, OP>(n, a, y, offy);
	}
	else
	{
		y += offy;

		for (int i = 0; i < n; i++, y += incy)
		{
			y[0] = OP(y[0], a);
		}
	}
}

#define OPC_IP(op) \
GENIXAPI(void, op##c_ip_s8)(int n, __int8 a, __int8* y, int offy) { __opc_ip<__int8, op>(n, a, y, offy); } \
GENIXAPI(void, op##c_ip_s16)(int n, __int16 a, __int16* y, int offy) { __opc_ip<__int16, op>(n, a, y, offy); } \
GENIXAPI(void, op##c_ip_s32)(int n, __int32 a, __int32* y, int offy) { __opc_ip<__int32, op>(n, a, y, offy); } \
GENIXAPI(void, op##c_ip_s64)(int n, __int64 a, __int64* y, int offy) { __opc_ip<__int64, op>(n, a, y, offy); } \
GENIXAPI(void, op##c_ip_u8)(int n, unsigned __int8 a, unsigned __int8* y, int offy) { __opc_ip<unsigned __int8, op>(n, a, y, offy); } \
GENIXAPI(void, op##c_ip_u16)(int n, unsigned __int16 a, unsigned __int16* y, int offy) { __opc_ip<unsigned __int16, op>(n, a, y, offy); } \
GENIXAPI(void, op##c_ip_u32)(int n, unsigned __int32 a, unsigned __int32* y, int offy) { __opc_ip<unsigned __int32, op>(n, a, y, offy); } \
GENIXAPI(void, op##c_ip_u64)(int n, unsigned __int64 a, unsigned __int64* y, int offy) { __opc_ip<unsigned __int64, op>(n, a, y, offy); } \
GENIXAPI(void, op##c_ip_f32)(int n, float a, float* y, int offy) { __opc_ip<float, op>(n, a, y, offy); } \
GENIXAPI(void, op##c_ip_f64)(int n, double a, double* y, int offy) { __opc_ip<double, op>(n, a, y, offy); } \
\
GENIXAPI(void, op##c_inc_ip_s8)(int n, __int8 a, __int8* y, int offy, int incy) { __opc_inc_ip<__int8, op>(n, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_ip_s16)(int n, __int16 a, __int16* y, int offy, int incy) { __opc_inc_ip<__int16, op>(n, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_ip_s32)(int n, __int32 a, __int32* y, int offy, int incy) { __opc_inc_ip<__int32, op>(n, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_ip_s64)(int n, __int64 a, __int64* y, int offy, int incy) { __opc_inc_ip<__int64, op>(n, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_ip_u8)(int n, unsigned __int8 a, unsigned __int8* y, int offy, int incy) { __opc_inc_ip<unsigned __int8, op>(n, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_ip_u16)(int n, unsigned __int16 a, unsigned __int16* y, int offy, int incy) { __opc_inc_ip<unsigned __int16, op>(n, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_ip_u32)(int n, unsigned __int32 a, unsigned __int32* y, int offy, int incy) { __opc_inc_ip<unsigned __int32, op>(n, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_ip_u64)(int n, unsigned __int64 a, unsigned __int64* y, int offy, int incy) { __opc_inc_ip<unsigned __int64, op>(n, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_ip_f32)(int n, float a, float* y, int offy, int incy) { __opc_inc_ip<float, op>(n, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_ip_f64)(int n, double a, double* y, int offy, int incy) { __opc_inc_ip<double, op>(n, a, y, offy, incy); }

// Generic operation between a constant and a vector not-in-place.
template<typename T, T OP(T, T)> void __forceinline __opc(int n, const T* x, int offx, T a, T* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = OP(x[i], a);
	}
}
template<typename T, T OP(T, T)> void __forceinline __opc_inc(int n, const T* x, int offx, int incx, T a, T* y, int offy, int incy)
{
	if (incx == 1 && incy == 1)
	{
		__opc<T, OP>(n, x, offx, a, y, offy);
	}
	else
	{
		x += offx;
		y += offy;

		for (int i = 0; i < n; i++, x += incx, y += incy)
		{
			y[0] = OP(x[0], a);
		}
	}
}

#define OPC(op) \
GENIXAPI(void, op##c_s8)(int n, const __int8* x, int offx, __int8 a, __int8* y, int offy) { __opc<__int8, op>(n, x, offx, a, y, offy); } \
GENIXAPI(void, op##c_s16)(int n, const __int16* x, int offx, __int16 a, __int16* y, int offy) { __opc<__int16, op>(n, x, offx, a, y, offy); } \
GENIXAPI(void, op##c_s32)(int n, const __int32* x, int offx, __int32 a, __int32* y, int offy) { __opc<__int32, op>(n, x, offx, a, y, offy); } \
GENIXAPI(void, op##c_s64)(int n, const __int64* x, int offx, __int64 a, __int64* y, int offy) { __opc<__int64, op>(n, x, offx, a, y, offy); } \
GENIXAPI(void, op##c_u8)(int n, const unsigned __int8* x, int offx, unsigned __int8 a, unsigned __int8* y, int offy) { __opc<unsigned __int8, op>(n, x, offx, a, y, offy); } \
GENIXAPI(void, op##c_u16)(int n, const unsigned __int16* x, int offx, unsigned __int16 a, unsigned __int16* y, int offy) { __opc<unsigned __int16, op>(n, x, offx, a, y, offy); } \
GENIXAPI(void, op##c_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32 a, unsigned __int32* y, int offy) { __opc<unsigned __int32, op>(n, x, offx, a, y, offy); } \
GENIXAPI(void, op##c_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64 a, unsigned __int64* y, int offy) { __opc<unsigned __int64, op>(n, x, offx, a, y, offy); } \
GENIXAPI(void, op##c_f32)(int n, const float* x, int offx, float a, float* y, int offy) { __opc<float, op>(n, x, offx, a, y, offy); } \
GENIXAPI(void, op##c_f64)(int n, const double* x, int offx, double a, double* y, int offy) { __opc<double, op>(n, x, offx, a, y, offy); } \
\
GENIXAPI(void, op##c_inc_s8)(int n, const __int8* x, int offx, int incx, __int8 a, __int8* y, int offy, int incy) { __opc_inc<__int8, op>(n, x, offx, incx, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_s16)(int n, const __int16* x, int offx, int incx, __int16 a, __int16* y, int offy, int incy) { __opc_inc<__int16, op>(n, x, offx, incx, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_s32)(int n, const __int32* x, int offx, int incx, __int32 a, __int32* y, int offy, int incy) { __opc_inc<__int32, op>(n, x, offx, incx, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_s64)(int n, const __int64* x, int offx, int incx, __int64 a, __int64* y, int offy, int incy) { __opc_inc<__int64, op>(n, x, offx, incx, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_u8)(int n, const unsigned __int8* x, int offx, int incx, unsigned __int8 a, unsigned __int8* y, int offy, int incy) { __opc_inc<unsigned __int8, op>(n, x, offx, incx, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_u16)(int n, const unsigned __int16* x, int offx, int incx, unsigned __int16 a, unsigned __int16* y, int offy, int incy) { __opc_inc<unsigned __int16, op>(n, x, offx, incx, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_u32)(int n, const unsigned __int32* x, int offx, int incx, unsigned __int32 a, unsigned __int32* y, int offy, int incy) { __opc_inc<unsigned __int32, op>(n, x, offx, incx, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_u64)(int n, const unsigned __int64* x, int offx, int incx, unsigned __int64 a, unsigned __int64* y, int offy, int incy) { __opc_inc<unsigned __int64, op>(n, x, offx, incx, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_f32)(int n, const float* x, int offx, int incx, float a, float* y, int offy, int incy) { __opc_inc<float, op>(n, x, offx, incx, a, y, offy, incy); } \
GENIXAPI(void, op##c_inc_f64)(int n, const double* x, int offx, int incx, double a, double* y, int offy, int incy) { __opc_inc<double, op>(n, x, offx, incx, a, y, offy, incy); }

// Generic operation between two vectors in-place.
template<typename T, T OP(T, T)> void __forceinline __op_ip(int n, const T* x, int offx, T* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = OP(y[i], x[i]);
	}
}
template<typename T, T OP(T, T)> void __forceinline __op_inc_ip(int n, const T* x, int offx, int incx, T* y, int offy, int incy)
{
	if (incx == 1 && incy == 1)
	{
		__op_ip<T, OP>(n, x, offx, y, offy);
	}
	else
	{
		x += offx;
		y += offy;

		for (int i = 0; i < n; i++, x += incx, y += incy)
		{
			y[0] = OP(y[0], x[0]);
		}
	}
}

#define OP_IP(op) \
GENIXAPI(void, op##_ip_s8)(int n, const __int8* x, int offx, __int8* y, int offy) { __op_ip<__int8, op>(n, x, offx, y, offy); } \
GENIXAPI(void, op##_ip_s18)(int n, const __int16* x, int offx, __int16* y, int offy) { __op_ip<__int16, op>(n, x, offx, y, offy); } \
GENIXAPI(void, op##_ip_s32)(int n, const __int32* x, int offx, __int32* y, int offy) { __op_ip<__int32, op>(n, x, offx, y, offy); } \
GENIXAPI(void, op##_ip_s64)(int n, const __int64* x, int offx, __int64* y, int offy) { __op_ip<__int64, op>(n, x, offx, y, offy); } \
GENIXAPI(void, op##_ip_u8)(int n, const unsigned __int8* x, int offx, unsigned __int8* y, int offy) { __op_ip<unsigned __int8, op>(n, x, offx, y, offy); } \
GENIXAPI(void, op##_ip_u16)(int n, const unsigned __int16* x, int offx, unsigned __int16* y, int offy) { __op_ip<unsigned __int16, op>(n, x, offx, y, offy); } \
GENIXAPI(void, op##_ip_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32* y, int offy) { __op_ip<unsigned __int32, op>(n, x, offx, y, offy); } \
GENIXAPI(void, op##_ip_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) { __op_ip<unsigned __int64, op>(n, x, offx, y, offy); } \
GENIXAPI(void, op##_ip_f32)(int n, const float* x, int offx, float* y, int offy) { __op_ip<float, op>(n, x, offx, y, offy); } \
GENIXAPI(void, op##_ip_f64)(int n, const double* x, int offx, double* y, int offy) { __op_ip<double, op>(n, x, offx, y, offy); } \
\
GENIXAPI(void, op##_inc_ip_s8)(int n, const __int8* x, int offx, int incx, __int8* y, int offy, int incy) { __op_inc_ip<__int8, op>(n, x, offx, incx, y, offy, incy); } \
GENIXAPI(void, op##_inc_ip_s18)(int n, const __int16* x, int offx, int incx, __int16* y, int offy, int incy) { __op_inc_ip<__int16, op>(n, x, offx, incx, y, offy, incy); } \
GENIXAPI(void, op##_inc_ip_s32)(int n, const __int32* x, int offx, int incx, __int32* y, int offy, int incy) { __op_inc_ip<__int32, op>(n, x, offx, incx, y, offy, incy); } \
GENIXAPI(void, op##_inc_ip_s64)(int n, const __int64* x, int offx, int incx, __int64* y, int offy, int incy) { __op_inc_ip<__int64, op>(n, x, offx, incx, y, offy, incy); } \
GENIXAPI(void, op##_inc_ip_u8)(int n, const unsigned __int8* x, int offx, int incx, unsigned __int8* y, int offy, int incy) { __op_inc_ip<unsigned __int8, op>(n, x, offx, incx, y, offy, incy); } \
GENIXAPI(void, op##_inc_ip_u16)(int n, const unsigned __int16* x, int offx, int incx, unsigned __int16* y, int offy, int incy) { __op_inc_ip<unsigned __int16, op>(n, x, offx, incx, y, offy, incy); } \
GENIXAPI(void, op##_inc_ip_u32)(int n, const unsigned __int32* x, int offx, int incx, unsigned __int32* y, int offy, int incy) { __op_inc_ip<unsigned __int32, op>(n, x, offx, incx, y, offy, incy); } \
GENIXAPI(void, op##_inc_ip_u64)(int n, const unsigned __int64* x, int offx, int incx, unsigned __int64* y, int offy, int incy) { __op_inc_ip<unsigned __int64, op>(n, x, offx, incx, y, offy, incy); } \
GENIXAPI(void, op##_inc_ip_f32)(int n, const float* x, int offx, int incx, float* y, int offy, int incy) { __op_inc_ip<float, op>(n, x, offx, incx, y, offy, incy); } \
GENIXAPI(void, op##_inc_ip_f64)(int n, const double* x, int offx, int incx, double* y, int offy, int incy) { __op_inc_ip<double, op>(n, x, offx, incx, y, offy, incy); }

// Generic operation between two vectors not-in-place.
template<typename T, T OP(T, T)> void __forceinline __op(int n, const T* a, int offa, const T* b, int offb, T* y, int offy)
{
	a += offa;
	b += offb;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = OP(a[i], b[i]);
	}
}
template<typename T, T OP(T, T)> void __forceinline __op_inc(int n, const T* a, int offa, int inca, const T* b, int offb, int incb, T* y, int offy, int incy)
{
	if (inca == 1 && incb == 1 && incy == 1)
	{
		__op<T, OP>(n, a, offa, b, offb, y, offy);
	}
	else
	{
		a += offa;
		b += offb;
		y += offy;

		for (int i = 0; i < n; i++, a += inca, b += incb, y += incy)
		{
			y[0] = OP(a[0], b[0]);
		}
	}
}

#define OP(op) \
GENIXAPI(void, op##_s8)(int n, const __int8* a, int offa, const __int8* b, int offb, __int8* y, int offy) { __op<__int8, op>(n, a, offa, b, offb, y, offy); } \
GENIXAPI(void, op##_s16)(int n, const __int16* a, int offa, const __int16* b, int offb, __int16* y, int offy) { __op<__int16, op>(n, a, offa, b, offb, y, offy); } \
GENIXAPI(void, op##_s32)(int n, const __int32* a, int offa, const __int32* b, int offb, __int32* y, int offy) { __op<__int32, op>(n, a, offa, b, offb, y, offy); } \
GENIXAPI(void, op##_s64)(int n, const __int64* a, int offa, const __int64* b, int offb, __int64* y, int offy) { __op<__int64, op>(n, a, offa, b, offb, y, offy); } \
GENIXAPI(void, op##_u8)(int n, const unsigned __int8* a, int offa, const unsigned __int8* b, int offb, unsigned __int8* y, int offy) { __op<unsigned __int8, op>(n, a, offa, b, offb, y, offy); } \
GENIXAPI(void, op##_u16)(int n, const unsigned __int16* a, int offa, const unsigned __int16* b, int offb, unsigned __int16* y, int offy) { __op<unsigned __int16, op>(n, a, offa, b, offb, y, offy); } \
GENIXAPI(void, op##_u32)(int n, const unsigned __int32* a, int offa, const unsigned __int32* b, int offb, unsigned __int32* y, int offy) { __op<unsigned __int32, op>(n, a, offa, b, offb, y, offy); } \
GENIXAPI(void, op##_u64)(int n, const unsigned __int64* a, int offa, const unsigned __int64* b, int offb, unsigned __int64* y, int offy) { __op<unsigned __int64, op>(n, a, offa, b, offb, y, offy); } \
GENIXAPI(void, op##_f32)(int n, const float* a, int offa, const float* b, int offb, float* y, int offy) { __op<float, op>(n, a, offa, b, offb, y, offy); } \
GENIXAPI(void, op##_f64)(int n, const double* a, int offa, const double* b, int offb, double* y, int offy) { __op<double, op>(n, a, offa, b, offb, y, offy); } \
\
GENIXAPI(void, op##_inc_s8)(int n, const __int8* a, int offa, int inca, const __int8* b, int offb, int incb, __int8* y, int offy, int incy) { __op_inc<__int8, op>(n, a, offa, inca, b, offb, incb, y, offy, incy); } \
GENIXAPI(void, op##_inc_s16)(int n, const __int16* a, int offa, int inca, const __int16* b, int offb, int incb, __int16* y, int offy, int incy) { __op_inc<__int16, op>(n, a, offa, inca, b, offb, incb, y, offy, incy); } \
GENIXAPI(void, op##_inc_s32)(int n, const __int32* a, int offa, int inca, const __int32* b, int offb, int incb, __int32* y, int offy, int incy) { __op_inc<__int32, op>(n, a, offa, inca, b, offb, incb, y, offy, incy); } \
GENIXAPI(void, op##_inc_s64)(int n, const __int64* a, int offa, int inca, const __int64* b, int offb, int incb, __int64* y, int offy, int incy) { __op_inc<__int64, op>(n, a, offa, inca, b, offb, incb, y, offy, incy); } \
GENIXAPI(void, op##_inc_u8)(int n, const unsigned __int8* a, int offa, int inca, const unsigned __int8* b, int offb, int incb, unsigned __int8* y, int offy, int incy) { __op_inc<unsigned __int8, op>(n, a, offa, inca, b, offb, incb, y, offy, incy); } \
GENIXAPI(void, op##_inc_u16)(int n, const unsigned __int16* a, int offa, int inca, const unsigned __int16* b, int offb, int incb, unsigned __int16* y, int offy, int incy) { __op_inc<unsigned __int16, op>(n, a, offa, inca, b, offb, incb, y, offy, incy); } \
GENIXAPI(void, op##_inc_u32)(int n, const unsigned __int32* a, int offa, int inca, const unsigned __int32* b, int offb, int incb, unsigned __int32* y, int offy, int incy) { __op_inc<unsigned __int32, op>(n, a, offa, inca, b, offb, incb, y, offy, incy); } \
GENIXAPI(void, op##_inc_u64)(int n, const unsigned __int64* a, int offa, int inca, const unsigned __int64* b, int offb, int incb, unsigned __int64* y, int offy, int incy) { __op_inc<unsigned __int64, op>(n, a, offa, inca, b, offb, incb, y, offy, incy); } \
GENIXAPI(void, op##_inc_f32)(int n, const float* a, int offa, int inca, const float* b, int offb, int incb, float* y, int offy, int incy) { __op_inc<float, op>(n, a, offa, inca, b, offb, incb, y, offy, incy); } \
GENIXAPI(void, op##_inc_f64)(int n, const double* a, int offa, int inca, const double* b, int offb, int incb, double* y, int offy, int incy) { __op_inc<double, op>(n, a, offa, inca, b, offb, incb, y, offy, incy); }

// Adds a constant value to each element of a vector.
template<typename T> T __forceinline add(T a, T b) { return a + b; }
OPC_IP(add);
OPC(add);

// Adds the elements of two vectors.
OP_IP(add);
OP(add);

// Subtracts a constant value from each element of a vector.
template<typename T> T __forceinline sub(T a, T b) { return a - b; }
OPC_IP(sub);
OPC(sub);

// Subtracts the elements of two vectors.
OP_IP(sub);
OP(sub);

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

// Multiplies each element of a vector by a constant value.
template<typename T> T __forceinline mul(T a, T b) { return a * b; }
OPC_IP(mul);
OPC(mul);

// Multiplies the elements of two vectors.
OP_IP(mul);
OP(mul);

// Divides each element of a vector by a constant value.
template<typename T> T __forceinline div(T a, T b) { return a / b; }
OPC_IP(div);
OPC(div);

// Divides the elements of two vectors.
OP_IP(div);
OP(div);

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

template<typename T> void __forceinline __sparse_addproductc(
	const int n,
	const int* xidx, const T* x,
	const T a,
	T* y, const int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[xidx[i]] += x[i] * a;
	}
}

GENIXAPI(void, sparse_addproductc_s32)(int n, const int* xidx, const __int32* x, __int32 a, __int32* y, int offy) { __sparse_addproductc(n, xidx, x, a, y, offy); }
GENIXAPI(void, sparse_addproductc_s64)(int n, const int* xidx, const __int64* x, __int64 a, __int64* y, int offy) { __sparse_addproductc(n, xidx, x, a, y, offy); }
GENIXAPI(void, sparse_addproductc_u32)(int n, const int* xidx, const unsigned __int32* x, unsigned __int32 a, unsigned __int32* y, int offy) { __sparse_addproductc(n, xidx, x, a, y, offy); }
GENIXAPI(void, sparse_addproductc_u64)(int n, const int* xidx, const unsigned __int64* x, unsigned __int64 a, unsigned __int64* y, int offy) { __sparse_addproductc(n, xidx, x, a, y, offy); }
GENIXAPI(void, sparse_addproductc_f32)(int n, const int* xidx, const float* x, float a, float* y, int offy) { __sparse_addproductc(n, xidx, x, a, y, offy); }
GENIXAPI(void, sparse_addproductc_f64)(int n, const int* xidx, const double* x, double a, double* y, int offy) { __sparse_addproductc(n, xidx, x, a, y, offy); }

// Adds product of two vectors to the accumulator vector.
// y += a * b
template<typename T> void __forceinline __addproduct(
	const int n,
	const T* a, const int offa,
	const T* b, const int offb,
	T* y, const int offy)
{
	a += offa;
	b += offb;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] += a[i] * b[i];
	}
}

GENIXAPI(void, addproduct_s32)(int n, const __int32* a, int offa, const __int32* b, int offb, __int32* y, int offy) { __addproduct(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, addproduct_s64)(int n, const __int64* a, int offa, const __int64* b, int offb, __int64* y, int offy) { __addproduct(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, addproduct_u32)(int n, const unsigned __int32* a, int offa, const unsigned __int32* b, int offb, unsigned __int32* y, int offy) { __addproduct(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, addproduct_u64)(int n, const unsigned __int64* a, int offa, const unsigned __int64* b, int offb, unsigned __int64* y, int offy) { __addproduct(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, addproduct_f32)(int n, const float* a, int offa, const float* b, int offb, float* y, int offy) { __addproduct(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, addproduct_f64)(int n, const double* a, int offa, const double* b, int offb, double* y, int offy) { __addproduct(n, a, offa, b, offb, y, offy); }

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

// y = y ^ 1/2
GENIXAPI(void, sqrt_ip_f32)(
	int n,
	float* y, int offy)
{
	y += offy;

	if (n <= 32)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = ::sqrtf(y[i]);
		}
	}
	else
	{
		::vsSqrt(n, y, y);
	}
}
GENIXAPI(void, sqrt_ip_f64)(
	int n,
	double* y, int offy)
{
	y += offy;

	if (n <= 32)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = ::sqrt(y[i]);
		}
	}
	else
	{
		::vdSqrt(n, y, y);
	}
}

// y = x ^ 1/2
GENIXAPI(void, sqrt_f32)(
	int n,
	const float* x, int offx,
	float* y, int offy)
{
	x += offx;
	y += offy;

	if (n <= 32)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = ::sqrtf(x[i]);
		}
	}
	else
	{
		::vsSqrt(n, x, y);
	}
}
GENIXAPI(void, sqrt_f64)(
	int n,
	const double* x, int offx,
	double* y, int offy)
{
	x += offx;
	y += offy;

	if (n <= 32)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = ::sqrt(x[i]);
		}
	}
	else
	{
		::vdSqrt(n, x, y);
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

// y = ln(x)
GENIXAPI(void, log_ip_f32)(int n, float* y, int offy) { ::vsLn(n, y + offy, y + offy); }
GENIXAPI(void, log_ip_f64)(int n, double* y, int offy) { ::vdLn(n, y + offy, y + offy); }
GENIXAPI(void, log_f32)(int n, const float* x, int offx, float* y, int offy) { ::vsLn(n, x + offx, y + offy); }
GENIXAPI(void, log_f64)(int n, const double* x, int offx, double* y, int offy) { ::vdLn(n, x + offx, y + offy); }

// y = exp(x)
GENIXAPI(void, exp_ip_f32)(int n, float* y, int offy) { ::vsExp(n, y + offy, y + offy); }
GENIXAPI(void, exp_ip_f64)(int n,  double* y, int offy) { ::vdExp(n, y + offy, y + offy); }
GENIXAPI(void, exp_f32)(int n, const float* x, int offx, float* y, int offy) { ::vsExp(n, x + offx, y + offy); }
GENIXAPI(void, exp_f64)(int n, const double* x, int offx, double* y, int offy) { ::vdExp(n, x + offx, y + offy); }

// y = sin(x)
GENIXAPI(void, sin_ip_f32)(int n, float* y, int offy) { ::vsSin(n, y + offy, y + offy); }
GENIXAPI(void, sin_ip_f64)(int n, double* y, int offy) { ::vdSin(n, y + offy, y + offy); }
GENIXAPI(void, sin_f32)(int n, const float* x, int offx, float* y, int offy) { ::vsSin(n, x + offx, y + offy); }
GENIXAPI(void, sin_f64)(int n, const double* x, int offx, double* y, int offy) { ::vdSin(n, x + offx, y + offy); }

// y += sin(x)'
template<typename T> void __forceinline __sin_gradient(
	int n,
	const T* x, T* dx, int offx, BOOL cleardx,
	const T* dy, int offdy)
{
	x += offx;
	dx += offx;
	dy += offdy;

	if (cleardx)
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] = cos(x[i]) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += cos(x[i]) * dy[i];
		}
	}
}

GENIXAPI(void, sin_gradient_f32)(
	int n,
	const float* x, float* dx, int offx, BOOL cleardx,
	const float* y, const float* dy, int offdy)
{
	__sin_gradient(n, x, dx, offx, cleardx, dy, offdy);
}
GENIXAPI(void, sin_gradient_f64)(
	int n,
	const double* x, double* dx, int offx, BOOL cleardx,
	const double* y, const double* dy, int offdy)
{
	__sin_gradient(n, x, dx, offx, cleardx, dy, offdy);
}

// y = cos(x)
GENIXAPI(void, cos_ip_f32)(int n, float* y, int offy) { ::vsCos(n, y + offy, y + offy); }
GENIXAPI(void, cos_ip_f64)(int n, double* y, int offy) { ::vdCos(n, y + offy, y + offy); }
GENIXAPI(void, cos_f32)(int n, const float* x, int offx, float* y, int offy) { ::vsCos(n, x + offx, y + offy); }
GENIXAPI(void, cos_f64)(int n, const double* x, int offx, double* y, int offy) { ::vdCos(n, x + offx, y + offy); }

// y += cos(x)'
template<typename T> void __forceinline __cos_gradient(
	int n,
	const T* x, T* dx, int offx, BOOL cleardx,
	const T* dy, int offdy)
{
	x += offx;
	dx += offx;
	dy += offdy;

	if (cleardx)
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] = -sin(x[i]) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] -= sin(x[i]) * dy[i];
		}
	}
}

GENIXAPI(void, cos_gradient_f32)(
	int n,
	const float* x, float* dx, int offx, BOOL cleardx,
	const float* y, const float* dy, int offdy)
{
	__cos_gradient(n, x, dx, offx, cleardx, dy, offdy);
}
GENIXAPI(void, cos_gradient_f64)(
	int n,
	const double* x, double* dx, int offx, BOOL cleardx,
	const double* y, const double* dy, int offdy)
{
	__cos_gradient(n, x, dx, offx, cleardx, dy, offdy);
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

// cumulative sum(x)
template<typename T> T __forceinline __cumulative_sum_ip(
	const int n,
	T* y, const int offy)
{
	y += offy;

	T sum = T(0);
	for (int i = 0; i < n; i++)
	{
		sum += y[i];
		y[i] = sum;
	}

	return sum;
}

GENIXAPI(__int32, cumulative_sum_ip_s32)(const int n, __int32* x, const int offx) { return __cumulative_sum_ip(n, x, offx); }
GENIXAPI(__int64, cumulative_sum_ip_s64)(const int n, __int64* x, const int offx) { return __cumulative_sum_ip(n, x, offx); }
GENIXAPI(unsigned __int32, cumulative_sum_ip_u32)(const int n, unsigned __int32* x, const int offx) { return __cumulative_sum_ip(n, x, offx); }
GENIXAPI(unsigned __int64, cumulative_sum_ip_u64)(const int n, unsigned __int64* x, const int offx) { return __cumulative_sum_ip(n, x, offx); }
GENIXAPI(float, cumulative_sum_ip_f32)(const int n, float* x, const int offx) { return __cumulative_sum_ip(n, x, offx); }
GENIXAPI(double, cumulative_sum_ip_f64)(const int n, double* x, const int offx) { return __cumulative_sum_ip(n, x, offx); }

template<typename T> T __forceinline __cumulative_sum(
	const int n,
	const T* x, const int offx,
	T* y, const int offy)
{
	x += offx;
	y += offy;

	T sum = T(0);
	for (int i = 0; i < n; i++)
	{
		sum += x[i];
		y[i] = sum;
	}

	return sum;
}

GENIXAPI(__int32, cumulative_sum_s32)(int n, const __int32* x, int offx, __int32* y, int offy) { return __cumulative_sum(n, x, offx, y, offy); }
GENIXAPI(__int64, cumulative_sum_s64)(int n, const __int64* x, int offx, __int64* y, int offy) { return __cumulative_sum(n, x, offx, y, offy); }
GENIXAPI(unsigned __int32, cumulative_sum_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32* y, int offy) { return __cumulative_sum(n, x, offx, y, offy); }
GENIXAPI(unsigned __int64, cumulative_sum_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) { return __cumulative_sum(n, x, offx, y, offy); }
GENIXAPI(float, cumulative_sum_f32)(int n, const float* x, int offx, float* y, int offy) { return __cumulative_sum(n, x, offx, y, offy); }
GENIXAPI(double, cumulative_sum_f64)(int n, const double* x, int offx, double* y, int offy) { return __cumulative_sum(n, x, offx, y, offy); }

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

GENIXAPI(float, euclidean_distance_squared_f32)(const int n, const float* x, int offx, const float* y, int offy)
{
	return __euclidean_distance_squared(n, x, offx, y, offy);
}
GENIXAPI(double, euclidean_distance_squared_f64)(const int n, const double* x, int offx, const double* y, int offy)
{
	return __euclidean_distance_squared(n, x, offx, y, offy);
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


