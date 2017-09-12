#include "Sample.h"

bool matchesSample(Sample sample, Sample ethalon)
{
	for (int i = 0; i < SAMPLE_SIZE; ++i)
		for (int j = 0; j < SAMPLE_SIZE; ++j)
			if (sample.val[i][j] != ethalon.val[i][j])
				return false;
	return true;
}
