#include "stdafx.h"
#include "nonlinearity.inl"
#include "parallel.inl"
#include "mkl.h"

/*#include <amp.h>
#include <amp_math.h>
using namespace concurrency;*/

GENIXAPI(void, relu)(
	int n,
	const float* x, int offx,
	float* y, int offy)
{
	const int Partition = 65536;

	x += offx;
	y += offy;

	parallel(n, Partition, [&](int start, int end) {

		__nonlinearity<__relu>(start, end, x, y);
	});
}

GENIXAPI(void, relu_gradient2)(
	int n,
	float* dx, int offdx, BOOL cleardx,
	const float* y, int offy,
	const float* dy, int offdy)
{
	const int Partition = 65536;

	dx += offdx;
	y += offy;
	dy += offdy;

	parallel(n, Partition, [&](int start, int end) {

		__nonlinearity_gradient2<__relu_derivative2>(start, end, dx, cleardx, y, dy);
	});
}

GENIXAPI(void, relu_gradient2_ip)(
	int n,
	float* dxy, int offdxy,
	const float* y, int offy)
{
	const int Partition = 65536;

	dxy += offdxy;
	y += offy;

	parallel(n, Partition, [&](int start, int end) {

		__nonlinearity_gradient2_ip<__relu_derivative2>(start, end, dxy, y);
	});
}

GENIXAPI(void, sigmoid)(
	int n,
	const float* x, int offx,
	float* y, int offy)
{
	const int Partition = 65536;

	x += offx;
	y += offy;

	parallel(n, Partition, [&](int start, int end) {

		__nonlinearity<__sigmoid>(start, end, x, y);
	});
}

GENIXAPI(void, sigmoid_gradient2)(
	int n,
	float* dx, int offdx, BOOL cleardx,
	const float* y, int offy,
	const float* dy, int offdy)
{
	const int Partition = 65536;

	dx += offdx;
	y += offy;
	dy += offdy;

	parallel(n, Partition, [&](int start, int end) {

		__nonlinearity_gradient2<__sigmoid_derivative2>(start, end, dx, cleardx, y, dy);
	});
}

GENIXAPI(void, sigmoid_gradient2_ip)(
	int n,
	float* dxy, int offdxy,
	const float* y, int offy)
{
	const int Partition = 65536;

	dxy += offdxy;
	y += offy;

	parallel(n, Partition, [&](int start, int end) {

		__nonlinearity_gradient2_ip<__sigmoid_derivative2>(start, end, dxy, y);
	});
}

GENIXAPI(void, _tanh)(
	int n,
	const float* x, int offx,
	float* y, int offy)
{
	const int Partition = 65536;

	x += offx;
	y += offy;

	parallel(n, Partition, [&](int start, int end) {

		::vsTanh(end - start, x + start, y + start);
	});

	/*concurrency::array_view<const float, 1> ax(n, x);
	concurrency::array_view<float, 1> ay(n, y);
	ay.discard_data();

	concurrency::parallel_for_each(
		// Define the compute domain, which is the set of threads that are created.
		ay.extent,
		// Define the code to run on each thread on the accelerator.
		[=](concurrency::index<1> idx) restrict(amp)
		{
			ay[idx] = concurrency::fast_math::tanhf(ax[idx]);
		}
	);*/
}

GENIXAPI(void, tanh_gradient2)(
	int n,
	float* dx, int offdx, BOOL cleardx,
	const float* y, int offy,
	const float* dy, int offdy)
{
	const int Partition = 65536;

	dx += offdx;
	y += offy;
	dy += offdy;

	parallel(n, Partition, [&](int start, int end) {

		__nonlinearity_gradient2<__tanh_derivative2>(start, end, dx, cleardx, y, dy);
	});
	/*if (cleardx)
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
	}*/
}

GENIXAPI(void, tanh_gradient2_ip)(
	int n,
	float* dxy, int offdxy,
	const float* y, int offy)
{
	const int Partition = 65536;

	dxy += offdxy;
	y += offy;

	parallel(n, Partition, [&](int start, int end) {

		__nonlinearity_gradient2_ip<__tanh_derivative2>(start, end, dxy, y);
	});
}
