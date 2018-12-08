#include "stdafx.h"
#include <stdlib.h>
#include "mkl.h"

#include <ppl.h>
using namespace concurrency;

void __forceinline tile(const int count, const int length, const float* src, float* dst, const int dststep)
{
	for (int i = 0; i < count; i++, dst += dststep)
	{
		memcpy(dst, src, length * sizeof(float));
	}
}

int __forceinline output_size_floor(const int size, const int ksize, const int kstride, const int kpadding)
{
	return (max(size - ksize + (2 * kpadding), 0) / kstride) + 1;
}

int __forceinline divide_ceiling(const int value, const int divisor)
{
	return (value + divisor - 1) / divisor;
}

int __forceinline output_size_ceiling(const int size, const int ksize, const int kstride, const int kpadding)
{
	return divide_ceiling(max(size - ksize + (2 * kpadding), 0), kstride) + 1;
}

void __forceinline matmul(
	const int m, const int n, const int k,
	const float* A, const int lda,
	const float* B, const int ldb,
	float* C, const int ldc)
{
	::cblas_sgemm(
		CblasColMajor,
		CblasNoTrans,
		CblasNoTrans,
		m,
		n,
		k,
		1.0f,
		A,
		lda,
		B,
		ldb,
		1.0f,
		C,
		ldc);
}

GENIXAPI(void, convolution)(
	const int ksize1,
	const int ksize2,
	const int kstride1,
	const int kstride2,
	const int kpadding1l,
	const int kpadding1r,
	const int kpadding2l,
	const int kpadding2r,
	const float* ww,
	const float* bw,
	const int* waxes,
	const int* wstrides,
	const float* xw,
	const int* xaxes,
	const int* xstrides,
	float* yw,
	const int* yaxes,
	const int* ystrides)
{
	const int w0 = waxes[0];
	const int w1 = waxes[1];
	const int wstride0 = wstrides[0];
	const int wstride1 = wstrides[1];

	const int x0 = xaxes[0];
	const int x1 = xaxes[1];
	const int x2 = xaxes[2];
	const int xstride0 = xstrides[0];
	const int xstride1 = xstrides[1];
	const int xstride2 = xstrides[2];

	const int y0 = yaxes[0];
	const int y1 = yaxes[1];
	const int y2 = yaxes[2];
	const int y3 = yaxes[3];	// number of channels
	const int ystride0 = ystrides[0];
	const int ystride1 = ystrides[1];
	const int ystride2 = ystrides[2];

	const int ldw = y3;
	const int ldx = kstride1 * xstride1;
	const int ldy = ystride1;
	const int kstep = ksize2 * xstride2 * ldw;

	// if kpadding1 or kpadding2 are negative we need to shrink working area of x tensor
	if (kpadding1l < 0)
	{
		xw += -ptrdiff_t(kpadding1l) * xstride1;
	}

	if (kpadding2l < 0)
	{
		xw += -ptrdiff_t(kpadding2l) * xstride2;
	}

	const int x1e = x1 + min(kpadding1l, 0) + min(kpadding1r, 0);
	const int x2e = x2 + min(kpadding2l, 0) + min(kpadding2r, 0);

	for (int iy2 = 0; iy2 < y2; iy2++) {
		//parallel_for(0, y2, [&](int iy2) {

		const float* www = ww;
		const float* xww = xw;
		float* yww = yw + (ptrdiff_t(iy2) * ystride2);

		// 1. initialize destination tensor with biases
		tile(y0 * y1, y3, bw, yww, ldy);

		// 2. add matrix product to destination
		const int ix2 = (iy2 * kstride2) - max(kpadding2l, 0);
		const int ix2e = min(ix2 + ksize2, x2e);
		const int k = (ix2e - max(ix2, 0)) * xstride2;

		// k may be zero if the current portion of input tensor
		// is completely in the padding area
		if (k > 0)
		{
			xww += ptrdiff_t(max(ix2, 0)) * xstride2;
			www += ptrdiff_t(max(-ix2, 0)) * xstride2 * ldw;

			for (int ixy0 = 0; ixy0 < x0; ixy0++, xww += xstride0, yww += ystride0)
			{
				for (int ixy1 = 0; ixy1 < ksize1; ixy1++)
				{
					int ix1 = ixy1 - max(kpadding1l, 0);
					int iy1 = 0;
					int n = min(y1, ((x1e - ix1 - 1) / kstride1 + 1));

					while (ix1 < 0)
					{
						ix1 += kstride1;
						n--;
						iy1++;
					}

					matmul(
						y3, n, k,
						www + (ptrdiff_t(ixy1) * kstep), ldw,
						xww + (ptrdiff_t(ix1) * xstride1), ldx,
						yww + (ptrdiff_t(iy1) * ystride1), ldy);
				}
			}
		}
	}/*);*/
}
