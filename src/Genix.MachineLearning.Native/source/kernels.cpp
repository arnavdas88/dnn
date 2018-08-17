#include "stdafx.h"

template<typename T> T __forceinline __chisquare(
	const int n,
	const T* x, const int offx,
	const T* y, const int offy,
	const T eps)
{
	x += offx;
	y += offy;

	T sum = T(0);
	for (int i = 0; i < n; i++)
	{
		T num = x[i] - y[i];
		sum += (num * num) / (x[i] + y[i] + eps);
	}

	return T(1) - (T(2) * sum);
}

GENIXAPI(float, chisquare_f32)(int n, const float* x, int offx, const float* y, int offy)
{
	return __chisquare(n, x, offx, y, offy, 1e-10f);
}
GENIXAPI(double, chisquare_f64)(int n, const double* x, int offx, const double* y, int offy)
{
	return __chisquare(n, x, offx, y, offy, 1e-10);
}

template<typename T> T __forceinline __sparse_chisquare(
	const int n,
	const int* xidx, const T* x,
	const T* y, const int offy,
	const T eps)
{
	y += offy;

	T sum = T(0);
	for (int i = 0; i < n; i++)
	{
		T num = x[i] - y[xidx[i]];
		sum += (num * num) / (x[i] + y[xidx[i]] + eps);
	}

	return T(1) - (T(2) * sum);
}

GENIXAPI(float, sparse_chisquare_f32)(int n, const int* xidx, const float* x, const float* y, int offy)
{
	return __sparse_chisquare(n, xidx, x, y, offy, 1e-10f);
}
GENIXAPI(double, sparse_chisquare_f64)(int n, const int* xidx, const double* x, const double* y, int offy)
{
	return __sparse_chisquare(n, xidx, x, y, offy, 1e-10);
}
