#include "stdafx.h"

#if 1
#include "daal.h"

using namespace std;
using namespace daal;
using namespace daal::algorithms;
using namespace daal::data_management;
using namespace daal::services;

/*const size_t samples = 4;
const size_t dimension = 3;
float inputArray[samples * dimension] =
{
	1.0, 2.0, 4.0,
	2.0, 13.0, 50.0,
	4.0, 23.0, 77.0,
	4.0, 23.0, 79.0,
};*/
#endif


GENIXAPI(void, kmeans)(int k, int iter, int dimension, int samples, float* x)
{
#if 1
	////k = 2;

	// Create data source
///	NumericTablePtr inputData = NumericTablePtr(new Matrix<float>(dimension, dimension, inputArray));
////	NumericTablePtr inputData = NumericTablePtr(new HomogenNumericTable<float>(inputArray, dimension, samples));
	NumericTablePtr inputData = NumericTablePtr(new HomogenNumericTable<float>(x, dimension, samples));

	// Get initial clusters for the K - Means algorithm
	kmeans::init::Batch<float, kmeans::init::plusPlusDense> init(k);

	init.input.set(kmeans::init::data, inputData);
	init.compute();

	NumericTablePtr centroids = init.getResult()->get(kmeans::init::centroids);

	// Create an algorithm object for the K-Means algorithm
	kmeans::Batch<> algorithm(k, iter);

	algorithm.input.set(kmeans::data, inputData);
	algorithm.input.set(kmeans::inputCentroids, centroids);

	algorithm.compute();

	// Print the clusterization results
	NumericTablePtr endCentroids = algorithm.getResult()->get(kmeans::centroids);

	size_t rows = endCentroids->getNumberOfRows();
	/*if (nRows != nClusters) {
		printf("ERROR!! first round of kmeans centroids rows dont match num clusters.\n");
	}*/

	//    size_t nCols = dataTable->getNumberOfColumns();
	BlockDescriptor<float> cent_block;
	endCentroids->getBlockOfRows(0, rows, readOnly, cent_block);
	float *centArray = cent_block.getBlockPtr();

	/*printNumericTable(algorithm.getResult()->get(kmeans::assignments), "First 10 cluster assignments:", 10);
	printNumericTable(algorithm.getResult()->get(kmeans::centroids), "First 10 dimensions of centroids:", 20, 10);
	printNumericTable(algorithm.getResult()->get(kmeans::objectiveFunction), "Objective function value:");*/
#endif
}
