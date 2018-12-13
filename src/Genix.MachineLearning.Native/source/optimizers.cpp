#include "stdafx.h"
#include <cmath>

GENIXAPI(void, adadelta)(const int n, float* gradient, float* gsum, float* xsum, const float rho, const float eps)
{
	const float rhoinv = 1.0f - rho;

	for (int i = 0; i < n; i++)
	{
		float g = gradient[i];

		gsum[i] = (rho * gsum[i]) + (rhoinv * g * g);
		g *= -::sqrt((xsum[i] + eps) / (gsum[i] + eps));
		xsum[i] = (rho * xsum[i]) + (rhoinv * g * g); // yes, xsum lags behind gsum by 1.

		gradient[i] = g;
	}
}