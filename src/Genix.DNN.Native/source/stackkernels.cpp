#include "stdafx.h"

void __forceinline __set(int n, float* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = 0;
	}
}
void __forceinline __copy(int n, const float* x, int offx, float* y, int offy)
{
	::memcpy(y + offy, x + offx, n * sizeof(float));
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

GENIXAPI(void, stack_kernels)(
	const int ksize1,
	const int ksize2,
	const int kstride1,
	const int kstride2,
	const int kpadding1,
	const int kpadding2,
	const float* xw,
	const int x0,
	const int x1,
	const int x2,
	const int xstride0,
	const int xstride1,
	const int xstride2,
	float* yw,
	const int y1,
	const int y2,
	const int ystride0,
	const int ystride1)
{
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
	const int x0,
	const int x1,
	const int x2,
	const int xstride0,
	const int xstride1,
	const int xstride2,
	const float* dyw,
	const int y1,
	const int y2,
	const int ystride0,
	const int ystride1)
{
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
