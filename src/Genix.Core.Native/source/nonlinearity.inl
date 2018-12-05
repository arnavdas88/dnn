#include <cmath>

float __forceinline __relu(float x)
{
	return x <= 0.0f ? 0.0f : x;
}

float __forceinline __relu_derivative2(float y)
{
	return y <= 0.0f ? 0.0f : 1.0f;
}

float __forceinline __tanh_derivative2(float y)
{
	return 1.0f - (y * y);
}

float __forceinline __sigmoid(float x)
{
	return 1.0f / (1.0f + ::expf(-x));
}

float __forceinline __sigmoid_derivative2(float y)
{
	return y * (1.0f - y);
}

template <float _Func(float)>
void __forceinline __nonlinearity(const int start, const int end, const float* x, float* y)
{
	for (int i = start; i < end; i++)
	{
		y[i] = _Func(x[i]);
	}
}

template <float _Func(float)>
void __forceinline __nonlinearity_gradient2(const int start, const int end, float* dx, BOOL cleardx, const float* y, const float* dy)
{
	if (cleardx)
	{
		for (int i = start; i < end; i++)
		{
			dx[i] = _Func(y[i]) * dy[i];
		}
	}
	else
	{
		for (int i = start; i < end; i++)
		{
			dx[i] += _Func(y[i]) * dy[i];
		}
	}
}

template <float _Func(float)>
void __forceinline __nonlinearity_gradient2_ip(const int start, const int end, float* dxy, const float* y)
{
	for (int i = start; i < end; i++)
	{
		dxy[i] *= _Func(y[i]);
	}
}
