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

int __forceinline kernel_output_size(const int size, const int ksize, const int kstride, const int kpadding)
{
	return ((max(size - ksize + (2 * kpadding), 0) + kstride - 1) / kstride) + 1;
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
	const int kpadding1,
	const int kpadding2,
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
	const int y3 = yaxes[3];
	const int ystride0 = ystrides[0];
	const int ystride1 = ystrides[1];
	const int ystride2 = ystrides[2];

	const int ldw = y3;		// number of channels
	const int ldx = xstride1;
	const int ldy = ystride1;
	const int kstep = ksize2 * xstride2 * ldw;

	//for (int iy2 = 0; iy2 < y2; iy2++) {
	parallel_for(0, y2, [&](int iy2) {

		float* yww = yw + (ptrdiff_t(iy2) * ystride2);

		// 1. initialize destination tensor with biases
		tile(y0 * y1, y3, bw, yww, ldy);

		// 2. add matrix product to destination
		// k may be zero if the current portion of input tensor
		// is completely in the padding area
		const int ix2 = (iy2 * kstride2) - kpadding2;
		const int ix2e = min(ix2 + ksize2, x2);
		if (ix2e > max(ix2, 0))
		{
			const int k = (ix2e - max(ix2, 0)) * xstride2;
			const int m = y3;

			int wpos = max(-ix2, 0) * xstride2 * ldw;
			int xpos = max(ix2, 0) * xstride2;

			for (int ixy1 = 0; ixy1 < ksize1; ixy1++, wpos += kstep, xpos += xstride1)
			{
				const int n = y0 * y1; //// min(y1, (x1 - ixy1 + 1) / kstride1);

				/*int ix1 = ixy1 - kpadding1;
				const int ix1e = min(x1, ix1 + ((y1 - 1) * kstride1) + ksize1);

				int xposaddon = 0;
				int yposaddon = 0;
				while (ix1 < 0)
				{
					ix1 += kstride1;
					xposaddon += xstride1 * kstride1;
					yposaddon += ystride1;
				}

				const int n = min(y1, kernel_output_size(ix1e - ix1, ksize1, kstride1, 0));*/

				matmul(m, n, k, ww + wpos, ldw, xw + xpos/* + xposaddon*/, ldx, yww/* + yposaddon*/, ldy);
			}
		}
	});
}
