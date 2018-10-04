#include "stdafx.h"
#include <cmath>
#include <intrin.h>

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

// Hamming distance
unsigned __int32 __forceinline popcnt32(unsigned __int32 value)
{
	return __popcnt(value);
}
unsigned __int64 __forceinline popcnt64(unsigned __int64 value)
{
#ifndef _WIN64
	return (unsigned __int64)(__popcnt(unsigned(value))) + __popcnt((unsigned)(value >> 32));
#else
	return __popcnt64(value);
#endif
}

template<typename T, T popcnt(T)> T __forceinline __hamming_distance(
	const int n,
	const T* x, const int offx,
	const T* y, const int offy)
{
	x += offx;
	y += offy;

	T sum = T(0);
	for (int i = 0; i < n; i++)
	{
		sum += popcnt(x[i] ^ y[i]);
	}

	return sum;
}

GENIXAPI(unsigned __int32, hamming_distance_ip_u32)(const int n, const unsigned __int32* x, int offx, const unsigned __int32* y, int offy)
{
	return __hamming_distance<unsigned __int32, popcnt32>(n, x, offx, y, offy);
}
GENIXAPI(unsigned __int64, hamming_distance_ip_u64)(const int n, const unsigned __int64* x, int offx, const unsigned __int64* y, int offy)
{
	return __hamming_distance<unsigned __int64, popcnt64>(n, x, offx, y, offy);
}
