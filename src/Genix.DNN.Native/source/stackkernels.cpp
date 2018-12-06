#include "stdafx.h"
#include <stdlib.h>

void __forceinline __set(int n, float* y, int offy)
{
	::memset(y + offy, 0, n * sizeof(float));
	/*y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = 0;
	}*/
}
void __forceinline __copy(int n, const float* x, int offx, float* y, int offy)
{
	::memcpy(y + offy, x + offx, n * sizeof(float));
}

void __forceinline __copy_strides(int count, int n, const float* x, int offx, int stepx, float* y, int offy, int stepy)
{
	x += offx;
	y += offy;
	n *= sizeof(float);

	for (int i = 0; i < count; i++, x += stepx, y += stepy)
	{
		::memcpy(y, x, n);
	}
}

void __forceinline __add(int n, const float* x, int offx, float* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] += x[i];
	}
}

#if true
GENIXAPI(void, stack_kernels)(
	const int ksize1,
	const int ksize2,
	const int kstride1,
	const int kstride2,
	const int kpadding1,
	const int kpadding2,
	const float* xw,
	const int* xaxes,
	const int* xstrides,
	float* yw,
	const int ywlen,
	const int* yaxes,
	const int* ystrides)
{
	const int x0 = xaxes[0];
	const int x1 = xaxes[1];
	const int x2 = xaxes[2];
	const int xstride0 = xstrides[0];
	const int xstride1 = xstrides[1];
	const int xstride2 = xstrides[2];
	const int y1 = yaxes[1];
	const int y2 = yaxes[2];
	const int ystride0 = ystrides[0];
	////const int ystride1 = ystrides[1];

	const int ystride2 = ksize2 * xstride2;
	const int xstride1K = xstride1 * kstride1;
	const int xstride2K = xstride2 * kstride2;

	if (kpadding1 == 0 && kpadding2 == 0)
	{
#if false
		////#pragma loop(hint_parallel(0))
		////#pragma loop(ivdep)
		for (int iy2 = 0; iy2 < y2; iy2++)
		{
			const int ix2 = iy2 * kstride2;

			for (int iy1 = 0, /*ix1 = 0,*/ xpos = ix2 * xstride2, ypos = (iy2 * y1) * ystride0; iy1 < y1; iy1++, /*ix1 += kstride1,*/ xpos += xstride1K, ypos += ystride0)
			{
				int xposk = xpos;
				int yposk = ypos;

				int ix1 = iy1 * kstride1;
				const int ix1e = ix1 + ksize1;
				const int ix1m = min(ix1e, x1);

				/*if (ix1 > 0)
				{
					const int x1over = ksize1 - kstride1;
					if (x1over > 0)
					{
						const int copycount = x1over * ystride2;
						__copy(copycount, yw, xposk - copycount, yw, yposk);

						ix1 += x1over;
						yposk += copycount;
					}
				}*/

				if (ix2 + ksize2 <= x2)
				{
					for (; ix1 < ix1m; ix1++, xposk += xstride1, yposk += ystride2)
					{
						__copy(ystride2, xw, xposk, yw, yposk);
					}
				}
				else
				{
					const int copycount = (x2 - ix2) * xstride2;
					const int setcount = ystride2 - copycount;
					for (; ix1 < ix1m; ix1++, xposk += xstride1, yposk += ystride2)
					{
						__copy(copycount, xw, xposk, yw, yposk);
						__set(setcount, yw, yposk + copycount);
					}
				}

				if (ix1 < ix1e)
				{
					__set((ix1e - ix1) * ystride2, yw, yposk);
				}
			}
		}
#else
		#pragma loop(hint_parallel(0))
		#pragma loop(ivdep)
		for (int iy = 0, iiy = ywlen / ystride0; iy < iiy; iy++)
		{
			int ix1 = (iy / y2) * kstride1;
			const int ix2 = (iy % y2) * kstride2;

			int xposk = (ix1 * xstride1) + (ix2 * xstride2);
			int yposk = iy * ystride0;

			ix1 %= x1;
			const int ix1e = ix1 + ksize1;
			const int ix1m = min(ix1e, x1);

			if (ix2 + ksize2 <= x2)
			{
				for (; ix1 < ix1m; ix1++, xposk += xstride1, yposk += ystride2)
				{
					__copy(ystride2, xw, xposk, yw, yposk);
				}
			}
			else
			{
				const int copycount = (x2 - ix2) * xstride2;
				const int setcount = ystride2 - copycount;
				for (; ix1 < ix1m; ix1++, xposk += xstride1, yposk += ystride2)
				{
					__copy(copycount, xw, xposk, yw, yposk);
					__set(setcount, yw, yposk + copycount);
				}
			}

			if (ix1 < ix1e)
			{
				__set((ix1e - ix1) * ystride2, yw, yposk);
			}
		}
#endif
	}
	else
	{
#if true
		//#pragma loop(hint_parallel(0))
		//#pragma loop(ivdep)
		for (int iy = 0, iiy = ywlen / ystride0; iy < iiy; iy++)
		{
			int iy1 = iy / y2;
			const int iy2 = iy % y2;
			const int iy0 = iy1 / y1;
			iy1 %= y1;

			int ix1 = (iy1 * kstride1) - kpadding1;
			const int ix2 = (iy2 * kstride2) - kpadding2;

			int xposk = (iy0 * xstride0) + (ix1 * xstride1) + (ix2 * xstride2);
			int yposk = iy * ystride0;

			const int ix1e = ix1 + ksize1;
			const int ix1m = min(ix1e, x1);

			if (ix1 < 0)
			{
				const int setcount = -ix1 * ystride2;
				__set(setcount, yw, yposk);
				xposk += -ix1 * xstride1;
				yposk += setcount;
				ix1 = 0;
			}

			const int ix2e = ix2 + ksize2;
			const int ix2m = min(ix2e, x2);

			if (ix2 >= 0 && ix2e <= ix2m)
			{
				for (; ix1 < ix1m; ix1++, xposk += xstride1, yposk += ystride2)
				{
					__copy(ystride2, xw, xposk, yw, yposk);
				}
			}
			else
			{
				const int setcount1 = min(max(-ix2, 0), ksize2) * xstride2;
				const int copycount = ((ix2m - ix2) * xstride2) - setcount1;
				const int setcount2 = ystride2 - copycount - setcount1;
				for (; ix1 < ix1m; ix1++, xposk += xstride1, yposk += ystride2)
				{
					if (setcount1 > 0)
					{
						__set(setcount1, yw, yposk);
					}

					if (copycount > 0)
					{
						__copy(copycount, xw, xposk + setcount1, yw, yposk + setcount1);
					}

					if (setcount2 > 0)
					{
						__set(setcount2, yw, yposk + setcount1 + copycount);
					}
				}
			}

			if (ix1m < ix1e)
			{
				__set((ix1e - ix1m) * ystride2, yw, yposk);
			}
		}
#else
		for (int ix0 = 0, xpos0 = (-kpadding1 * xstride1) + (-kpadding2 * xstride2), ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0)
		{
			for (int iy1 = 0, ix1 = -kpadding1, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, xpos1 += xstride1K)
			{
				for (int iy2 = 0, ix2 = -kpadding2, xpos2 = xpos1; iy2 < y2; iy2++, ix2 += kstride2, xpos2 += xstride2K)
				{
					// optimized version for BWHC layout
					// copy inner HC part of the tensor in one operation
					for (int ixk = ix1, ixke = ixk + ksize1, xposk = xpos2, yposk = ypos0;
						ixk < ixke;
						ixk++, xposk += xstride1, yposk += ystride2)
					{
						if (ixk >= 0 && ixk < x1)
						{
							int xposk2 = xposk;
							int yposk2 = yposk;

							int start = ix2;
							int end = ix2 + ksize2;

							if (start < 0)
							{
								const int count = -start * xstride2;
								__set(count, yw, yposk2);
								xposk2 += count;
								yposk2 += count;

								start = 0;
							}

							if (end > x2)
							{
								__set((end - x2) * xstride2, yw, yposk2 + ((x2 - start) * xstride2));

								end = x2;
							}

							__copy((end - start) * xstride2, xw, xposk2, yw, yposk2);
						}
						else
						{
							__set(ystride2, yw, yposk);
						}
					}

					ypos0 += ystride0;
				}
			}
		}
#endif
	}
}

GENIXAPI(void, stack_kernels_gradient)(
	const int ksize1,
	const int ksize2,
	const int kstride1,
	const int kstride2,
	const int kpadding1,
	const int kpadding2,
	float* dxw,
	const int* xaxes,
	const int* xstrides,
	const float* dyw,
	const int* yaxes,
	const int* ystrides)
{
	const int x0 = xaxes[0];
	const int x1 = xaxes[1];
	const int x2 = xaxes[2];
	const int xstride0 = xstrides[0];
	const int xstride1 = xstrides[1];
	const int xstride2 = xstrides[2];
	const int y1 = yaxes[1];
	const int y2 = yaxes[2];
	const int ystride0 = ystrides[0];
	////const int ystride1 = ystrides[1];

	const int ystride2 = ksize2 * xstride2;
	const int xstride1K = xstride1 * kstride1;
	const int xstride2K = xstride2 * kstride2;

	for (int ix0 = 0, xpos0 = (-kpadding1 * xstride1) + (-kpadding2 * xstride2), ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0)
	{
		for (int iy1 = 0, ix1 = -kpadding1, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, xpos1 += xstride1K)
		{
			for (int iy2 = 0, ix2 = -kpadding2, xpos2 = xpos1; iy2 < y2; iy2++, ix2 += kstride2, xpos2 += xstride2K)
			{
				for (int ixk = ix1, ixke = ixk + ksize1, xposk = xpos2, yposk = ypos0;
					ixk < ixke;
					ixk++, xposk += xstride1, yposk += ystride2)
				{
					if (ixk >= 0 && ixk < x1)
					{
						int xposk2 = xposk;
						int yposk2 = yposk;

						int start = ix2;
						int end = ix2 + ksize2;

						if (start < 0)
						{
							int count = -start * xstride2;
							xposk2 += count;
							yposk2 += count;

							start = 0;
						}

						if (end > x2)
						{
							end = x2;
						}

						__add((end - start) * xstride2, dyw, yposk2, dxw, xposk2);
					}
				}

				ypos0 += ystride0;
			}
		}
	}
}
#else
GENIXAPI(void, stack_kernels)(
	const int ksize1,
	const int ksize2,
	const int kstride1,
	const int kstride2,
	const int kpadding1,
	const int kpadding2,
	const float* xw,
	const int* xaxes,
	const int* xstrides,
	float* yw,
	const int ywlen,
	const int* yaxes,
	const int* ystrides)
{
	const int x0 = xaxes[0];
	const int x1 = xaxes[1];
	const int x2 = xaxes[2];
	const int xstride0 = xstrides[0];
	const int xstride1 = xstrides[1];
	const int xstride2 = xstrides[2];
	const int y1 = yaxes[1];
	const int y2 = yaxes[2];
	const int ystride0 = ystrides[0];
	const int ystride1 = ystrides[1];

	if (kpadding1 == 0 && kpadding2 == 0)
	{
		for (int ix0 = 0, xpos0 = 0, ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0)
		{
			for (int iy1 = 0, ix1 = 0, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, xpos1 += xstride1)
			{
				for (int iy2 = 0, ix2 = 0, xpos2 = xpos1; iy2 < y2; iy2++, ix2 += kstride2, xpos2 += xstride2)
				{
					// optimized version for BWHC layout
					// copy inner HC part of the tensor in one operation
					int ixk = ix1;
					const int ixke = ixk + ksize1;
					int xposk = xpos2;
					int yposk = ypos0;

					if (ix2 + ksize2 <= x2)
					{
						for (; ixk < ixke && ixk < x1; ixk++, xposk += xstride1, yposk += ystride1)
						{
							__copy(ksize2 * xstride2, xw, xposk, yw, yposk);
						}
					}
					else
					{
						const int copycount = (x2 - ix2) * xstride2;
						const int setcount = (ksize2 * xstride2) - copycount;
						for (; ixk < ixke && ixk < x1; ixk++, xposk += xstride1, yposk += ystride1)
						{
							__copy(copycount, xw, xposk, yw, yposk);
							__set(setcount, yw, yposk + copycount);
						}
					}

					if (ixk < ixke)
					{
						__set((ixke - ixk) * ksize2 * xstride2, yw, yposk);
					}

					ypos0 += ystride0;
				}
			}
		}
	}
	else
	{
		for (int ix0 = 0, xpos0 = (-kpadding1 * xstride1) + (-kpadding2 * xstride2), ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0)
		{
			for (int iy1 = 0, ix1 = -kpadding1, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, xpos1 += xstride1)
			{
				for (int iy2 = 0, ix2 = -kpadding2, xpos2 = xpos1; iy2 < y2; iy2++, ix2 += kstride2, xpos2 += xstride2)
				{
					// optimized version for BWHC layout
					// copy inner HC part of the tensor in one operation
					for (int ixk = ix1, ixke = ixk + ksize1, xposk = xpos2, yposk = ypos0;
						ixk < ixke;
						ixk++, xposk += xstride1, yposk += ystride1)
					{
						if (ixk >= 0 && ixk < x1)
						{
							int xposk2 = xposk;
							int yposk2 = yposk;

							int start = ix2;
							int end = ix2 + ksize2;

							if (start < 0)
							{
								const int count = -start * xstride2;
								__set(count, yw, yposk2);
								xposk2 += count;
								yposk2 += count;

								start = 0;
							}

#if 0
							if (start < x2)
							{
								const int count = (x2 - start) * xstride2;
								__copy(count, xw, xposk2, yw, yposk2);
								yposk2 += count;
							}

							if (end > x2)
							{
								__set((end - x2) * xstride2, yw, yposk2);
							}
#else
							if (end > x2)
							{
								__set((end - x2) * xstride2, yw, yposk2 + ((x2 - start) * xstride2));

								end = x2;
							}

							__copy((end - start) * xstride2, xw, xposk2, yw, yposk2);
#endif
						}
						else
						{
							__set(ksize2 * xstride2, yw, yposk);
						}
					}

					ypos0 += ystride0;
				}
			}
		}
	}
}

GENIXAPI(void, stack_kernels_gradient)(
	const int ksize1,
	const int ksize2,
	const int kstride1,
	const int kstride2,
	const int kpadding1,
	const int kpadding2,
	float* dxw,
	const int* xaxes,
	const int* xstrides,
	const float* dyw,
	const int* yaxes,
	const int* ystrides)
{
	const int x0 = xaxes[0];
	const int x1 = xaxes[1];
	const int x2 = xaxes[2];
	const int xstride0 = xstrides[0];
	const int xstride1 = xstrides[1];
	const int xstride2 = xstrides[2];
	const int y1 = yaxes[1];
	const int y2 = yaxes[2];
	const int ystride0 = ystrides[0];
	const int ystride1 = ystrides[1];

	for (int ix0 = 0, xpos0 = (-kpadding1 * xstride1) + (-kpadding2 * xstride2), ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0)
	{
		for (int iy1 = 0, ix1 = -kpadding1, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, xpos1 += xstride1)
		{
			for (int iy2 = 0, ix2 = -kpadding2, xpos2 = xpos1; iy2 < y2; iy2++, ix2 += kstride2, xpos2 += xstride2)
			{
				for (int ixk = ix1, ixke = ixk + ksize1, xposk = xpos2, yposk = ypos0;
					ixk < ixke;
					ixk++, xposk += xstride1, yposk += ystride1)
				{
					if (ixk >= 0 && ixk < x1)
					{
						int xposk2 = xposk;
						int yposk2 = yposk;

						int start = ix2;
						int end = ix2 + ksize2;

						if (start < 0)
						{
							int count = -start * xstride2;
							xposk2 += count;
							yposk2 += count;

							start = 0;
						}

						if (end > x2)
						{
							end = x2;
						}

						__add((end - start) * xstride2, dyw, yposk2, dxw, xposk2);
					}
				}

				ypos0 += ystride0;
			}
		}
	}
}
#endif
