#include "stdafx.h"
#include "mkl.h"

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

extern "C" __declspec(dllexport) void WINAPI minmax_gradient_f32(
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
			dx[i] = (x[i] == y[i] ? 1.0f : 0.0f) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += (x[i] == y[i] ? 1.0f : 0.0f) * dy[i];
		}
	}
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

extern "C" __declspec(dllexport) int WINAPI argmin_ip_s8(int n, const __int8* x, int offx) { return __argmin(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmin_ip_s16(int n, const __int16* x, int offx) { return __argmin(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmin_ip_s32(int n, const __int32* x, int offx) { return __argmin(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmin_ip_s64(int n, const __int64* x, int offx) { return __argmin(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmin_ip_u8(int n, const unsigned __int8* x, int offx) { return __argmin(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmin_ip_u16(int n, const unsigned __int16* x, int offx) { return __argmin(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmin_ip_u32(int n, const unsigned __int32* x, int offx) { return __argmin(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmin_ip_u64(int n, const unsigned __int64* x, int offx) { return __argmin(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmin_ip_f32(int n, const float* x, int offx) { return __argmin(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmin_ip_f64(int n, const double* x, int offx) { return __argmin(n, x, offx); }

extern "C" __declspec(dllexport) int WINAPI argmax_ip_s8(int n, const __int8* x, int offx) { return __argmax(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmax_ip_s16(int n, const __int16* x, int offx) { return __argmax(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmax_ip_s32(int n, const __int32* x, int offx) { return __argmax(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmax_ip_s64(int n, const __int64* x, int offx) { return __argmax(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmax_ip_u8(int n, const unsigned __int8* x, int offx) { return __argmax(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmax_ip_u16(int n, const unsigned __int16* x, int offx) { return __argmax(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmax_ip_u32(int n, const unsigned __int32* x, int offx) { return __argmax(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmax_ip_u64(int n, const unsigned __int64* x, int offx) { return __argmax(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmax_ip_f32(int n, const float* x, int offx) { return __argmax(n, x, offx); }
extern "C" __declspec(dllexport) int WINAPI argmax_ip_f64(int n, const double* x, int offx) { return __argmax(n, x, offx); }

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

extern "C" __declspec(dllexport) int WINAPI argmin_inc_ip_s8(int n, const __int8* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmin_inc_ip_s16(int n, const __int16* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmin_inc_ip_s32(int n, const __int32* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmin_inc_ip_s64(int n, const __int64* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmin_inc_ip_u8(int n, const unsigned __int8* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmin_inc_ip_u16(int n, const unsigned __int16* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmin_inc_ip_u32(int n, const unsigned __int32* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmin_inc_ip_u64(int n, const unsigned __int64* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmin_inc_ip_f32(int n, const float* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmin_inc_ip_f64(int n, const double* x, int offx, int incx) { return __argmin_inc(n, x, offx, incx); }

extern "C" __declspec(dllexport) int WINAPI argmax_inc_ip_s8(int n, const __int8* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmax_inc_ip_s16(int n, const __int16* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmax_inc_ip_s32(int n, const __int32* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmax_inc_ip_s64(int n, const __int64* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmax_inc_ip_u8(int n, const unsigned __int8* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmax_inc_ip_u16(int n, const unsigned __int16* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmax_inc_ip_u32(int n, const unsigned __int32* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmax_inc_ip_u64(int n, const unsigned __int64* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmax_inc_ip_f32(int n, const float* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) int WINAPI argmax_inc_ip_f64(int n, const double* x, int offx, int incx) { return __argmax_inc(n, x, offx, incx); }

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

extern "C" __declspec(dllexport) void WINAPI argminmax_s8(int n, const __int8* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
extern "C" __declspec(dllexport) void WINAPI argminmax_s16(int n, const __int16* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
extern "C" __declspec(dllexport) void WINAPI argminmax_s32(int n, const __int32* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
extern "C" __declspec(dllexport) void WINAPI argminmax_s64(int n, const __int64* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
extern "C" __declspec(dllexport) void WINAPI argminmax_u8(int n, const unsigned __int8* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
extern "C" __declspec(dllexport) void WINAPI argminmax_u16(int n, const unsigned __int16* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
extern "C" __declspec(dllexport) void WINAPI argminmax_u32(int n, const unsigned __int32* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
extern "C" __declspec(dllexport) void WINAPI argminmax_u64(int n, const unsigned __int64* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
extern "C" __declspec(dllexport) void WINAPI argminmax_f32(int n, const float* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }
extern "C" __declspec(dllexport) void WINAPI argminmax_f64(int n, const double* x, int offx, int& winmin, int& winmax) { return __argminmax(n, x, offx, winmin, winmax); }

template<typename T> T __forceinline ___min(int n, const T* x, int offx) { return x[__argmin(n, x, offx)]; }
template<typename T> T __forceinline ___max(int n, const T* x, int offx) { return x[__argmax(n, x, offx)]; }

extern "C" __declspec(dllexport) __int8  WINAPI _min_ip_s8(int n, const __int8* x, int offx) { return ___min(n, x, offx); }
extern "C" __declspec(dllexport) __int16 WINAPI _min_ip_s16(int n, const __int16* x, int offx) { return ___min(n, x, offx); }
extern "C" __declspec(dllexport) __int32 WINAPI _min_ip_s32(int n, const __int32* x, int offx) { return ___min(n, x, offx); }
extern "C" __declspec(dllexport) __int64 WINAPI _min_ip_s64(int n, const __int64* x, int offx) { return ___min(n, x, offx); }
extern "C" __declspec(dllexport) unsigned __int8  WINAPI _min_ip_u8(int n, const unsigned __int8* x, int offx) { return ___min(n, x, offx); }
extern "C" __declspec(dllexport) unsigned __int16 WINAPI _min_ip_u16(int n, const unsigned __int16* x, int offx) { return ___min(n, x, offx); }
extern "C" __declspec(dllexport) unsigned __int32 WINAPI _min_ip_u32(int n, const unsigned __int32* x, int offx) { return ___min(n, x, offx); }
extern "C" __declspec(dllexport) unsigned __int64 WINAPI _min_ip_u64(int n, const unsigned __int64* x, int offx) { return ___min(n, x, offx); }
extern "C" __declspec(dllexport) float WINAPI _min_ip_f32(int n, const float* x, int offx) { return ___min(n, x, offx); }
extern "C" __declspec(dllexport) double WINAPI _min_ip_f64(int n, const double* x, int offx) { return ___min(n, x, offx); }

extern "C" __declspec(dllexport) __int8  WINAPI _max_ip_s8(int n, const __int8* x, int offx) { return ___max(n, x, offx); }
extern "C" __declspec(dllexport) __int16 WINAPI _max_ip_s16(int n, const __int16* x, int offx) { return ___max(n, x, offx); }
extern "C" __declspec(dllexport) __int32 WINAPI _max_ip_s32(int n, const __int32* x, int offx) { return ___max(n, x, offx); }
extern "C" __declspec(dllexport) __int64 WINAPI _max_ip_s64(int n, const __int64* x, int offx) { return ___max(n, x, offx); }
extern "C" __declspec(dllexport) unsigned __int8  WINAPI _max_ip_u8(int n, const unsigned __int8* x, int offx) { return ___max(n, x, offx); }
extern "C" __declspec(dllexport) unsigned __int16 WINAPI _max_ip_u16(int n, const unsigned __int16* x, int offx) { return ___max(n, x, offx); }
extern "C" __declspec(dllexport) unsigned __int32 WINAPI _max_ip_u32(int n, const unsigned __int32* x, int offx) { return ___max(n, x, offx); }
extern "C" __declspec(dllexport) unsigned __int64 WINAPI _max_ip_u64(int n, const unsigned __int64* x, int offx) { return ___max(n, x, offx); }
extern "C" __declspec(dllexport) float WINAPI _max_ip_f32(int n, const float* x, int offx) { return ___max(n, x, offx); }
extern "C" __declspec(dllexport) double WINAPI _max_ip_f64(int n, const double* x, int offx) { return ___max(n, x, offx); }

template<typename T> T __forceinline ___min_inc(int n, const T* x, int offx, int incx) { return x[__argmin_inc(n, x, offx, incx)]; }
template<typename T> T __forceinline ___max_inc(int n, const T* x, int offx, int incx) { return x[__argmax_inc(n, x, offx, incx)]; }

extern "C" __declspec(dllexport) __int8  WINAPI _min_inc_ip_s8(int n, const __int8* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) __int16 WINAPI _min_inc_ip_s16(int n, const __int16* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) __int32 WINAPI _min_inc_ip_s32(int n, const __int32* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) __int64 WINAPI _min_inc_ip_s64(int n, const __int64* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) unsigned __int8  WINAPI _min_inc_ip_u8(int n, const unsigned __int8* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) unsigned __int16 WINAPI _min_inc_ip_u16(int n, const unsigned __int16* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) unsigned __int32 WINAPI _min_inc_ip_u32(int n, const unsigned __int32* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) unsigned __int64 WINAPI _min_inc_ip_u64(int n, const unsigned __int64* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) float WINAPI _min_inc_ip_f32(int n, const float* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) double WINAPI _min_inc_ip_f64(int n, const double* x, int offx, int incx) { return ___min_inc(n, x, offx, incx); }

extern "C" __declspec(dllexport) __int8  WINAPI _max_inc_ip_s8(int n, const __int8* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) __int16 WINAPI _max_inc_ip_s16(int n, const __int16* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) __int32 WINAPI _max_inc_ip_s32(int n, const __int32* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) __int64 WINAPI _max_inc_ip_s64(int n, const __int64* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) unsigned __int8  WINAPI _max_inc_ip_u8(int n, const unsigned __int8* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) unsigned __int16 WINAPI _max_inc_ip_u16(int n, const unsigned __int16* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) unsigned __int32 WINAPI _max_inc_ip_u32(int n, const unsigned __int32* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) unsigned __int64 WINAPI _max_inc_ip_u64(int n, const unsigned __int64* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) float WINAPI _max_inc_ip_f32(int n, const float* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }
extern "C" __declspec(dllexport) double WINAPI _max_inc_ip_f64(int n, const double* x, int offx, int incx) { return ___max_inc(n, x, offx, incx); }



