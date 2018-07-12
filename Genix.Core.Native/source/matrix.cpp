#include "stdafx.h"
#include "mkl.h"

extern "C" __declspec(dllexport) void WINAPI matrix_vv(
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

extern "C" __declspec(dllexport) void WINAPI matrix_mv(
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

extern "C" __declspec(dllexport) void WINAPI matrix_mm(
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

extern "C" __declspec(dllexport) void WINAPI matrix_transpose(
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
