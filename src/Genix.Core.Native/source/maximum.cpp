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

GENIXAPI(void, minc_ip_s32)(int n, __int32 a, __int32* y, int offy) { __minc_ip(n, a, y, offy); }
GENIXAPI(void, minc_ip_s64)(int n, __int64 a, __int64* y, int offy) { __minc_ip(n, a, y, offy); }
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

GENIXAPI(void, minc_s32)(int n, const __int32* x, int offx, __int32 a, __int32* y, int offy) { __minc(n, x, offx, a, y, offy); }
GENIXAPI(void, minc_s64)(int n, const __int64* x, int offx, __int64 a, __int64* y, int offy) { __minc(n, x, offx, a, y, offy); }
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

GENIXAPI(void, maxc_ip_s32)(int n, __int32 a, __int32* y, int offy) { __maxc_ip(n, a, y, offy); }
GENIXAPI(void, maxc_ip_s64)(int n, __int64 a, __int64* y, int offy) { __maxc_ip(n, a, y, offy); }
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

GENIXAPI(void, maxc_s32)(int n, const __int32* x, int offx, __int32 a, __int32* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_s64)(int n, const __int64* x, int offx, __int64 a, __int64* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32 a, unsigned __int32* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64 a, unsigned __int64* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_f32)(int n, const float* x, int offx, float a, float* y, int offy) { __maxc(n, x, offx, a, y, offy); }
GENIXAPI(void, maxc_f64)(int n, const double* x, int offx, double a, double* y, int offy) { __maxc(n, x, offx, a, y, offy); }

// Returns the smaller of each pair of elements of the two vector arguments.
extern "C" __declspec(dllexport) void WINAPI smin(
	int n,
	const float* a, int offa,
	float* b, int offb,
	float* y, int offy)
{
	::vsFmin(n, a + offa, b + offb, y + offy);
}

// Returns the larger of each pair of elements of the two vector arguments.
extern "C" __declspec(dllexport) void WINAPI smax(
	int n,
	const float* a, int offa,
	float* b, int offb,
	float* y, int offy)
{
	::vsFmax(n, a + offa, b + offb, y + offy);
}

extern "C" __declspec(dllexport) void WINAPI sminmax_gradient(
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

extern "C" __declspec(dllexport) int WINAPI i32argmin(int n, const int* x, int offx)
{
	return __argmin<int>(n, x, offx);
}

extern "C" __declspec(dllexport) int WINAPI i32argmax(int n, const int* x, int offx)
{
	return __argmax<int>(n, x, offx);
}

extern "C" __declspec(dllexport) int WINAPI sargmin(int n, const float* x, int offx)
{
	return __argmin<float>(n, x, offx);
}

extern "C" __declspec(dllexport) int WINAPI sargmax(int n, const float* x, int offx)
{
	return __argmax<float>(n, x, offx);
}

extern "C" __declspec(dllexport) void WINAPI sargminmax(int n, const float* x, int offx, int& winmin, int& winmax)
{
	x += offx;

	winmin = winmax = 0;
	float min = x[0];
	float max = x[0];

	for (int i = 1; i < n; i++)
	{
		const float value = x[i];
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
