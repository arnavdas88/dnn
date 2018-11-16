#include "stdafx.h"

#include "C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v10.0\include\nvblas.h"

GENIXAPI(void, cuda_matrix_mm)(
	int m, int k, int n,
	const float* a, int offa, BOOL transa,
	const float* b, int offb, BOOL transb,
	float* c, int offc, BOOL clearc)
{
	const int lda = transa ? k : m;
	const int ldb = transb ? n : k;
	const int ldc = m;

	const float alpha = 1.0f;
	const float beta = clearc ? 0.0f : 1.0f;

	::sgemm(
		transa ? "T" : "N",
		transb ? "T" : "N",
		&m,
		&n,
		&k,
		&alpha,
		a + offa,
		&lda,
		b + offb,
		&ldb,
		&beta,
		c + offc,
		&ldc);
}
