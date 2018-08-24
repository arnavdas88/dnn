#include "leptonica/src/allheaders.h"
#include "tesseract/src/api/baseapi.h"

void temp()
{
	tesseract::TessBaseAPI* api = new tesseract::TessBaseAPI();

	int rc = api->Init("path", "eng");
	if (rc == -1)
	{
		////throw gcnew InvalidOperationException("Could not initialize TesseractOCR");
	}

	////api->SetVariable("classify_enable_learning", "0");
	////api->SetVariable("classify_enable_adaptive_matcher", "0");

}