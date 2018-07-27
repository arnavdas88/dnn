#include <math.h>

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
