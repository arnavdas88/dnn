#include "stdafx.h"

#include <math.h> 

constexpr float __forceinline relu(float x)
{
	return x <= 0.0f ? 0.0f : x;
}

constexpr float __forceinline relu_derivative2(float y)
{
	return y <= 0.0f ? 0.0f : 1.0f;
}

constexpr float __forceinline tanh_derivative2(float y)
{
	return 1.0f - (y * y);
}

float __forceinline sigmoid(float x)
{
	return 1.0f / (1.0f + ::expf(-x));
}

constexpr float __forceinline sigmoid_derivative2(float y)
{
	return y * (1.0f - y);
}

extern "C" __declspec(dllexport) void WINAPI relu(int n, const float* x, int offx, float* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = relu(x[i]);
	}
}

extern "C" __declspec(dllexport) void WINAPI relu_derivative2_grad(
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
			dx[i] = relu_derivative2(y[i]) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += relu_derivative2(y[i]) * dy[i];
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI tanh_derivative2_grad(
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
			dx[i] = tanh_derivative2(y[i]) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += tanh_derivative2(y[i]) * dy[i];
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI tanh_derivative2_grad_ip(
	int n,
	const float* x, int offx,
	float* dx, int offdx)
{
	x += offx;
	dx += offdx;

	for (int i = 0; i < n; i++)
	{
		dx[i] *= tanh_derivative2(x[i]);
	}
}

extern "C" __declspec(dllexport) void WINAPI sigmoid(int n, const float* x, int offx, float* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = sigmoid(x[i]);
	}
}

extern "C" __declspec(dllexport) void WINAPI sigmoid_derivative2_grad(
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
			dx[i] = sigmoid_derivative2(y[i]) * dy[i];
		}
	}
	else
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += sigmoid_derivative2(y[i]) * dy[i];
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI addc(int n, float a, float* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] += a;
	}
}

extern "C" __declspec(dllexport) void WINAPI addxc(int n, const float* x, int offx, float a, float* y, int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = x[i] + a;
	}
}

extern "C" __declspec(dllexport) void WINAPI mulc(int n,
	const float* x, int offx, int incx,
	float a,
	float* y, int offy, int incy)
{
	x += offx;
	y += offy;

	if (incx == 1 && incy == 1)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = x[i] * a;
		}
	}
	else
	{
		for (int i = 0; i < n; i++, x += incx, y += incy)
		{
			*y = *x * a;
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI matchandadd(
	int n,
	const float* x, const float* xmask, int offx,
	float* y, const float* ymask, int offy)
{
	x += offx;
	y += offy;
	xmask += offx;
	ymask += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] += (xmask[i] == ymask[i] ? 1.0f : 0.0f) * x[i];
	}
}

extern "C" __declspec(dllexport) int WINAPI fcompare(int n,
	const float* x, int offx,
	const float* y, int offy)
{
	return ::memcmp(x + offx, y + offy, n * sizeof(float));
}

extern "C" __declspec(dllexport) int WINAPI icompare(int n,
	const int* x, int offx,
	const int* y, int offy)
{
	return ::memcmp(x + offx, y + offy, n * sizeof(int));
}

extern "C" __declspec(dllexport) int WINAPI ccompare(int n,
	const wchar_t* x, int offx,
	const wchar_t* y, int offy)
{
	return ::memcmp(x + offx, y + offy, n * sizeof(wchar_t));
}

extern "C" __declspec(dllexport) void WINAPI pow_derivative(
	int n,
	const float* x, int offx,
	float power,
	const float* dy, int offdy,
	float* dx, int offdx)
{
	x += offx;
	dy += offdy;
	dx += offdx;

	if (power == 2.0f)
	{
		for (int i = 0; i < n; i++)
		{
			dx[i] += 2.0f * x[i] * dy[i];
		}
	}
	else
	{
		const float p = power - 1.0f;
		for (int i = 0; i < n; i++)
		{
			dx[i] += power * ::powf(x[i], p) * dy[i];
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI abs_derivative(
	int n,
	const float* x, float* dx, int offx,
	const float* y, const float* dy, int offy)
{
	x += offx;
	dx += offx;
	y += offy;
	dy += offy;

	for (int i = 0; i < n; i++)
	{
		dx[i] = (x[i] == y[i] ? 1.0f : -1.0f) * dy[i];
	}
}
