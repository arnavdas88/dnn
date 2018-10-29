#include "stdafx.h"

extern "C" __declspec(dllimport) void WINAPI set_f32(int n, float a, float* y, int offy);
extern "C" __declspec(dllimport) void WINAPI copy_f32(int n, const float* x, int offx, float* y, int offy);

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
							set_f32(count, 0.0f, yw, yposk2);
							xposk2 += count;
							yposk2 += count;

							start = 0;
						}

						if (end > x2)
						{
							set_f32((end - x2) * xstride2, 0.0f, yw, yposk2 + ((x2 - start) * xstride2));

							end = x2;
						}

						copy_f32((end - start) * xstride2, xw, xposk2, yw, yposk2);
					}
					else
					{
						set_f32(ksize2 * xstride2, 0.0f, yw, yposk);
					}
				}

				ypos0 += ystride0;
			}
		}
	}
}
