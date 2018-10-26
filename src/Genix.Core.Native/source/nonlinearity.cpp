#include "stdafx.h"
#include "mkl.h"
#include "nonlinearity.inl"

GENIXAPI(void, relu)(
	int n,
	const float* x, int offx,
	float* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = __relu(x[i]);
	}
}

GENIXAPI(void, relu_gradient2)(
	int n,
	float* dx, int offdx, BOOL cleardx,
	const float* y, int offy,
	const float* dy, int offdy)
{
	dx += offdx;
	y += offy;
	dy += offdy;

	if (cleardx)
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] = __relu_derivative2(y[i]) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += __relu_derivative2(y[i]) * dy[i];
		}
	}
}

GENIXAPI(void, relu_gradient2_ip)(
	int n,
	float* dxy, int offdxy,
	const float* y, int offy)
{
	dxy += offdxy;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		dxy[i] *= __relu_derivative2(y[i]);
	}
}

GENIXAPI(void, sigmoid)(
	int n,
	const float* x, int offx,
	float* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = __sigmoid(x[i]);
	}
}

GENIXAPI(void, sigmoid_gradient2)(
	int n,
	float* dx, int offdx, BOOL cleardx,
	const float* y, int offy,
	const float* dy, int offdy)
{
	dx += offdx;
	y += offy;
	dy += offdy;

	if (cleardx)
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] = __sigmoid_derivative2(y[i]) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += __sigmoid_derivative2(y[i]) * dy[i];
		}
	}
}

GENIXAPI(void, _tanh)(
	int n,
	const float* x, int offx,
	float* y, int offy)
{
	::vsTanh(n, x + offx, y + offy);
}

GENIXAPI(void, tanh_gradient2)(
	int n,
	float* dx, int offdx, BOOL cleardx,
	const float* y, int offy,
	const float* dy, int offdy)
{
	dx += offdx;
	y += offy;
	dy += offdy;

	if (cleardx)
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] = __tanh_derivative2(y[i]) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += __tanh_derivative2(y[i]) * dy[i];
		}
	}
}

GENIXAPI(void, tanh_gradient2_ip)(
	int n,
	float* dxy, int offdxy,
	const float* y, int offy)
{
	dxy += offdxy;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		dxy[i] *= __tanh_derivative2(y[i]);
	}
}
