#include "stdafx.h"
#include <cmath>

#undef min
#undef max

// Computes the smaller of each element of an array and a scalar value in-place
template<typename T> void __forceinline __minc_ip(
	int n,
	const T a,
	T* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = __min(y[i], a);
	}
}

GENIXAPI(void, minc_ip_s8)(int n, __int8 a, __int8* y, int offy) { __minc_ip(n, a, y, offy); }
GENIXAPI(void, minc_ip_s16)(int n, __int16 a, __int16* y, int offy) { __minc_ip(n, a, y, offy); }
GENIXAPI(void, minc_ip_s32)(int n, __int32 a, __int32* y, int offy) { __minc_ip(n, a, y, offy); }
GENIXAPI(void, minc_ip_s64)(int n, __int64 a, __int64* y, int offy) { __minc_ip(n, a, y, offy); }
GENIXAPI(void, minc_ip_u8)(int n, unsigned __int8 a, unsigned __int8* y, int offy) { __minc_ip(n, a, y, offy); }
GENIXAPI(void, minc_ip_u16)(int n, unsigned __int16 a, unsigned __int16* y, int offy) { __minc_ip(n, a, y, offy); }
GENIXAPI(void, minc_ip_u32)(int n, unsigned __int32 a, unsigned __int32* y, int offy) { __minc_ip(n, a, y, offy); }
GENIXAPI(void, minc_ip_u64)(int n, unsigned __int64 a, unsigned __int64* y, int offy) { __minc_ip(n, a, y, offy); }
GENIXAPI(void, minc_ip_f32)(int n, float a, float* y, int offy) { __minc_ip(n, a, y, offy); }
GENIXAPI(void, minc_ip_f64)(int n, double a, double* y, int offy) { __minc_ip(n, a, y, offy); }

// Computes the smaller of each element of an array and a scalar value not-in-place
template<typename T> void __forceinline __minc(
	int n,
	const T* x, int offx,
	const T a,
	T* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = __min(x[i], a);
	}
}

GENIXAPI(void, minc_s8)(int n, const __int8* x, int offx, __int8 a, __int8* y, int offy) { __minc(n, x, offx, a, y, offy); }
GENIXAPI(void, minc_s16)(int n, const __int16* x, int offx, __int16 a, __int16* y, int offy) { __minc(n, x, offx, a, y, offy); }
GENIXAPI(void, minc_s32)(int n, const __int32* x, int offx, __int32 a, __int32* y, int offy) { __minc(n, x, offx, a, y, offy); }
GENIXAPI(void, minc_s64)(int n, const __int64* x, int offx, __int64 a, __int64* y, int offy) { __minc(n, x, offx, a, y, offy); }
GENIXAPI(void, minc_u8)(int n, const unsigned __int8* x, int offx, unsigned __int8 a, unsigned __int8* y, int offy) { __minc(n, x, offx, a, y, offy); }
GENIXAPI(void, minc_u16)(int n, const unsigned __int16* x, int offx, unsigned __int16 a, unsigned __int16* y, int offy) { __minc(n, x, offx, a, y, offy); }
GENIXAPI(void, minc_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32 a, unsigned __int32* y, int offy) { __minc(n, x, offx, a, y, offy); }
GENIXAPI(void, minc_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64 a, unsigned __int64* y, int offy) { __minc(n, x, offx, a, y, offy); }
GENIXAPI(void, minc_f32)(int n, const float* x, int offx, float a, float* y, int offy) { __minc(n, x, offx, a, y, offy); }
GENIXAPI(void, minc_f64)(int n, const double* x, int offx, double a, double* y, int offy) { __minc(n, x, offx, a, y, offy); }

// Computes the larger of each element of an array and a scalar value in-place
template<typename T> void __forceinline __maxc_ip(
	int n,
	const T a,
	T* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = __max(y[i], a);
	}
}

GENIXAPI(void, maxc_ip_s8)(int n, __int8 a, __int8* y, int offy) { __maxc_ip(n, a, y, offy); }
GENIXAPI(void, maxc_ip_s16)(int n, __int16 a, __int16* y, int offy) { __maxc_ip(n, a, y, offy); }
GENIXAPI(void, maxc_ip_s32)(int n, __int32 a, __int32* y, int offy) { __maxc_ip(n, a, y, offy); }
GENIXAPI(void, maxc_ip_s64)(int n, __int64 a, __int64* y, int offy) { __maxc_ip(n, a, y, offy); }
GENIXAPI(void, maxc_ip_u8)(int n, unsigned __int8 a, unsigned __int8* y, int offy) { __maxc_ip(n, a, y, offy); }
GENIXAPI(void, maxc_ip_u16)(int n, unsigned __int16 a, unsigned __int16* y, int offy) { __maxc_ip(n, a, y, offy); }
GENIXAPI(void, maxc_ip_u32)(int n, unsigned __int32 a, unsigned __int32* y, int offy) { __maxc_ip(n, a, y, offy); }
GENIXAPI(void, maxc_ip_u64)(int n, unsigned __int64 a, unsigned __int64* y, int offy) { __maxc_ip(n, a, y, offy); }
GENIXAPI(void, maxc_ip_f32)(int n, float a, float* y, int offy) { __maxc_ip(n, a, y, offy); }
GENIXAPI(void, maxc_ip_f64)(int n, double a, double* y, int offy) { __maxc_ip(n, a, y, offy); }

// Computes the larger of each element of an array and a scalar value not-in-place
template<typename T> void __forceinline __maxc(
	int n,
	const T* x, int offx,
	const T a,
	T* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = __max(x[i], a);
	}
}

GENIXAPI(void, maxc_s8)(int n, const __int8* x, int offx, __int8 a, __int8* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_s16)(int n, const __int16* x, int offx, __int16 a, __int16* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_s32)(int n, const __int32* x, int offx, __int32 a, __int32* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_s64)(int n, const __int64* x, int offx, __int64 a, __int64* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_u8)(int n, const unsigned __int8* x, int offx, unsigned __int8 a, unsigned __int8* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_u16)(int n, const unsigned __int16* x, int offx, unsigned __int16 a, unsigned __int16* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32 a, unsigned __int32* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64 a, unsigned __int64* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_f32)(int n, const float* x, int offx, float a, float* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_f64)(int n, const double* x, int offx, double a, double* y, int offy) { __maxc(n, x, offx, a, y, offy); }

// Returns the smaller of each pair of elements of the two vector arguments in-place
template<typename T> void __forceinline __min_ip(
	int n,
	const T* x, int offx,
	T* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = __min(x[i], y[i]);
	}
}

GENIXAPI(void, min_ip_s8)(int n, const __int8* x, int offx, __int8* y, int offy) { __min_ip(n, x, offx, y, offy); }
GENIXAPI(void, min_ip_s16)(int n, const __int16* x, int offx, __int16* y, int offy) { __min_ip(n, x, offx, y, offy); }
GENIXAPI(void, min_ip_s32)(int n, const __int32* x, int offx, __int32* y, int offy) { __min_ip(n, x, offx, y, offy); }
GENIXAPI(void, min_ip_s64)(int n, const __int64* x, int offx, __int64* y, int offy) { __min_ip(n, x, offx, y, offy); }
GENIXAPI(void, min_ip_u8)(int n, const unsigned __int8* x, int offx, unsigned __int8* y, int offy) { __min_ip(n, x, offx, y, offy); }
GENIXAPI(void, min_ip_u16)(int n, const unsigned __int16* x, int offx, unsigned __int16* y, int offy) { __min_ip(n, x, offx, y, offy); }
GENIXAPI(void, min_ip_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32* y, int offy) { __min_ip(n, x, offx, y, offy); }
GENIXAPI(void, min_ip_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) { __min_ip(n, x, offx, y, offy); }
GENIXAPI(void, min_ip_f32)(int n, const float* x, int offx, float* y, int offy) { __min_ip(n, x, offx, y, offy); }
GENIXAPI(void, min_ip_f64)(int n, const double* x, int offx, double* y, int offy) { __min_ip(n, x, offx, y, offy); }

// Returns the smaller of each pair of elements of the two vector arguments in-place with increment
template<typename T> void __forceinline __min_inc_ip(
	int n,
	const T* x, int offx, int incx,
	T* y, int offy, int incy)
{
	for (int i = 0; i < n; i++, offx += incx, offy += incy)
	{
		y[offy] = __min(x[offx], y[offy]);
	}
}

GENIXAPI(void, min_inc_ip_s8)(int n, const __int8* x, int offx, int incx, __int8* y, int offy, int incy) { __min_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, min_inc_ip_s16)(int n, const __int16* x, int offx, int incx, __int16* y, int offy, int incy) { __min_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, min_inc_ip_s32)(int n, const __int32* x, int offx, int incx, __int32* y, int offy, int incy) { __min_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, min_inc_ip_s64)(int n, const __int64* x, int offx, int incx, __int64* y, int offy, int incy) { __min_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, min_inc_ip_u8)(int n, const unsigned __int8* x, int offx, int incx, unsigned __int8* y, int offy, int incy) { __min_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, min_inc_ip_u16)(int n, const unsigned __int16* x, int offx, int incx, unsigned __int16* y, int offy, int incy) { __min_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, min_inc_ip_u32)(int n, const unsigned __int32* x, int offx, int incx, unsigned __int32* y, int offy, int incy) { __min_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, min_inc_ip_u64)(int n, const unsigned __int64* x, int offx, int incx, unsigned __int64* y, int offy, int incy) { __min_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, min_inc_ip_f32)(int n, const float* x, int offx, int incx, float* y, int offy, int incy) { __min_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, min_inc_ip_f64)(int n, const double* x, int offx, int incx, double* y, int offy, int incy) { __min_inc_ip(n, x, offx, incx, y, offy, incy); }

// Returns the smaller of each pair of elements of the two vector arguments not-in-place
template<typename T> void __forceinline ___min(
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
		y[i] = __min(a[i], b[i]);
	}
}

GENIXAPI(void, min_s8)(int n, const __int8* a, int offa, const __int8* b, int offb, __int8* y, int offy) { ___min(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, min_s16)(int n, const __int16* a, int offa, const __int16* b, int offb, __int16* y, int offy) { ___min(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, min_s32)(int n, const __int32* a, int offa, const __int32* b, int offb, __int32* y, int offy) { ___min(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, min_s64)(int n, const __int64* a, int offa, const __int64* b, int offb, __int64* y, int offy) { ___min(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, min_u8)(int n, const unsigned __int8* a, int offa, const unsigned __int8* b, int offb, unsigned __int8* y, int offy) { ___min(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, min_u16)(int n, const unsigned __int16* a, int offa, const unsigned __int16* b, int offb, unsigned __int16* y, int offy) { ___min(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, min_u32)(int n, const unsigned __int32* a, int offa, const unsigned __int32* b, int offb, unsigned __int32* y, int offy) { ___min(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, min_u64)(int n, const unsigned __int64* a, int offa, const unsigned __int64* b, int offb, unsigned __int64* y, int offy) { ___min(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, min_f32)(int n, const float* a, int offa, const float* b, int offb, float* y, int offy) { ___min(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, min_f64)(int n, const double* a, int offa, const double* b, int offb, double* y, int offy) { ___min(n, a, offa, b, offb, y, offy); }

// Returns the smaller of each pair of elements of the two vector arguments not-in-place with increment
template<typename T> void __forceinline __min_inc(
	int n,
	const T* a, int offa, int inca,
	const T* b, int offb, int incb,
	T* y, int offy, int incy)
{
	for (int i = 0; i < n; i++, offa += inca, offb += incb, offy += incy)
	{
		y[offy] = __min(a[offa], b[offb]);
	}
}

GENIXAPI(void, min_inc_s8)(int n, const __int8* a, int offa, int inca, const __int8* b, int offb, int incb, __int8* y, int offy, int incy) { __min_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, min_inc_s16)(int n, const __int16* a, int offa, int inca, const __int16* b, int offb, int incb, __int16* y, int offy, int incy) { __min_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, min_inc_s32)(int n, const __int32* a, int offa, int inca, const __int32* b, int offb, int incb, __int32* y, int offy, int incy) { __min_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, min_inc_s64)(int n, const __int64* a, int offa, int inca, const __int64* b, int offb, int incb, __int64* y, int offy, int incy) { __min_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, min_inc_u8)(int n, const unsigned __int8* a, int offa, int inca, const unsigned __int8* b, int offb, int incb, unsigned __int8* y, int offy, int incy) { __min_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, min_inc_u16)(int n, const unsigned __int16* a, int offa, int inca, const unsigned __int16* b, int offb, int incb, unsigned __int16* y, int offy, int incy) { __min_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, min_inc_u32)(int n, const unsigned __int32* a, int offa, int inca, const unsigned __int32* b, int offb, int incb, unsigned __int32* y, int offy, int incy) { __min_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, min_inc_u64)(int n, const unsigned __int64* a, int offa, int inca, const unsigned __int64* b, int offb, int incb, unsigned __int64* y, int offy, int incy) { __min_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, min_inc_f32)(int n, const float* a, int offa, int inca, const float* b, int offb, int incb, float* y, int offy, int incy) { __min_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, min_inc_f64)(int n, const double* a, int offa, int inca, const double* b, int offb, int incb, double* y, int offy, int incy) { __min_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }

// Returns the larger of each pair of elements of the two vector arguments in-place
template<typename T> void __forceinline __max_ip(
	int n,
	const T* x, int offx,
	T* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = __max(x[i], y[i]);
	}
}

GENIXAPI(void, max_ip_s8)(int n, const __int8* x, int offx, __int8* y, int offy) { __max_ip(n, x, offx, y, offy); }
GENIXAPI(void, max_ip_s16)(int n, const __int16* x, int offx, __int16* y, int offy) { __max_ip(n, x, offx, y, offy); }
GENIXAPI(void, max_ip_s32)(int n, const __int32* x, int offx, __int32* y, int offy) { __max_ip(n, x, offx, y, offy); }
GENIXAPI(void, max_ip_s64)(int n, const __int64* x, int offx, __int64* y, int offy) { __max_ip(n, x, offx, y, offy); }
GENIXAPI(void, max_ip_u8)(int n, const unsigned __int8* x, int offx, unsigned __int8* y, int offy) { __max_ip(n, x, offx, y, offy); }
GENIXAPI(void, max_ip_u16)(int n, const unsigned __int16* x, int offx, unsigned __int16* y, int offy) { __max_ip(n, x, offx, y, offy); }
GENIXAPI(void, max_ip_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32* y, int offy) { __max_ip(n, x, offx, y, offy); }
GENIXAPI(void, max_ip_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) { __max_ip(n, x, offx, y, offy); }
GENIXAPI(void, max_ip_f32)(int n, const float* x, int offx, float* y, int offy) { __max_ip(n, x, offx, y, offy); }
GENIXAPI(void, max_ip_f64)(int n, const double* x, int offx, double* y, int offy) { __max_ip(n, x, offx, y, offy); }

// Returns the larger of each pair of elements of the two vector arguments in-place with increment
template<typename T> void __forceinline __max_inc_ip(
	int n,
	const T* x, int offx, int incx,
	T* y, int offy, int incy)
{
	for (int i = 0; i < n; i++, offx += incx, offy += incy)
	{
		y[offy] = __max(x[offx], y[offy]);
	}
}

GENIXAPI(void, max_inc_ip_s8)(int n, const __int8* x, int offx, int incx, __int8* y, int offy, int incy) { __max_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, max_inc_ip_s16)(int n, const __int16* x, int offx, int incx, __int16* y, int offy, int incy) { __max_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, max_inc_ip_s32)(int n, const __int32* x, int offx, int incx, __int32* y, int offy, int incy) { __max_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, max_inc_ip_s64)(int n, const __int64* x, int offx, int incx, __int64* y, int offy, int incy) { __max_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, max_inc_ip_u8)(int n, const unsigned __int8* x, int offx, int incx, unsigned __int8* y, int offy, int incy) { __max_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, max_inc_ip_u16)(int n, const unsigned __int16* x, int offx, int incx, unsigned __int16* y, int offy, int incy) { __max_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, max_inc_ip_u32)(int n, const unsigned __int32* x, int offx, int incx, unsigned __int32* y, int offy, int incy) { __max_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, max_inc_ip_u64)(int n, const unsigned __int64* x, int offx, int incx, unsigned __int64* y, int offy, int incy) { __max_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, max_inc_ip_f32)(int n, const float* x, int offx, int incx, float* y, int offy, int incy) { __max_inc_ip(n, x, offx, incx, y, offy, incy); }
GENIXAPI(void, max_inc_ip_f64)(int n, const double* x, int offx, int incx, double* y, int offy, int incy) { __max_inc_ip(n, x, offx, incx, y, offy, incy); }

// Returns the larger of each pair of elements of the two vector arguments not-in-place
template<typename T> void __forceinline ___max(
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
		y[i] = __max(a[i], b[i]);
	}
}

GENIXAPI(void, max_s8)(int n, const __int8* a, int offa, const __int8* b, int offb, __int8* y, int offy) { ___max(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, max_s16)(int n, const __int16* a, int offa, const __int16* b, int offb, __int16* y, int offy) { ___max(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, max_s32)(int n, const __int32* a, int offa, const __int32* b, int offb, __int32* y, int offy) { ___max(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, max_s64)(int n, const __int64* a, int offa, const __int64* b, int offb, __int64* y, int offy) { ___max(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, max_u8)(int n, const unsigned __int8* a, int offa, const unsigned __int8* b, int offb, unsigned __int8* y, int offy) { ___max(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, max_u16)(int n, const unsigned __int16* a, int offa, const unsigned __int16* b, int offb, unsigned __int16* y, int offy) { ___max(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, max_u32)(int n, const unsigned __int32* a, int offa, const unsigned __int32* b, int offb, unsigned __int32* y, int offy) { ___max(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, max_u64)(int n, const unsigned __int64* a, int offa, const unsigned __int64* b, int offb, unsigned __int64* y, int offy) { ___max(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, max_f32)(int n, const float* a, int offa, const float* b, int offb, float* y, int offy) { ___max(n, a, offa, b, offb, y, offy); }
GENIXAPI(void, max_f64)(int n, const double* a, int offa, const double* b, int offb, double* y, int offy) { ___max(n, a, offa, b, offb, y, offy); }

// Returns the larger of each pair of elements of the two vector arguments not-in-place with increment
template<typename T> void __forceinline __max_inc(
	int n,
	const T* a, int offa, int inca,
	const T* b, int offb, int incb,
	T* y, int offy, int incy)
{
	for (int i = 0; i < n; i++, offa += inca, offb += incb, offy += incy)
	{
		y[offy] = __max(a[offa], b[offb]);
	}
}

GENIXAPI(void, max_inc_s8)(int n, const __int8* a, int offa, int inca, const __int8* b, int offb, int incb, __int8* y, int offy, int incy) { __max_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, max_inc_s16)(int n, const __int16* a, int offa, int inca, const __int16* b, int offb, int incb, __int16* y, int offy, int incy) { __max_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, max_inc_s32)(int n, const __int32* a, int offa, int inca, const __int32* b, int offb, int incb, __int32* y, int offy, int incy) { __max_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, max_inc_s64)(int n, const __int64* a, int offa, int inca, const __int64* b, int offb, int incb, __int64* y, int offy, int incy) { __max_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, max_inc_u8)(int n, const unsigned __int8* a, int offa, int inca, const unsigned __int8* b, int offb, int incb, unsigned __int8* y, int offy, int incy) { __max_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, max_inc_u16)(int n, const unsigned __int16* a, int offa, int inca, const unsigned __int16* b, int offb, int incb, unsigned __int16* y, int offy, int incy) { __max_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, max_inc_u32)(int n, const unsigned __int32* a, int offa, int inca, const unsigned __int32* b, int offb, int incb, unsigned __int32* y, int offy, int incy) { __max_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, max_inc_u64)(int n, const unsigned __int64* a, int offa, int inca, const unsigned __int64* b, int offb, int incb, unsigned __int64* y, int offy, int incy) { __max_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, max_inc_f32)(int n, const float* a, int offa, int inca, const float* b, int offb, int incb, float* y, int offy, int incy) { __max_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }
GENIXAPI(void, max_inc_f64)(int n, const double* a, int offa, int inca, const double* b, int offb, int incb, double* y, int offy, int incy) { __max_inc(n, a, offa, inca, b, offb, incb, y, offy, incy); }

template<typename T> void __forceinline __minmax_gradient(
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
			dx[i] = (x[i] == y[i] ? (T)1.0 : (T)0.0) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += (x[i] == y[i] ? (T)1.0 : (T)0.0) * dy[i];
		}
	}
}
GENIXAPI(void, minmax_gradient_f32)(
	int n,
	const float* x, float* dx, int offx, BOOL cleardx,
	const float* y, const float* dy, int offy)
{
	__minmax_gradient(n, x, dx, offx, cleardx, y, dy, offy);
}
GENIXAPI(void, minmax_gradient_f64)(
	int n,
	const double* x, double* dx, int offx, BOOL cleardx,
	const double* y, const double* dy, int offy)
{
	__minmax_gradient(n, x, dx, offx, cleardx, y, dy, offy);
}

template<typename T> int __forceinline __argmin(int n, const T* x, int offx)
{
	x += offx;

	int win = 0;
	T min = x[0];

	for (int i = 1; i < n; i++)
	{
		const T value = x[i];
		if (value < min)
		{
			win = i;
			min = value;
		}
	}

	return offx + win;
}

template<typename T> int __forceinline __argmax(int n, const T* x, int offx)
{
	x += offx;

	int win = 0;
	T max = x[0];

	for (int i = 1; i < n; i++)
	{
		const T value = x[i];
		if (value > max)
		{
			win = i;
			max = value;
		}
	}

	return offx + win;
}

GENIXAPI(int, argmin_ip_s8s32)(int n, const __int8* x, int offx) { return __argmin(n, x, offx); }
GENIXAPI(int, argmin_ip_s16s32)(int n, const __int16* x, int offx) { return __argmin(n, x, offx); }
GENIXAPI(int, argmin_ip_s32)(int n, const __int32* x, int offx) { return __argmin(n, x, offx); }
GENIXAPI(int, argmin_ip_s64s32)(int n, const __int64* x, int offx) { return __argmin(n, x, offx); }
GENIXAPI(int, argmin_ip_u8s32)(int n, const unsigned __int8* x, int offx) { return __argmin(n, x, offx); }
GENIXAPI(int, argmin_ip_u16s32)(int n, const unsigned __int16* x, int offx) { return __argmin(n, x, offx); }
GENIXAPI(int, argmin_ip_u32s32)(int n, const unsigned __int32* x, int offx) { return __argmin(n, x, offx); }
GENIXAPI(int, argmin_ip_u64s32)(int n, const unsigned __int64* x, int offx) { return __argmin(n, x, offx); }
GENIXAPI(int, argmin_ip_f32s32)(int n, const float* x, int offx) { return __argmin(n, x, offx); }
GENIXAPI(int, argmin_ip_f64s32)(int n, const double* x, int offx) { return __argmin(n, x, offx); }

GENIXAPI(int, argmax_ip_s8s32)(int n, const __int8* x, int offx) { return __argmax(n, x, offx); }
GENIXAPI(int, argmax_ip_s16s32)(int n, const __int16* x, int offx) { return __argmax(n, x, offx); }
GENIXAPI(int, argmax_ip_s32)(int n, const __int32* x, int offx) { return __argmax(n, x, offx); }
GENIXAPI(int, argmax_ip_s64s32)(int n, const __int64* x, int offx) { return __argmax(n, x, offx); }
GENIXAPI(int, argmax_ip_u8s32)(int n, const unsigned __int8* x, int offx) { return __argmax(n, x, offx); }
GENIXAPI(int, argmax_ip_u16s32)(int n, const unsigned __int16* x, int offx) { return __argmax(n, x, offx); }
GENIXAPI(int, argmax_ip_u32s32)(int n, const unsigned __int32* x, int offx) { return __argmax(n, x, offx); }
GENIXAPI(int, argmax_ip_u64s32)(int n, const unsigned __int64* x, int offx) { return __argmax(n, x, offx); }
GENIXAPI(int, argmax_ip_f32s32)(int n, const float* x, int offx) { return __argmax(n, x, offx); }
GENIXAPI(int, argmax_ip_f64s32)(int n, const double* x, int offx) { return __argmax(n, x, offx); }

template<typename T> int __forceinline __argmin_inc(int n, const T* x, int offx, int incx)
{
	x += offx;

	int win = 0;
	T min = x[0];

	for (int i = incx; i < n; i += incx)
	{
		const T value = x[i];
		if (value < min)
		{
			win = i;
			min = value;
		}
	}

	return offx + win;
}

template<typename T> int __forceinline __argmax_inc(int n, const T* x, int offx, int incx)
{
	x += offx;

	int win = 0;
	T max = x[0];

	for (int i = incx; i < n; i += incx)
	{
		const T value = x[i];
		if (value > max)
		{
			win = i;
			max = value;
		}
	}

	return offx + win;
}

GENIXAPI(int, argmin_inc_ip_s8s32)(int n, const __int8* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
GENIXAPI(int, argmin_inc_ip_s16s32)(int n, const __int16* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
GENIXAPI(int, argmin_inc_ip_s32)(int n, const __int32* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
GENIXAPI(int, argmin_inc_ip_s64s32)(int n, const __int64* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
GENIXAPI(int, argmin_inc_ip_u8s32)(int n, const unsigned __int8* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
GENIXAPI(int, argmin_inc_ip_u16s32)(int n, const unsigned __int16* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
GENIXAPI(int, argmin_inc_ip_u32s32)(int n, const unsigned __int32* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
GENIXAPI(int, argmin_inc_ip_u64s32)(int n, const unsigned __int64* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
GENIXAPI(int, argmin_inc_ip_f32s32)(int n, const float* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
GENIXAPI(int, argmin_inc_ip_f64s32)(int n, const double* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }

GENIXAPI(int, argmax_inc_ip_s8s32)(int n, const __int8* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
GENIXAPI(int, argmax_inc_ip_s16s32)(int n, const __int16* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
GENIXAPI(int, argmax_inc_ip_s32)(int n, const __int32* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
GENIXAPI(int, argmax_inc_ip_s64s32)(int n, const __int64* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
GENIXAPI(int, argmax_inc_ip_u8s32)(int n, const unsigned __int8* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
GENIXAPI(int, argmax_inc_ip_u16s32)(int n, const unsigned __int16* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
GENIXAPI(int, argmax_inc_ip_u32s32)(int n, const unsigned __int32* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
GENIXAPI(int, argmax_inc_ip_u64s32)(int n, const unsigned __int64* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
GENIXAPI(int, argmax_inc_ip_f32s32)(int n, const float* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
GENIXAPI(int, argmax_inc_ip_f64s32)(int n, const double* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }

template<typename T> void __forceinline __argminmax(int n, const T* x, int offx, int& winmin, int& winmax)
{
	x += offx;

	winmin = winmax = 0;
	T min = x[0];
	T max = x[0];

	for (int i = 1; i < n; i++)
	{
		const T value = x[i];
		if (value < min)
		{
			winmin = i;
			min = value;
		}
		else if (value > max)
		{
			winmax = i;
			max = value;
		}
	}

	winmin += offx;
	winmax += offx;
}

GENIXAPI(void, argminmax_s8)(int n, const __int8* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
GENIXAPI(void, argminmax_s16)(int n, const __int16* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
GENIXAPI(void, argminmax_s32)(int n, const __int32* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
GENIXAPI(void, argminmax_s64)(int n, const __int64* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
GENIXAPI(void, argminmax_u8)(int n, const unsigned __int8* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
GENIXAPI(void, argminmax_u16)(int n, const unsigned __int16* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
GENIXAPI(void, argminmax_u32)(int n, const unsigned __int32* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
GENIXAPI(void, argminmax_u64)(int n, const unsigned __int64* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
GENIXAPI(void, argminmax_f32)(int n, const float* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
GENIXAPI(void, argminmax_f64)(int n, const double* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }

template<typename T> T __forceinline ___min(int n, const T* x, int offx) { return x[__argmin(n, x, offx)]; }
template<typename T> T __forceinline ___max(int n, const T* x, int offx) { return x[__argmax(n, x, offx)]; }

GENIXAPI(__int8, _min_ip_s8)(int n, const __int8* x, int offx) { return ___min(n, x, offx); }
GENIXAPI(__int16, _min_ip_s16)(int n, const __int16* x, int offx) { return ___min(n, x, offx); }
GENIXAPI(__int32, _min_ip_s32)(int n, const __int32* x, int offx) { return ___min(n, x, offx); }
GENIXAPI(__int64, _min_ip_s64)(int n, const __int64* x, int offx) { return ___min(n, x, offx); }
GENIXAPI(unsigned __int8, _min_ip_u8)(int n, const unsigned __int8* x, int offx) { return ___min(n, x, offx); }
GENIXAPI(unsigned __int16, _min_ip_u16)(int n, const unsigned __int16* x, int offx) { return ___min(n, x, offx); }
GENIXAPI(unsigned __int32, _min_ip_u32)(int n, const unsigned __int32* x, int offx) { return ___min(n, x, offx); }
GENIXAPI(unsigned __int64, _min_ip_u64)(int n, const unsigned __int64* x, int offx) { return ___min(n, x, offx); }
GENIXAPI(float, _min_ip_f32)(int n, const float* x, int offx) { return ___min(n, x, offx); }
GENIXAPI(double, _min_ip_f64)(int n, const double* x, int offx) { return ___min(n, x, offx); }

GENIXAPI(__int8, _max_ip_s8)(int n, const __int8* x, int offx) { return ___max(n, x, offx); }
GENIXAPI(__int16, _max_ip_s16)(int n, const __int16* x, int offx) { return ___max(n, x, offx); }
GENIXAPI(__int32, _max_ip_s32)(int n, const __int32* x, int offx) { return ___max(n, x, offx); }
GENIXAPI(__int64, _max_ip_s64)(int n, const __int64* x, int offx) { return ___max(n, x, offx); }
GENIXAPI(unsigned __int8, _max_ip_u8)(int n, const unsigned __int8* x, int offx) { return ___max(n, x, offx); }
GENIXAPI(unsigned __int16, _max_ip_u16)(int n, const unsigned __int16* x, int offx) { return ___max(n, x, offx); }
GENIXAPI(unsigned __int32, _max_ip_u32)(int n, const unsigned __int32* x, int offx) { return ___max(n, x, offx); }
GENIXAPI(unsigned __int64, _max_ip_u64)(int n, const unsigned __int64* x, int offx) { return ___max(n, x, offx); }
GENIXAPI(float, _max_ip_f32)(int n, const float* x, int offx) { return ___max(n, x, offx); }
GENIXAPI(double, _max_ip_f64)(int n, const double* x, int offx) { return ___max(n, x, offx); }

template<typename T> T __forceinline ___min_inc(int n, const T* x, int offx, int incx) { return x[__argmin_inc(n, x, offx, incx)]; }
template<typename T> T __forceinline ___max_inc(int n, const T* x, int offx, int incx) { return x[__argmax_inc(n, x, offx, incx)]; }

GENIXAPI(__int8, _min_inc_ip_s8)(int n, const __int8* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
GENIXAPI(__int16, _min_inc_ip_s16)(int n, const __int16* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
GENIXAPI(__int32, _min_inc_ip_s32)(int n, const __int32* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
GENIXAPI(__int64, _min_inc_ip_s64)(int n, const __int64* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
GENIXAPI(unsigned __int8, _min_inc_ip_u8)(int n, const unsigned __int8* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
GENIXAPI(unsigned __int16, _min_inc_ip_u16)(int n, const unsigned __int16* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
GENIXAPI(unsigned __int32, _min_inc_ip_u32)(int n, const unsigned __int32* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
GENIXAPI(unsigned __int64, _min_inc_ip_u64)(int n, const unsigned __int64* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
GENIXAPI(float, _min_inc_ip_f32)(int n, const float* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
GENIXAPI(double, _min_inc_ip_f64)(int n, const double* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }

GENIXAPI(__int8, _max_inc_ip_s8)(int n, const __int8* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
GENIXAPI(__int16, _max_inc_ip_s16)(int n, const __int16* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
GENIXAPI(__int32, _max_inc_ip_s32)(int n, const __int32* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
GENIXAPI(__int64, _max_inc_ip_s64)(int n, const __int64* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
GENIXAPI(unsigned __int8, _max_inc_ip_u8)(int n, const unsigned __int8* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
GENIXAPI(unsigned __int16, _max_inc_ip_u16)(int n, const unsigned __int16* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
GENIXAPI(unsigned __int32, _max_inc_ip_u32)(int n, const unsigned __int32* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
GENIXAPI(unsigned __int64, _max_inc_ip_u64)(int n, const unsigned __int64* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
GENIXAPI(float, _max_inc_ip_f32)(int n, const float* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
GENIXAPI(double, _max_inc_ip_f64)(int n, const double* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }

// Softmax
template<typename T> void __forceinline __softmax(int n, const T* x, int offx, T* y, int offy)
{
	x += offx;
	y += offy;

	// compute max activation
	T amax = ___max(n, x, 0);

	// compute exponentials (carefully to not blow up)
	T esum = (T)0;
	for (int i = 0; i < n; i++)
	{
		y[i] = exp(x[i] - amax);
		esum += y[i];
	}

	// normalize and output to sum to one
	if (esum != 0)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] /= esum;
		}
	}
}

GENIXAPI(void, softmax_ip_f32)(int n, float* y, int offy) { __softmax(n, y, offy, y, offy); }
GENIXAPI(void, softmax_ip_f64)(int n, double* y, int offy) { __softmax(n, y, offy, y, offy); }
GENIXAPI(void, softmax_f32)(int n, const float* x, int offx, float* y, int offy) { __softmax(n, x, offx, y, offy); }
GENIXAPI(void, softmax_f64)(int n, const double* x, int offx, double* y, int offy) { __softmax(n, x, offx, y, offy); }

template<typename T> void __forceinline __softmax_batch(int n, int batchlen, const T* x, int offx, T* y, int offy)
{
	for (int i = 0; i < n; i += batchlen, offx += batchlen, offy += batchlen)
	{
		__softmax(batchlen, x, offx, y, offy);
	}
}

GENIXAPI(void, softmax_batch_ip_f32)(int n, int batchlen, float* y, int offy) { __softmax_batch(n, batchlen, y, offy, y, offy); }
GENIXAPI(void, softmax_batch_ip_f64)(int n, int batchlen, double* y, int offy) { __softmax_batch(n, batchlen, y, offy, y, offy); }
GENIXAPI(void, softmax_batch_f32)(int n, const float* x, int offx, int batchlen, float* y, int offy) { __softmax_batch(n, batchlen, x, offx, y, offy); }
GENIXAPI(void, softmax_batch_f64)(int n, const double* x, int offx, int batchlen, double* y, int offy) { __softmax_batch(n, batchlen, x, offx, y, offy); }
