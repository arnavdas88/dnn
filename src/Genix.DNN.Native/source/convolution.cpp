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

GENIXAPI(void, convolution)(
	const int ksize1,
	const int ksize2,
	const int kstride1,
	const int kstride2,
	int kpadding1,
	int kpadding2,
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
	int x1 = xaxes[1];
	int x2 = xaxes[2];
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

	/*if (kstride1 == 1 && kstride2 == 1 && kpadding1 == 0 && kpadding2 == 0)
	{
		// 1. initialize destination tensor with biases
		tile(y0 * y1 * y2, y3, bw, yw, y3);

		const int k = ksize2 * xstride2;
		const int n = y1;

		const int group_size = y2 * y0;
		const float** A_Array = (const float**)::alloca(group_size * sizeof(const float*));
		const float** B_Array = (const float**)::alloca(group_size * sizeof(const float*));
		float** C_Array = (float**)::alloca(group_size * sizeof(const float*));

		for (int ixy1 = 0; ixy1 < ksize1; ixy1++)
		{
			for (int iy2 = 0, i = 0; iy2 < y2; iy2++)
			{
				const int ix2 = iy2 * kstride2;

				const float* www = ww + (ptrdiff_t(ixy1) * kstep);
				const float* xww = xw + (ptrdiff_t(ixy1) * xstride1) + (ptrdiff_t(ix2) * xstride2);
				float* yww = yw + (ptrdiff_t(iy2) * ystride2);

				for (int ixy0 = 0; ixy0 < x0; ixy0++, xww += xstride0, yww += ystride0)
				{
					A_Array[i] = www;
					B_Array[i] = xww;
					C_Array[i] = yww;
					i++;
				}
			}

			CBLAS_TRANSPOSE trans = CblasNoTrans;
			const float alpha_beta = 1.0f;

			::cblas_sgemm_batch(
				CblasColMajor, &trans, &trans,
				&y3, &n, &k,
				&alpha_beta,
				A_Array, &ldw,
				B_Array, &ldx,
				&alpha_beta,
				C_Array, &ldy,
				1, &group_size);
		}
	}
	else*/
	{
		// if kpadding1 or kpadding2 are negative we need to shrink working area of x tensor
		if (kpadding1 < 0)
		{
			xw += -ptrdiff_t(kpadding1) * xstride1;
			x1 += 2 * kpadding1;
			kpadding1 = 0;
		}

		if (kpadding2 < 0)
		{
			xw += -ptrdiff_t(kpadding2) * xstride2;
			x2 += 2 * kpadding2;
			kpadding2 = 0;
		}

		//for (int iy02 = 0; iy02 < y0 * y2; iy02++) {
		parallel_for(0, y0 * y2, [&](int iy02) {

			const int ixy0 = iy02 / y2;
			const int iy2 = iy02 % y2;

			const float* www = ww;
			const float* xww = xw + (ptrdiff_t(ixy0) * xstride0);
			float* yww = yw + (ptrdiff_t(iy2) * ystride2) + (ptrdiff_t(ixy0) * ystride0);

			// 1. initialize destination tensor with biases
			tile(y1, y3, bw, yww, ldy);

			// 2. add matrix product to destination
			const int ix2 = (iy2 * kstride2) - kpadding2;
			const int ix2e = min(ix2 + ksize2, x2);
			const int k = (ix2e - max(ix2, 0)) * xstride2;

			// k may be zero if the current portion of input tensor
			// is completely in the padding area
			if (k > 0)
			{
				xww += ptrdiff_t(max(ix2, 0)) * xstride2;
				www += ptrdiff_t(max(-ix2, 0)) * xstride2 * ldw;

				for (int ixy1 = 0; ixy1 < ksize1; ixy1++)
				{
					const int iy1 = ixy1 < kpadding1 ? (kpadding1 - ixy1 + kstride1 - 1) / kstride1 : 0;
					const int ix1 = ixy1 - kpadding1 + (iy1 * kstride1);
					const int n = min(y1, ((x1 - (ixy1 - kpadding1) - 1) / kstride1 + 1)) - iy1;

					if (n > 0)
					{
						::cblas_sgemm(
							CblasColMajor, CblasNoTrans, CblasNoTrans,
							y3, n, k,
							1.0f,
							www + (ptrdiff_t(ixy1) * kstep), ldw,
							xww + (ptrdiff_t(ix1) * xstride1), ldx,
							1.0f,
							yww + (ptrdiff_t(iy1) * ystride1), ldy);
					}
				}
			}
		});
	}
}

GENIXAPI(void, convolution_gradient)(
	const int ksize1,
	const int ksize2,
	const int kstride1,
	const int kstride2,
	int kpadding1,
	int kpadding2,
	const float* ww,
	float* dww,
	float* dbw,
	const int* waxes,
	const int* wstrides,
	const float* xw,
	float* dxw,
	const int* xaxes,
	const int* xstrides,
	const float* dyw,
	const int* yaxes,
	const int* ystrides)
{
	const int w0 = waxes[0];
	const int w1 = waxes[1];
	const int wstride0 = wstrides[0];
	const int wstride1 = wstrides[1];

	const int x0 = xaxes[0];
	int x1 = xaxes[1];
	int x2 = xaxes[2];
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
	if (kpadding1 < 0)
	{
		const int off = -ptrdiff_t(kpadding1) * xstride1;
		xw += off;
		if (dxw != NULL)
		{
			dxw += off;
		}

		x1 += 2 * kpadding1;
		kpadding1 = 0;
	}

	if (kpadding2 < 0)
	{
		const int off = -ptrdiff_t(kpadding2) * xstride2;
		xw += off;
		if (dxw != NULL)
		{
			dxw += off;
		}

		x2 += 2 * kpadding2;
		kpadding2 = 0;
	}

	float* ones = (float*)::alloca(ptrdiff_t(y1) * sizeof(float));
	for (int i = 0; i < y1; i++) { ones[i] = 1.0f; }

	for (int iy02 = 0; iy02 < y0 * y2; iy02++) {
	//parallel_for(0, y0 * y2, [&](int iy02) {

		const int ixy0 = iy02 / y2;
		const int iy2 = iy02 % y2;

		const float* www = ww;
		float* dwww = dww;
		const float* xww = xw + (ptrdiff_t(ixy0) * xstride0);
		float* dxww = dxw + (ptrdiff_t(ixy0) * xstride0);
		const float* dyww = dyw + (ptrdiff_t(iy2) * ystride2) + (ptrdiff_t(ixy0) * ystride0);

		// 1. Calculate biases
		::cblas_sgemv(
			CblasColMajor, CblasNoTrans,
			y3, y1,
			1.0f,
			dyww, ldy,
			ones, 1,
			1.0f,
			dbw, 1);

		// 2. calculate w and x gradients
		const int ix2 = (iy2 * kstride2) - kpadding2;
		const int ix2e = min(ix2 + ksize2, x2);
		const int k = (ix2e - max(ix2, 0)) * xstride2;

		// k may be zero if the current portion of input tensor
		// is completely in the padding area
		if (k > 0)
		{
			const int xoff = ptrdiff_t(max(ix2, 0)) * xstride2;
			xww += xoff;
			dxww += xoff;
			const int woff = ptrdiff_t(max(-ix2, 0)) * xstride2 * ldw;
			www += woff;
			dwww += woff;

			for (int ixy1 = 0; ixy1 < ksize1; ixy1++)
			{
				const int iy1 = ixy1 < kpadding1 ? (kpadding1 - ixy1 + kstride1 - 1) / kstride1 : 0;
				const int ix1 = ixy1 - kpadding1 + (iy1 * kstride1);
				const int n = min(y1, ((x1 - (ixy1 - kpadding1) - 1) / kstride1 + 1)) - iy1;

				if (n > 0)
				{
					::cblas_sgemm(
						CblasColMajor, CblasNoTrans, CblasTrans,
						y3, k, n,
						1.0f,
						dyww + (ptrdiff_t(iy1) * ystride1), ldy,
						xww + (ptrdiff_t(ix1) * xstride1), ldx,
						1.0f,
						dwww + (ptrdiff_t(ixy1) * kstep), ldw);

					if (dxw != NULL)
					{
						::cblas_sgemm(
							CblasColMajor, CblasTrans, CblasNoTrans,
							k, n, y3,
							1.0f,
							www + (ptrdiff_t(ixy1) * kstep), ldw,
							dyww + (ptrdiff_t(iy1) * ystride1), ldy,
							1.0f,
							dxww + (ptrdiff_t(ix1) * xstride1), ldx);
					}
				}
			}
		}
	}/*);*/
}

