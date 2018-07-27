#include "stdafx.h"
#include "mkl.h"

GENIXAPI(float, _sdot)(
	int n,
	const float* x, int offx, int incx,
	const float* y, int offy, int incy)
{
	x += offx;
	y += offy;

	if (n <= 32)
	{
		float res = 0.0f;

		if (incx == 1 && incy == 1)
		{
			for (int i = 0; i < n; i++)
			{
				res += x[i] * y[i];
			}
		}
		else
		{
			for (int i = 0, xi = 0, yi = 0; i < n; i++, xi += incx, yi += incy)
			{
				res += x[xi] * y[yi];
			}
		}

		return res;
	}
	else
	{
		return ::cblas_sdot(n, x, incx, y, incy);
	}
}

GENIXAPI(void, matrix_vv)(
	BOOL rowmajor,
	int m, int n,
	const float* x, int offx,
	const float* y, int offy,
	float* a, int offa)
{
	const int lda = rowmajor ? n : m;

	::cblas_sger(
		rowmajor ? CblasRowMajor : CblasColMajor,
		m,
		n,
		1.0f,
		x + offx,
		1,
		y + offy,
		1,
		a + offa,
		lda);
}

GENIXAPI(void, matrix_mv)(
	BOOL rowmajor,
	int m, int n,
	const float* a, int offa, BOOL transa,
	const float* x, int offx,
	float* y, int offy, BOOL cleary)
{
	const int lda = rowmajor ? n : m;

	::cblas_sgemv(
		rowmajor ? CblasRowMajor : CblasColMajor,
		transa ? CblasTrans : CblasNoTrans,
		m,
		n,
		1.0f,
		a + offa,
		lda,
		x + offx,
		1,
		cleary ? 0.0f : 1.0f,
		y + offy,
		1);
}

GENIXAPI(void, matrix_mm)(
	BOOL rowmajor,
	int m, int k, int n,
	const float* a, int offa, BOOL transa,
	const float* b, int offb, BOOL transb,
	float* c, int offc, BOOL clearc)
{
	const int lda = rowmajor ? (transa ? m : k) : (transa ? k : m);
	const int ldb = rowmajor ? (transb ? k : n) : (transb ? n : k);
	const int ldc = rowmajor ? n : m;

	::cblas_sgemm(
		rowmajor ? CblasRowMajor : CblasColMajor,
		transa ? CblasTrans : CblasNoTrans,
		transb ? CblasTrans : CblasNoTrans,
		m,
		n,
		k,
		1.0f,
		a + offa,
		lda,
		b + offb,
		ldb,
		clearc ? 0.0f : 1.0f,
		c + offc,
		ldc);
}

GENIXAPI(void, matrix_transpose)(
	BOOL rowmajor,
	int rows, int cols,
	float* ab, int offab)
{
	::mkl_simatcopy(
		rowmajor ? 'r' : 'c',
		't',
		rows,
		cols,
		1.0f,
		ab + offab,
		rowmajor ? cols : rows,
		rowmajor ? rows : cols);
}
