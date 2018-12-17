#include "stdafx.h"
#include <stdlib.h>

#include <ppl.h>
using namespace concurrency;

void __forceinline sum(const int n, const float* a, const float* b, float* y)
{
	for (int i = 0; i < n; i++)
	{
		y[i] = a[i] + b[i];
	}
}

void __forceinline sum(const int n, const float* a, const float* b, const float* c, float* y)
{
	for (int i = 0; i < n; i++)
	{
		y[i] = a[i] + b[i] + c[i];
	}
}

void __forceinline sum(const int n, const float* a, const float* b, const float* c, const float* d, float* y)
{
	for (int i = 0; i < n; i++)
	{
		y[i] = a[i] + b[i] + c[i] + d[i];
	}
}

void __forceinline add(const int n, const float* a, float* y)
{
	for (int i = 0; i < n; i++)
	{
		y[i] += a[i];
	}
}

void __forceinline add(const int n, const float* a, const float* b, float* y)
{
	for (int i = 0; i < n; i++)
	{
		y[i] += a[i] + b[i];
	}
}

void __forceinline add(const int n, const float* a, const float* b, const float* c, float* y)
{
	for (int i = 0; i < n; i++)
	{
		y[i] += a[i] + b[i] + c[i];
	}
}

void __forceinline add(const int n, const float* a, const float* b, const float* c, const float* d, float* y)
{
	for (int i = 0; i < n; i++)
	{
		y[i] += a[i] + b[i] + c[i] + d[i];
	}
}

void __forceinline div(const int n, const float* x, float* y, const float divisor)
{
	for (int i = 0; i < n; i++)
	{
		y[i] = x[i] / divisor;
	}
}

void __forceinline sum_div(const int n, const float* a, const float* b, float* y, const float divisor)
{
	for (int i = 0; i < n; i++)
	{
		y[i] = (a[i] + b[i]) / divisor;
	}
}

void __forceinline sum_div(const int n, const float* a, const float* b, const float* c, float* y, const float divisor)
{
	for (int i = 0; i < n; i++)
	{
		y[i] = (a[i] + b[i] + c[i]) / divisor;
	}
}

void __forceinline sum_div(const int n, const float* a, const float* b, const float* c, const float* d, float* y, const float divisor)
{
	for (int i = 0; i < n; i++)
	{
		y[i] = (a[i] + b[i] + c[i] + d[i]) / divisor;
	}
}

void __forceinline add_div(const int n, const float* x, float* y, const float divisor)
{
	for (int i = 0; i < n; i++)
	{
		y[i] += x[i];
		y[i] /= divisor;
	}
}

void __forceinline add_div(const int n, const float* a, const float* b, float* y, const float divisor)
{
	for (int i = 0; i < n; i++)
	{
		y[i] += a[i] + b[i];
		y[i] /= divisor;
	}
}

void __forceinline add_div(const int n, const float* a, const float* b, const float* c, float* y, const float divisor)
{
	for (int i = 0; i < n; i++)
	{
		y[i] += a[i] + b[i] + c[i];
		y[i] /= divisor;
	}
}

void __forceinline add_div(const int n, const float* a, const float* b, const float* c, const float* d, float* y, const float divisor)
{
	for (int i = 0; i < n; i++)
	{
		y[i] += a[i] + b[i] + c[i] + d[i];
		y[i] /= divisor;
	}
}

void __forceinline mulc(const int n, const float* x, const float a, float* y)
{
	for (int i = 0; i < n; i++)
	{
		y[i] = x[i] * a;
	}
}

void __forceinline add_productc(const int n, const float* x, const float a, float* y)
{
	for (int i = 0; i < n; i++)
	{
		y[i] += x[i] * a;
	}
}

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

GENIXAPI(void, avgpooling)(
	const int ksize1,
	const int ksize2,
	const int kstride1,
	const int kstride2,
	int kpadding1,
	int kpadding2,
	const float* xw,
	const int* xaxes,
	const int* xstrides,
	float* yw,
	const int* yaxes,
	const int* ystrides)
{
	const int x0 = xaxes[0];
	int x1 = xaxes[1];
	int x2 = xaxes[2];
	const int xstride0 = xstrides[0];
	const int xstride1 = xstrides[1];
	const int xstride2 = xstrides[2];
	const int xstride2K = xstride2 * kstride2;

	const int y0 = yaxes[0];
	const int y1 = yaxes[1];
	const int y2 = yaxes[2];
	const int ystride0 = ystrides[0];
	const int ystride1 = ystrides[1];
	const int ystride2 = ystrides[2];

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

	if (ksize1 == 2 && ksize2 == 2)
	{
		for (int ixy0 = 0; ixy0 < y0; ixy0++) {
			for (int iy1 = 0; iy1 < y1; iy1++) {
				//_parallel_for(0, y0, 0, y1, [&](int ixy0, int iy1) {

				const int ix1 = (iy1 * kstride1) - kpadding1;
				const int ix1b = max(ix1, 0);
				const int ix1e = min(ix1 + ksize1, x1);

				const float* xww = xw + (ptrdiff_t(ixy0) * xstride0) + (ptrdiff_t(ix1b) * xstride1);
				float* yww = yw + (ptrdiff_t(ixy0) * ystride0) + (ptrdiff_t(iy1) * ystride1);
				const float divisor = (float)(ksize1 * ksize2);

				// add along horizontal axis
				int size1 = ix1e - ix1b;
				if (size1 > 0)
				{
					float* xww1 = NULL;
					if (size1 > 1)
					{
						xww1 = new float[xstride1];
						sum(xstride1, xww, xww + xstride1, xww1);
					}

					// add along vertical axis
					for (int iy2 = 0, ix2 = -kpadding2; iy2 < y2; iy2++, ix2 += kstride2, yww += ystride2)
					{
						const int ix2b = max(ix2, 0);
						const int ix2e = min(ix2 + ksize2, x2);
						const float* xww2 = (xww1 != NULL ? xww1 : xww) + (ix2b * xstride2);

						switch (ix2e - ix2b)
						{
						case 0:
							::memset(yww, 0, ystride2 * sizeof(float));
							break;
						case 1:
							div(xstride2, xww2, yww, divisor);
							break;
						case 2:
							sum_div(xstride2, xww2, xww2 + xstride2, yww, divisor);
							break;
						}
					}

					if (xww1 != NULL)
					{
						delete[] xww1;
					}
				}
				else
				{
					::memset(yww, 0, ystride1 * sizeof(float));
				}
				/*});*/
			}
		}
	}
	else
	{
		for (int ixy0 = 0; ixy0 < y0; ixy0++) {
			for (int iy1 = 0; iy1 < y1; iy1++) {
				//_parallel_for(0, y0, 0, y1, [&](int ixy0, int iy1) {

				const int ix1 = (iy1 * kstride1) - kpadding1;
				const int ix1b = max(ix1, 0);
				const int ix1e = min(ix1 + ksize1, x1);

				const float* xww = xw + (ptrdiff_t(ixy0) * xstride0) + (ptrdiff_t(ix1b) * xstride1);
				float* yww = yw + (ptrdiff_t(ixy0) * ystride0) + (ptrdiff_t(iy1) * ystride1);
				const float divisor = (float)(ksize1 * ksize2);

				// add along horizontal axis
				int size1 = ix1e - ix1b;
				if (size1 > 0)
				{
					float* xww1 = NULL;
					if (size1 > 1)
					{
						xww1 = new float[xstride1];

						switch (size1)
						{
						case 2:
							sum(xstride1, xww, xww + xstride1, xww1);
							break;
						case 3:
							sum(xstride1, xww, xww + xstride1, xww + (ptrdiff_t(2) * xstride1), xww1);
							break;
						case 4:
							sum(xstride1, xww, xww + xstride1, xww + (ptrdiff_t(2) * xstride1), xww + (ptrdiff_t(3) * xstride1), xww1);
							break;
						default:
							sum(xstride1, xww, xww + xstride1, xww + (ptrdiff_t(2) * xstride1), xww + (ptrdiff_t(3) * xstride1), xww1);

							for (size1 -= 4, xww += ptrdiff_t(4) * xstride1; size1 >= 4; size1 -= 4, xww += ptrdiff_t(4) * xstride1)
							{
								add(xstride1, xww, xww + xstride1, xww + (ptrdiff_t(2) * xstride1), xww + (ptrdiff_t(3) * xstride1), xww1);
							}

							switch (size1)
							{
							case 1:
								add(xstride1, xww, xww1);
								break;
							case 2:
								add(xstride1, xww, xww + xstride1, xww1);
								break;
							case 3:
								add(xstride1, xww, xww + xstride1, xww + (ptrdiff_t(2) * xstride1), xww1);
								break;
							}
							break;
						}
					}

					// add along vertical axis
					for (int iy2 = 0, ix2 = -kpadding2; iy2 < y2; iy2++, ix2 += kstride2, yww += ystride2)
					{
						const int ix2b = max(ix2, 0);
						const int ix2e = min(ix2 + ksize2, x2);

						int size2 = ix2e - ix2b;
						if (size2 > 0)
						{
							const float* xww2 = (xww1 != NULL ? xww1 : xww) + (ix2b * xstride2);

							switch (size2)
							{
							case 1:
								div(xstride2, xww2, yww, divisor);
								break;
							case 2:
								sum_div(xstride2, xww2, xww2 + xstride2, yww, divisor);
								break;
							case 3:
								sum_div(xstride2, xww2, xww2 + xstride2, xww2 + (ptrdiff_t(2) * xstride2), yww, divisor);
								break;
							case 4:
								sum_div(xstride2, xww2, xww2 + xstride2, xww2 + (ptrdiff_t(2) * xstride2), xww2 + (ptrdiff_t(3) * xstride2), yww, divisor);
								break;
							default:
								sum(xstride2, xww2, xww2 + xstride2, xww2 + (ptrdiff_t(2) * xstride2), xww2 + (ptrdiff_t(3) * xstride2), yww);

								const float* xwww2 = xww2 + (ptrdiff_t(4) * xstride2);
								for (size2 -= 4; size2 > 4; size2 -= 4, xwww2 += ptrdiff_t(4) * xstride2)
								{
									add(xstride2, xwww2, xwww2 + xstride2, xwww2 + (ptrdiff_t(2) * xstride2), xwww2 + (ptrdiff_t(3) * xstride2), yww);
								}

								switch (size2)
								{
								case 1:
									add_div(xstride2, xwww2, yww, divisor);
									break;
								case 2:
									add_div(xstride2, xwww2, xwww2 + xstride2, yww, divisor);
									break;
								case 3:
									add_div(xstride2, xwww2, xwww2 + xstride2, xwww2 + (ptrdiff_t(2) * xstride2), yww, divisor);
									break;
								case 4:
									add_div(xstride2, xwww2, xwww2 + xstride2, xwww2 + (ptrdiff_t(2) * xstride2), xwww2 + (ptrdiff_t(3) * xstride2), yww, divisor);
									break;
								}
								break;
							}
						}
						else
						{
							::memset(yww, 0, ystride2 * sizeof(float));
						}
					}

					if (xww1 != NULL)
					{
						delete[] xww1;
					}
				}
				else
				{
					::memset(yww, 0, ystride1 * sizeof(float));
				}
				/*});*/
			}
		}
	}
}

GENIXAPI(void, avgpooling_gradient)(
	const int ksize1,
	const int ksize2,
	const int kstride1,
	const int kstride2,
	int kpadding1,
	int kpadding2,
	float* dxw,
	const int* xaxes,
	const int* xstrides,
	const float* dyw,
	const int* yaxes,
	const int* ystrides)
{
	const int x0 = xaxes[0];
	int x1 = xaxes[1];
	int x2 = xaxes[2];
	const int xstride0 = xstrides[0];
	const int xstride1 = xstrides[1];
	const int xstride2 = xstrides[2];
	const int xstride2K = xstride2 * kstride2;

	const int y0 = yaxes[0];
	const int y1 = yaxes[1];
	const int y2 = yaxes[2];
	const int ystride0 = ystrides[0];
	const int ystride1 = ystrides[1];
	const int ystride2 = ystrides[2];

	// if kpadding1 or kpadding2 are negative we need to shrink working area of x tensor
	if (kpadding1 < 0)
	{
		dxw += -ptrdiff_t(kpadding1) * xstride1;
		x1 += 2 * kpadding1;
		kpadding1 = 0;
	}

	if (kpadding2 < 0)
	{
		dxw += -ptrdiff_t(kpadding2) * xstride2;
		x2 += 2 * kpadding2;
		kpadding2 = 0;
	}

	const int iy2step = (ksize2 + kstride2 - 1) / kstride2;
	for (int iy2start = 0; iy2start < iy2step; iy2start++)
	{
		for (int iy2 = iy2start; iy2 < y2; iy2 += iy2step) {
		//parallel_for(iy2start, y2, iy2step, [&](int iy2) {

			float* buf = new float[ystride2];

			float* dxww = dxw;
			const float* dyww = dyw + (ptrdiff_t(iy2) * ystride2);
			for (int ixy0 = 0; ixy0 < y0; ixy0++, dxww += xstride0, dyww += ystride0)
			{
				const int ix2 = (iy2 * kstride2) - kpadding2;
				const int ix2b = max(ix2, 0);
				const int ix2e = min(ix2 + ksize2, x2);

				// portion of input tensormay be completely in the padding area
				if (ix2b < ix2e)
				{
					const float* dyww1 = dyww;
					for (int iy1 = 0; iy1 < y1; iy1++, dyww1 += ystride1)
					{
						const int ix1 = (iy1 * kstride1) - kpadding1;
						const int ix1b = max(ix1, 0);
						const int ix1e = min(ix1 + ksize1, x1);

						if (ix1b < ix1e)
						{
							mulc(ystride2, dyww1, 1.0f / (ksize1 * ksize2), buf);

							float* dxww1 = dxww + (ix1b * xstride1) + (ix2b * xstride2);
							for (int ik1 = ix1b; ik1 < ix1e; ik1++, dxww1 += xstride1)
							{
								float* dxww2 = dxww1;
								for (int ik2 = ix2b; ik2 < ix2e; ik2++, dxww2 += xstride2)
								{
									add(ystride2, buf, dxww2);
								}
							}
						}
					}
				}
			}

			delete[] buf;
		}/*);*/
	}
}
