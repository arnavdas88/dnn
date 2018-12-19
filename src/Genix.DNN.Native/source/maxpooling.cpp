#include "stdafx.h"
#include <stdlib.h>

#include <ppl.h>
using namespace concurrency;

#define MT

void __forceinline _max(const int n, const float* a, const float* b, float* y)
{
	for (int i = 0; i < n; i++)
	{
		y[i] = max(a[i], b[i]);
	}
}

void __forceinline _max(const int n, const float* a, const float* b, const float* c, float* y)
{
	for (int i = 0; i < n; i++)
	{
		const float ab = max(a[i], b[i]);
		y[i] = max(ab, c[i]);
	}
}

void __forceinline _max(const int n, const float* a, const float* b, const float* c, const float* d, float* y)
{
	for (int i = 0; i < n; i++)
	{
		const float ab = max(a[i], b[i]);
		const float cd = max(c[i], d[i]);
		y[i] = max(ab, cd);
	}
}

void __forceinline matchandadd(const int n, const float* x, const float* xmask, float* y, const float* ymask)
{
	for (int i = 0; i < n; i++)
	{
		y[i] += (xmask[i] == ymask[i] ? 1.0f : 0.0f) * x[i];
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

GENIXAPI(void, maxpooling)(
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
#ifdef MT
		_parallel_for(0, y0, 0, y1, [&](int ixy0, int iy1)
		{
#else
		for (int ixy0 = 0; ixy0 < y0; ixy0++)
		{
			for (int iy1 = 0; iy1 < y1; iy1++)
			{
#endif
				const int ix1 = (iy1 * kstride1) - kpadding1;
				const int ix1b = max(ix1, 0);
				const int ix1e = min(ix1 + ksize1, x1);

				const float* xww = xw + (ptrdiff_t(ixy0) * xstride0) + (ptrdiff_t(ix1b) * xstride1);
				float* yww = yw + (ptrdiff_t(ixy0) * ystride0) + (ptrdiff_t(iy1) * ystride1);

				// add along horizontal axis
				int size1 = ix1e - ix1b;
				if (size1 > 0)
				{
					float* xww1 = NULL;
					if (size1 > 1)
					{
						xww1 = new float[xstride1];
						_max(xstride1, xww, xww + xstride1, xww1);
					}

					// add along vertical axis
					for (int iy2 = 0, ix2 = -kpadding2; iy2 < y2; iy2++, ix2 += kstride2, yww += ystride2)
					{
						const int ix2b = max(ix2, 0);
						const int ix2e = min(ix2 + ksize2, x2);
						const float* xww2 = (xww1 != NULL ? xww1 : xww) + (ptrdiff_t(ix2b) * xstride2);

						switch (ix2e - ix2b)
						{
						case 0:
							::memset(yww, 0, ystride2 * sizeof(float));
							break;
						case 1:
							::memcpy(yww, xww2, xstride2 * sizeof(float));
							break;
						case 2:
							_max(xstride2, xww2, xww2 + xstride2, yww);
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
#ifdef MT
		});
#else
		}
		}
#endif
	}
	else
	{
#ifdef MT
		_parallel_for(0, y0, 0, y1, [&](int ixy0, int iy1)
		{
#else
		for (int ixy0 = 0; ixy0 < y0; ixy0++)
		{
			for (int iy1 = 0; iy1 < y1; iy1++)
			{
#endif
				const int ix1 = (iy1 * kstride1) - kpadding1;
				const int ix1b = max(ix1, 0);
				const int ix1e = min(ix1 + ksize1, x1);

				const float* xww = xw + (ptrdiff_t(ixy0) * xstride0) + (ptrdiff_t(ix1b) * xstride1);
				float* yww = yw + (ptrdiff_t(ixy0) * ystride0) + (ptrdiff_t(iy1) * ystride1);

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
							_max(xstride1, xww, xww + xstride1, xww1);
							break;
						case 3:
							_max(xstride1, xww, xww + xstride1, xww + (ptrdiff_t(2) * xstride1), xww1);
							break;
						default:
							_max(xstride1, xww, xww + xstride1, xww + (ptrdiff_t(2) * xstride1), xww + (ptrdiff_t(3) * xstride1), xww1);

							for (size1 -= 4, xww += ptrdiff_t(4) * xstride1; size1 >= 3; size1 -= 3, xww += ptrdiff_t(3) * xstride1)
							{
								_max(xstride1, xww, xww + xstride1, xww + (ptrdiff_t(2) * xstride1), xww1, xww1);
							}

							switch (size1)
							{
							case 1:
								_max(xstride1, xww, xww1, xww1);
								break;
							case 2:
								_max(xstride1, xww, xww + xstride1, xww1, xww1);
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
							const float* xww2 = (xww1 != NULL ? xww1 : xww) + (ptrdiff_t(ix2b) * xstride2);

							switch (size2)
							{
							case 1:
								::memcpy(yww, xww2, xstride2 * sizeof(float));
								break;
							case 2:
								_max(xstride2, xww2, xww2 + xstride2, yww);
								break;
							case 3:
								_max(xstride2, xww2, xww2 + xstride2, xww2 + (ptrdiff_t(2) * xstride2), yww);
								break;
							default:
								_max(xstride2, xww2, xww2 + xstride2, xww2 + (ptrdiff_t(2) * xstride2), xww2 + (ptrdiff_t(3) * xstride2), yww);

								const float* xwww2 = xww2 + (ptrdiff_t(4) * xstride2);
								for (size2 -= 4; size2 >= 3; size2 -= 3, xwww2 += ptrdiff_t(3) * xstride2)
								{
									_max(xstride2, xwww2, xwww2 + xstride2, xwww2 + (ptrdiff_t(2) * xstride2), yww, yww);
								}

								switch (size2)
								{
								case 1:
									_max(xstride2, xwww2, yww, yww);
									break;
								case 2:
									_max(xstride2, xwww2, xwww2 + xstride2, yww, yww);
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
#ifdef MT
		});
#else
		}
		}
#endif
	}
}

GENIXAPI(void, maxpooling_gradient)(
	const int ksize1,
	const int ksize2,
	const int kstride1,
	const int kstride2,
	int kpadding1,
	int kpadding2,
	float* xw,
	float* dxw,
	const int* xaxes,
	const int* xstrides,
	const float* yw,
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
	const int xstride1K = xstride1 * kstride1;
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
		const int off = -ptrdiff_t(kpadding1) * xstride1;
		xw += off;
		dxw += off;
		x1 += 2 * kpadding1;
		kpadding1 = 0;
	}

	if (kpadding2 < 0)
	{
		const int off = -ptrdiff_t(kpadding2) * xstride2;
		xw += off;
		dxw += off;
		x2 += 2 * kpadding2;
		kpadding2 = 0;
	}

	// cycle by the output
	const int iy1step = (ksize1 + kstride1 - 1) / kstride1;
	for (int iy1start = 0; iy1start < iy1step; iy1start++)
	{
#ifdef MT
		parallel_for(iy1start, y1, iy1step, [&](int iy1)
#else
		for (int iy1 = iy1start; iy1 < y1; iy1 += iy1step)
#endif
		{
			const int ix1 = (iy1 * kstride1) - kpadding1;
			const int kb1 = max(ix1, 0);
			const int ke1 = min(ix1 + ksize1, x1);

			for (int ix0 = 0, yoff1 = iy1 * ystride1, xoff1 = kb1 * xstride1; ix0 < x0; ix0++, yoff1 += ystride0, xoff1 += xstride0)
			{
				for (int iy2 = 0, ix2 = -kpadding2, yoff2 = yoff1; iy2 < y2; iy2++, ix2 += kstride2, yoff2 += ystride2)
				{
					const int kb2 = max(ix2, 0);
					const int ke2 = min(ix2 + ksize2, x2);

					// cycle by the kernel
					for (int ik1 = kb1, kxoff1 = xoff1 + (kb2 * xstride2); ik1 < ke1; ik1++, kxoff1 += xstride1)
					{
						for (int ik2 = kb2, kxoff2 = kxoff1; ik2 < ke2; ik2++, kxoff2 += xstride2)
						{
							matchandadd(ystride2, dyw + yoff2, yw + yoff2, dxw + kxoff2, xw + kxoff2);
						}
					}
				}
			}
		}
#ifdef MT
		);
#endif
	}
}
