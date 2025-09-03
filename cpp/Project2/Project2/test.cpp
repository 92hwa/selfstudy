#include <opencv2/opencv.hpp>
#include <iostream>

using namespace cv;
using namespace std;

int main() {
	Mat src = imread("D:\\workspace\\lenna.png", IMREAD_COLOR);
	if (src.empty()) { cout << "영상을 읽을 수 없음" << endl; }
	imshow("src", src);

	Mat gray, edge, output;
	cvtColor(src, gray, COLOR_BGR2GRAY);

	imshow("gray", gray);
	imwrite("D:\\gray.jpg", gray);

	waitKey(0);
	return 0;
}