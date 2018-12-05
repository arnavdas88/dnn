#include "stdafx.h"
#include <stdlib.h>
#include "mkl.h"

#include <ppl.h>
using namespace concurrency;

GENIXAPI(void, convolution)(
	const int ksize1,
	const int ksize2,
	const int kstride1,
	const int kstride2,
	const int kpadding1,
	const int kpadding2,
	const float* ww,
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

	if (kpadding1 == 0 && kpadding2 == 0)
	{
		const int ldw = y3;		// number of channels
		const int ldx = xstride1;
		const int ldy = ystride1;
		const int kstep = ksize2 * xstride2 * ldw;

		//for (int iy2 = 0; iy2 < y2; iy2++) {
		parallel_for(0, y2, [&](int iy2) {

			const int ix2 = iy2 * kstride2;
			const int ix2e = min(ix2 + ksize2, x2);
			const int k = (ix2e - ix2) * xstride2;
			const int m = y3;

			int wpos = 0;
			int xpos = ix2 * xstride2;
			const int ypos = iy2 * ystride2;

			for (int ixy1 = 0; ixy1 < ksize1; ixy1++, wpos += kstep, xpos += xstride1)
			{
				const int n = min(y1, (x1 - ixy1 + 1) / kstride1);

				::cblas_sgemm(
					CblasColMajor,
					CblasNoTrans,
					CblasNoTrans,
					m,
					n,
					k,
					1.0f,
					ww + wpos,
					ldw,
					xw + xpos,
					ldx,
					ixy1 == 0 ? 0.0f : 1.0f,
					yw + ypos,
					ldy);
			}
		});
	}
}
