#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

#define WIND_WIDTH  64  /* detection window width  */
#define WIND_HEIGHT 128 /* detection window image height */

GENIXAPI(int, hog)(
	const int bitsPerPixel,
	const int width, const int height, const int stride, const unsigned __int64* src)
{
	IppStatus status = ippStsNoErr;


	int hogCtxSize = 0, hogBuffSize = 0, winBuffSize = 0;
	int srcStep = 0;		// Steps, in bytes, through the source/destination/feature images
	IppiHOGSpec* pHOGctx = NULL;
	Ipp8u* pBuffer = NULL;
	Ipp32f* pDescriptor = NULL;
	const Ipp8u borderValue = 255;

	IppiSize roiSize = { width, height };
	IppiHOGConfig config =
	{
		1,			/* Use OpenCV compatible output format */
		8,			/* Cell size (pixels) */
		16,			/* Block size (pixels) */
		8,			/* Block stride size (pixels) */
		9,			/* Number of bins (orientations) in the histogram */
		1.0f,		/* Sigma value (in the applied Gaussian) */
		0.2f,		/* Threshold value (applied for normalization) */
		{ WIND_WIDTH, WIND_HEIGHT },	/* Size (width and height in pixels) of detection window */
	};

	int numLocsx = (width + WIND_WIDTH - 1) / WIND_WIDTH;
	int numLocsy = (height + WIND_HEIGHT - 1) / WIND_HEIGHT;
	int numLocs = numLocsx * numLocsy;
	IppiPoint* loc = (IppiPoint*)ippsMalloc_8u(numLocs * sizeof(IppiPoint));
	if (loc == NULL) { status = ippStsNoMemErr; }
	check_sts(status)

		for (int y = 0, ypos = 0, i = 0; y < numLocsy; y++, ypos += WIND_HEIGHT)
		{
			for (int x = 0, xpos = 0; x < numLocsx; x++, xpos += WIND_WIDTH)
			{
				loc[i++] = { xpos, ypos };
			}
		}

	/* Get size of HOG context */
	check_sts(status = ippiHOGGetSize(&config, &hogCtxSize));

	/* Initialize HOG context */
	pHOGctx = (IppiHOGSpec*)ippsMalloc_8u(hogCtxSize);
	check_sts(status = ippiHOGInit(&config, pHOGctx));

	/* Compute the temporary work buffer size */
	check_sts(status = ippiHOGGetBufferSize(pHOGctx, roiSize, &hogBuffSize));
	pBuffer = ippsMalloc_8u(hogBuffSize);

	/* Compute size of HOG descriptor */
	check_sts(status = ippiHOGGetDescriptorSize(pHOGctx, &winBuffSize));
	pDescriptor = (Ipp32f*)ippsMalloc_8u(numLocs*winBuffSize);

	/* Compute HOG descriptor */
	check_sts(status = ippiHOG_8u32f_C1R(
		(const Ipp8u *)src,
		stride * sizeof(unsigned __int64),
		roiSize,
		loc,
		numLocs,
		pDescriptor,
		pHOGctx,
		ippBorderConst,
		borderValue,
		pBuffer));

	EXIT_MAIN
		ippsFree(loc);
	ippsFree(pHOGctx);
	ippsFree(pBuffer);
	ippsFree(pDescriptor);
	return (int)status;
}
