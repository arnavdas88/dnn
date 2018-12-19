#include "stdafx.h"
#include "mkl.h"

#include <ppl.h>
using namespace concurrency;

#define MT

void __forceinline tile(const int count, const int length, const float* src, float* dst, const int dststep)
{
	for (int i = 0; i < count; i++, dst += dststep)
	{
		memcpy(dst, src, length * sizeof(float));
	}
}

#ifdef MT
template <typename _Index_type, typename _Function>
void _parallel_for(_Index_type _First0, _Index_type _Last0, _Index_type _First1, _Index_type _Last1, const _Function& _Func, const auto_partitioner& _Part = auto_partitioner())
{
	_Index_type count = (_Last0 - _First0) * (_Last1 - _First1);
	parallel_for(0, count, [&](_Index_type i) {
		const _Index_type i0 = (i / (_Last1 - _First1)) + _First0;
		const _Index_type i1 = (i % (_Last1 - _First1)) + _First1;
		_Func(i0, i1);
	}, _Part);
}
#endif

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

#ifdef MT
		_parallel_for(0, y0, 0, y2, [&](int ixy0, int iy2)
		{
#else
		for (int ixy0 = 0; ixy0 < y0; ixy0++)
		{
			for (int iy2 = 0; iy2 < y2; iy2++)
			{
#endif
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
#ifdef MT
		});
#else
		}
		}
#endif
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

#ifdef MT
	parallel_invoke(
		[&]
#endif
		{
			// 1. Calculate biases gradient
			const int n = y0 * y1 * y2;
			float* ones = new float[n];
			for (int i = 0; i < n; i++) { ones[i] = 1.0f; }

			::cblas_sgemv(
				CblasColMajor, CblasNoTrans,
				y3, n,
				1.0f,
				dyw, y3,
				ones, 1,
				1.0f,
				dbw, 1);

			delete[] ones;
		}
#ifdef MT
		, [&]
#endif
		{
			// 2. Calculate weights gradient
#ifdef MT
			parallel_for(0, ksize1, [&](int ixy1)
#else
			for (int ixy1 = 0; ixy1 < ksize1; ixy1++)
#endif
			{
				float* dwww = dww + (ptrdiff_t(ixy1) * kstep);
				const float* xww = xw;

				for (int ixy0 = 0; ixy0 < y0; ixy0++, xww += xstride0)
				{
					for (int iy2 = 0; iy2 < y2; iy2++)
					{
						const float* dyww = dyw + (ptrdiff_t(iy2) * ystride2) + (ptrdiff_t(ixy0) * ystride0);

						const int ix2 = (iy2 * kstride2) - kpadding2;
						const int ix2e = min(ix2 + ksize2, x2);
						const int k = (ix2e - max(ix2, 0)) * xstride2;

						// k may be zero if the current portion of input tensor
						// is completely in the padding area
						if (k > 0)
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
									xww + (ptrdiff_t(ix1) * xstride1) + (ptrdiff_t(max(ix2, 0)) * xstride2), ldx,
									1.0f,
									dwww + (ptrdiff_t(max(-ix2, 0)) * xstride2 * ldw), ldw);
							}
						}
					}
				}
#ifdef MT
			});
#else
			}
#endif
		}
#ifdef MT
			, [&]
#endif
		{
			// 3. Calculate x gradient
			if (dxw != NULL)
			{
				const int iy2step = (ksize2 + kstride2 - 1) / kstride2;
				for (int iy2start = 0; iy2start < iy2step; iy2start++)
				{
#ifdef MT
					parallel_for(iy2start, y2, iy2step, [&](int iy2)
#else
					for (int iy2 = iy2start; iy2 < y2; iy2 += iy2step)
#endif
					{
						for (int ixy0 = 0; ixy0 < y0; ixy0++)
						{
							const float* www = ww;
							float* dxww = dxw + (ptrdiff_t(ixy0) * xstride0);
							const float* dyww = dyw + (ptrdiff_t(iy2) * ystride2) + (ptrdiff_t(ixy0) * ystride0);

							const int ix2 = (iy2 * kstride2) - kpadding2;
							const int ix2e = min(ix2 + ksize2, x2);
							const int k = (ix2e - max(ix2, 0)) * xstride2;

							// k may be zero if the current portion of input tensor
							// is completely in the padding area
							if (k > 0)
							{
								dxww += ptrdiff_t(max(ix2, 0)) * xstride2;
								www += ptrdiff_t(max(-ix2, 0)) * xstride2 * ldw;

								for (int ixy1 = 0; ixy1 < ksize1; ixy1++)
								{
									const int iy1 = ixy1 < kpadding1 ? (kpadding1 - ixy1 + kstride1 - 1) / kstride1 : 0;
									const int ix1 = ixy1 - kpadding1 + (iy1 * kstride1);
									const int n = min(y1, ((x1 - (ixy1 - kpadding1) - 1) / kstride1 + 1)) - iy1;

									if (n > 0)
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
#ifdef MT
					});
#else
					}
#endif
				}
			}
		}
#ifdef MT
		);
#endif
}

