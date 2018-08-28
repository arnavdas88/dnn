#include "stdafx.h"
#include "mkl.h"
#include "nonlinearity.inl"

extern "C" __declspec(dllexport) void WINAPI lstm(
	int steps,
	int ylen,
	const float* u,
	float* g,
	float* s,
	float* y,
	float forgetBias,
	BOOL forward,
	BOOL rowmajor)
{
	const int glen = 4 * ylen;
	const int m = glen;
	const int n = ylen;
	const CBLAS_LAYOUT layout = rowmajor ? CblasRowMajor : CblasColMajor;
	const int lda = rowmajor ? n : m;

	int ginc = glen, yinc = ylen;
	if (!forward)
	{
		ginc = -glen; yinc = -ylen;

		ptrdiff_t tstart = ptrdiff_t(steps) - 1;
		g += tstart * glen;
		s += tstart * ylen;
		y += tstart * ylen;
	}

	for (int t = 0; t < steps; t++, g += ginc, s += yinc, y += yinc)
	{
		float* jg = g + ylen;
		float* fg = jg + ylen;
		float* og = fg + ylen;

		if (t == 0)
		{
			for (int i = 0; i < ylen; i++)
			{
				const float igate = g[i] = __sigmoid(g[i]);
				const float jgate = jg[i] = ::tanhf(jg[i]);
				const float fgate = fg[i] = __sigmoid(fg[i] + forgetBias);
				const float ogate = og[i] = __sigmoid(og[i]);

				s[i] = igate * jgate;
				y[i] = ogate * ::tanhf(s[i]);
			}
		}
		else
		{
			::cblas_sgemv(layout, CblasNoTrans, m, n, 1.0f, u, lda, y - yinc, 1, 1.0f, g, 1);

			const float* sprev = s - ylen;
			for (int i = 0; i < ylen; i++)
			{
				const float igate = g[i] = __sigmoid(g[i]);
				const float jgate = jg[i] = ::tanhf(jg[i]);
				const float fgate = fg[i] = __sigmoid(fg[i] + forgetBias);
				const float ogate = og[i] = __sigmoid(og[i]);

				s[i] = (fgate * sprev[i]) + (igate * jgate);
				y[i] = ogate * ::tanhf(s[i]);
			}
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI lstm_gradient(
	int steps,
	int ylen,
	const float* u,
	float* du,
	const float* g,
	float* dg,
	const float* s,
	float* ds,
	const float* y,
	float* dy,
	BOOL forward,
	BOOL rowmajor)
{
	const int glen = 4 * ylen;
	const int m = glen;
	const int n = ylen;
	const CBLAS_LAYOUT layout = rowmajor ? CblasRowMajor : CblasColMajor;
	const int lda = rowmajor ? n : m;

	int ginc = glen, yinc = ylen;
	if (forward)
	{
		ptrdiff_t tstart = ptrdiff_t(steps) - 1;
		g += tstart * glen;
		dg += tstart * glen;
		s += tstart * ylen;
		ds += tstart * ylen;
		y += tstart * ylen;
		dy += tstart * ylen;
	}
	else
	{
		ginc = -glen; yinc = -ylen;
	}

	for (int t = steps - 1; t >= 0; t--, g -= ginc, dg -= ginc, s -= yinc, ds -= yinc, y -= yinc, dy -= yinc)
	{
		const float* jg = g + ylen;
		const float* fg = jg + ylen;
		const float* og = fg + ylen;

		float* djg = dg + ylen;
		float* dfg = djg + ylen;
		float* dog = dfg + ylen;

		if (t == 0)
		{
			for (int i = 0; i < ylen; i++)
			{
				const float igate = g[i];
				const float jgate = jg[i];
				const float fgate = fg[i];
				const float ogate = og[i];

				const float dyi = dy[i];
				const float sai = ::tanhf(s[i]);

				// output gate
				dog[i] = dyi * sai * __sigmoid_derivative2(ogate);

				// cell state gradient
				ds[i] += dyi * ogate * __tanh_derivative2(sai);
				const float dsi = ds[i];

				// input gate
				dg[i] = dsi * jgate * __sigmoid_derivative2(igate);

				// new input gate
				djg[i] = dsi * igate * __tanh_derivative2(jgate);

				// forget gate
				dfg[i] = 0;
			}
		}
		else
		{
			const float* sprev = s - yinc;
			float* dsprev = ds - yinc;

			for (int i = 0; i < ylen; i++)
			{
				const float igate = g[i];
				const float jgate = jg[i];
				const float fgate = fg[i];
				const float ogate = og[i];

				const float dyi = dy[i];
				const float sai = ::tanhf(s[i]);

				// output gate
				dog[i] = dyi * sai * __sigmoid_derivative2(ogate);

				// cell state gradient
				ds[i] += dyi * ogate * __tanh_derivative2(sai);
				const float dsi = ds[i];

				// input gate
				dg[i] = dsi * jgate * __sigmoid_derivative2(igate);

				// new input gate
				djg[i] = dsi * igate * __tanh_derivative2(jgate);

				// forget gate
				dfg[i] = dsi * sprev[i] * __sigmoid_derivative2(fgate);

				// previous cell state
				dsprev[i] += dsi * fgate;
			}

			::cblas_sger(layout, m, n, 1.0f, dg, 1, y - yinc, 1, du, lda);
			::cblas_sgemv(layout, CblasTrans, m, n, 1.0f, u, lda, dg, 1, 1.0f, dy - yinc, 1);
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI gru(
	const int steps,
	const int ystep,
	const float* u,
	float* g,
	float* y,
	const BOOL bidirectional,
	const BOOL rowmajor)
{
	const int hstep = bidirectional ? ystep >> 1 : ystep;
	const int gstep = 3 * ystep;
	const int m = 2 * hstep;
	const int n = hstep;
	const CBLAS_LAYOUT layout = rowmajor ? CblasRowMajor : CblasColMajor;
	const int ldu = rowmajor ? n : gstep;
	const float* uc = u + (rowmajor ? m * n : m);

	for (int t = 0; t < steps; t++, y += ystep, g += gstep)
	{
		float* rg = g + hstep;		// reset gate
		float* cg = rg + hstep;		// candidate

		if (t == 0)
		{
			for (int i = 0; i < hstep; i++)
			{
				g[i] = __sigmoid(g[i]);
				rg[i] = __sigmoid(rg[i]);
				cg[i] = ::tanhf(cg[i]);

				y[i] = g[i] * cg[i];
			}
		}
		else
		{
			const float* state = y - ystep;
			::cblas_sgemv(layout, CblasNoTrans, m, n, 1.0f, u, ldu, state, 1, 1.0f, g, 1);

			for (int i = 0; i < hstep; i++)
			{
				g[i] = __sigmoid(g[i]);
				rg[i] = __sigmoid(rg[i]);

				// use y as a temporary buffer for r * state
				y[i] = rg[i] * state[i];
			}

			::cblas_sgemv(layout, CblasNoTrans, n, n, 1.0f, uc, ldu, y, 1, 1.0f, cg, 1);

			for (int i = 0; i < hstep; i++)
			{
				cg[i] = ::tanhf(cg[i]);

				const float s = state[i];
				y[i] = s + (g[i] * (cg[i] - s));
			}
		}
	}

	// backward pass
	if (bidirectional)
	{
		y -= ystep / 2;
		g -= gstep / 2;

		int off = rowmajor ? (m + n) * n : m + n;
		u += off;
		uc += off;

		for (int t = steps - 1; t >= 0; t--, y -= ystep, g -= gstep)
		{
			float* rg = g + hstep;		// reset gate
			float* cg = rg + hstep;		// candidate

			if (t == steps - 1)
			{
				for (int i = 0; i < hstep; i++)
				{
					g[i] = __sigmoid(g[i]);
					rg[i] = __sigmoid(rg[i]);
					cg[i] = ::tanhf(cg[i]);

					y[i] = g[i] * cg[i];
				}
			}
			else
			{
				const float* state = y + ystep;
				::cblas_sgemv(layout, CblasNoTrans, m, n, 1.0f, u, ldu, state, 1, 1.0f, g, 1);

				for (int i = 0; i < hstep; i++)
				{
					g[i] = __sigmoid(g[i]);
					rg[i] = __sigmoid(rg[i]);

					// use y as a temporary buffer for r * state
					y[i] = rg[i] * state[i];
				}

				::cblas_sgemv(layout, CblasNoTrans, n, n, 1.0f, uc, ldu, y, 1, 1.0f, cg, 1);

				for (int i = 0; i < hstep; i++)
				{
					cg[i] = ::tanhf(cg[i]);

					const float s = state[i];
					y[i] = s + (g[i] * (cg[i] - s));
				}
			}
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI gru_gradient(
	const int steps,
	const int ystep,
	const float* u,
	float* du,
	const float* g,
	float* dg,
	const float* y,
	float* dy,
	const BOOL bidirectional,
	const BOOL rowmajor)
{
	const int hstep = bidirectional ? ystep >> 1 : ystep;
	const int gstep = 3 * ystep;
	const int m = 2 * hstep;
	const int n = hstep;
	const CBLAS_LAYOUT layout = rowmajor ? CblasRowMajor : CblasColMajor;
	const int ldu = rowmajor ? n : gstep;
	const float* uc = u + (rowmajor ? m * n : m);
	float* duc = du + (rowmajor ? m * n : m);

	ptrdiff_t tstart = ptrdiff_t(steps) - 1;
	g += tstart * gstep;
	dg += tstart * gstep;
	y += tstart * ystep;
	dy += tstart * ystep;

	for (int t = steps - 1; t >= 0; t--, y -= ystep, dy -= ystep, g -= gstep, dg -= gstep)
	{
		const float* rg = g + hstep;		// reset gate
		float* drg = dg + hstep;

		const float* cg = rg + hstep;		// candidate
		float* dcg = drg + hstep;

		if (t == 0)
		{
			for (int i = 0; i < hstep; i++)
			{
				const float ugate = g[i];
				const float candidate = cg[i];
				const float dyi = dy[i];

				// update gate
				dg[i] = dyi * candidate * __sigmoid_derivative2(ugate);

				// reset gate
				drg[i] = 0;

				// candidate
				dcg[i] = dyi * ugate * __tanh_derivative2(candidate);
			}
		}
		else
		{
			const float* state = y - ystep;
			float* dstate = dy - ystep;

			for (int i = 0; i < hstep; i++)
			{
				const float ugate = g[i];
				const float candidate = cg[i];
				const float dyi = dy[i];

				// update gate
				dg[i] = dyi * (candidate - state[i]) * __sigmoid_derivative2(ugate);

				// calculate r * state again
				// we will need it to update candidate weights
				// use drg as a temporary buffer
				drg[i] = rg[i] * state[i];

				// candidate
				dcg[i] = dyi * ugate * __tanh_derivative2(candidate);

				// state gradient
				dstate[i] += dyi * (1.0f - ugate);
			}

			::cblas_sger(layout, n, n, 1.0f, dcg, 1, drg, 1, duc, ldu);
			// use drg as a temporary buffer for d(r * state)
			::cblas_sgemv(layout, CblasTrans, n, n, 1.0f, uc, ldu, dcg, 1, 0.0f, drg, 1);

			// update state gradient and calculate reset gate
			for (int i = 0; i < hstep; i++)
			{
				// state gradient
				dstate[i] += drg[i] * rg[i];

				// reset gate
				drg[i] = drg[i] * state[i] * __sigmoid_derivative2(rg[i]);
			}

			::cblas_sger(layout, m, n, 1.0f, dg, 1, state, 1, du, ldu);
			::cblas_sgemv(layout, CblasTrans, m, n, 1.0f, u, ldu, dg, 1, 1.0f, dstate, 1);
		}
	}

	// backward pass
	if (bidirectional)
	{
		int off = ystep + (ystep / 2);
		y += off;
		dy += off;

		off = gstep + (gstep / 2);
		g += off;
		dg += off;

		off = rowmajor ? (m + n) * n : m + n;
		u += off;
		du += off;
		uc += off;
		duc += off;

		for (int t = 0; t < steps; t++, y += ystep, dy += ystep, g += gstep, dg += gstep)
		{
			const float* rg = g + hstep;		// reset gate
			float* drg = dg + hstep;

			const float* cg = rg + hstep;		// candidate
			float* dcg = drg + hstep;

			if (t == steps - 1)
			{
				for (int i = 0; i < hstep; i++)
				{
					const float ugate = g[i];
					const float candidate = cg[i];
					const float dyi = dy[i];

					// update gate
					dg[i] = dyi * candidate * __sigmoid_derivative2(ugate);

					// reset gate
					drg[i] = 0;

					// candidate
					dcg[i] = dyi * ugate * __tanh_derivative2(candidate);
				}
			}
			else
			{
				const float* state = y + ystep;
				float* dstate = dy + ystep;

				for (int i = 0; i < hstep; i++)
				{
					const float ugate = g[i];
					const float candidate = cg[i];
					const float dyi = dy[i];

					// update gate
					dg[i] = dyi * (candidate - state[i]) * __sigmoid_derivative2(ugate);

					// calculate r * state again
					// we will need it to update candidate weights
					// use drg as a temporary buffer
					drg[i] = rg[i] * state[i];

					// candidate
					dcg[i] = dyi * ugate * __tanh_derivative2(candidate);

					// state gradient
					dstate[i] += dyi * (1.0f - ugate);
				}

				::cblas_sger(layout, n, n, 1.0f, dcg, 1, drg, 1, duc, ldu);
				// use drg as a temporary buffer for d(r * state)
				::cblas_sgemv(layout, CblasTrans, n, n, 1.0f, uc, ldu, dcg, 1, 0.0f, drg, 1);

				// update state gradient and calculate reset gate
				for (int i = 0; i < hstep; i++)
				{
					// state gradient
					dstate[i] += drg[i] * rg[i];

					// reset gate
					drg[i] = drg[i] * state[i] * __sigmoid_derivative2(rg[i]);
				}

				::cblas_sger(layout, m, n, 1.0f, dg, 1, state, 1, du, ldu);
				::cblas_sgemv(layout, CblasTrans, m, n, 1.0f, u, ldu, dg, 1, 1.0f, dstate, 1);
			}
		}
	}
}
