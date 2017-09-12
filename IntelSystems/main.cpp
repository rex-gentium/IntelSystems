#include <iostream>
#include "EasyBMP.h"
#include "Sample.h"

using namespace std;
/* COLOR LOGIC */
const RGBApixel floodColor = { 255, 0, 0, 0 };
const RGBApixel eraserColor = { 0, 255, 0, 0 };
const RGBApixel whiteColor = { 255, 255, 255, 0 };
const RGBApixel blackColor = { 0, 0, 0, 0 };
bool isOfColor(const RGBApixel pixel, const RGBApixel color);
/* CIRCUIT LOGIC */
bool checkCircuit(BMP image);
void tryFlood(BMP& image, int hor, int ver);
/* ANGLE LOGIC */
Sample getSample(BMP image, int hor, int ver);
void eraseSample(BMP& image, int hor, int ver);
bool checkLines(BMP image);
bool checkAngle(Sample s);
int countAngles(BMP image);

bool checkBlank(BMP image);
void printImage(BMP image);
void printResult(bool blank, bool circuit, int angleCount);

int main() {
	BMP image;
	image.ReadFromFile("D:\\image.bmp");
	printImage(image);
	bool isBlank = checkBlank(image);
	if (isBlank)
		printResult(isBlank, false, 0);
	bool hasCircuit = checkCircuit(image);
	//bool hasLine = checkLines(image);
	int angleCount = countAngles(image);
	printResult(isBlank, hasCircuit, angleCount);
	system("PAUSE");
	return 0;
}

bool checkCircuit(BMP image)
{
	BMP img(image);
	// try flood from outside of the picture
	for (int hor = 0; hor < img.TellWidth(); hor++) {
		tryFlood(img, hor, 0); // first row
		tryFlood(img, hor, img.TellHeight() - 1); // last row
	}
	for (int ver = 0; ver < image.TellHeight(); ver++) {
		tryFlood(img, 0, ver); // first col
		tryFlood(img, img.TellWidth() - 1, ver); // last col
	}
	// check if any white pixel left
	for (int ver = 0; ver < img.TellHeight(); ver++)
		for (int hor = 0; hor < img.TellWidth(); hor++)
			if (isOfColor(*img(hor, ver), whiteColor)) {
				printImage(img);
				return true;
			}
	printImage(img);
	return false;
}

void tryFlood(BMP& image, int hor, int ver) {
	if (hor < 0 || ver < 0 || hor >= image.TellWidth() || ver >= image.TellHeight())
		return; // out of bounds
	RGBApixel p = image.GetPixel(hor, ver);
	if (isOfColor(p, floodColor))
		return; // already flooded this place
	if (isOfColor(p, blackColor))
		return; // can't flood through black
	image.SetPixel(hor, ver, floodColor);
	tryFlood(image, hor + 1, ver);
	tryFlood(image, hor - 1, ver);
	tryFlood(image, hor, ver + 1);
	tryFlood(image, hor, ver - 1);
}

Sample getSample(BMP image, int hor, int ver)
{
	Sample res;
	for (int i = 0; i < SAMPLE_SIZE; ++i)
		for (int j = 0; j < SAMPLE_SIZE; ++j) {
			RGBApixel p = image.GetPixel(hor + j, ver + i);
			if (isOfColor(p, whiteColor))
				res.val[i][j] = WHITE;
			else if (isOfColor(p, blackColor))
				res.val[i][j] = BLACK;
			else res.val[i][j] = UNKNOWN;
		}
	return res;
}

void eraseSample(BMP &image, int hor, int ver)
{
	for (int i = 0; i < SAMPLE_SIZE; ++i)
		for (int j = 0; j < SAMPLE_SIZE; ++j)
			image.SetPixel(hor + j, ver + i, whiteColor);
	printImage(image);
}

bool checkLines(BMP image)
{
	int heightLimit = image.TellHeight() - (SAMPLE_SIZE - 1);
	int widthLimit = image.TellWidth() - (SAMPLE_SIZE - 1);
	for (int ver = 0; ver < heightLimit; ++ver)
		for (int hor = 0; hor < widthLimit; ++hor) {
			Sample s = getSample(image, hor, ver);
			for each (Sample lineSample in lineSamples)
				if (matchesSample(s, lineSample))
					return true;
		}	
	return false;
}

bool checkAngle(Sample s)
{
	for each (Sample angleSample in angleSamples)
		if (matchesSample(s, angleSample))
			return true;
	return false;
}

int countAngles(BMP image)
{
	int res = 0;
	BMP img(image);
	int heightLimit = image.TellHeight() - (SAMPLE_SIZE - 1);
	int widthLimit = image.TellWidth() - (SAMPLE_SIZE - 1);
	for (int ver = 0; ver < heightLimit; ++ver)
		for (int hor = 0; hor < widthLimit; ++hor) {
			Sample s = getSample(img, hor, ver);
			bool hasAngle = checkAngle(s);
			if (hasAngle) {
				res++;
				eraseSample(img, hor, ver);
			}
		}
	return res;
}

bool checkBlank(BMP image)
{
	for (int ver = 0; ver < image.TellHeight(); ++ver)
		for (int hor = 0; hor < image.TellWidth(); ++hor)
			if (isOfColor(*image(hor, ver), blackColor))
				return false;
	return true;
}

void printImage(BMP image)
{
	for (int ver = 0; ver < image.TellHeight(); ++ver) {
		for (int hor = 0; hor < image.TellWidth(); ++hor) {
			RGBApixel p = image.GetPixel(hor, ver);
			if (isOfColor(p, whiteColor))
				cout << WHITE;
			else if (isOfColor(p, blackColor))
				cout << BLACK;
			else if (isOfColor(p, eraserColor))
				cout << ERASED;
			else cout << UNKNOWN;
		}
		cout << endl;
	}
	cout << endl;
}

void printResult(bool blank, bool circuit, int angleCount)
{
	if (blank) {
		cout << "The image is BLANK" << endl;
		return;
	}
	cout << "Figure type is ";
	if (circuit)
		switch (angleCount) {
		case 0: cout << "ELLIPSE"; break;
		case 3: cout << "TRIANGLE"; break;
		case 4: cout << "RECTANGLE"; break;
		default: cout << "UNKNOWN";
		}
	else 
		switch (angleCount) {
		case 0: cout << "LINE or CURVE"; break;
		default: cout << "POLYLINE or POLYCURVE";
		}
	cout << endl;
}

bool isOfColor(const RGBApixel pixel, const RGBApixel color) {
	return pixel.Red == color.Red 
		&& pixel.Green == color.Green
		&& pixel.Blue == color.Blue
		&& pixel.Alpha == color.Alpha;
}