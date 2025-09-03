#include <opencv2/opencv.hpp>
#include <iostream>

using namespace cv;
using namespace std;

int main() {
	Mat src = imread("D:\\workspace\\selfstudy\\cpp\\lenna.png", IMREAD_COLOR);
	if (src.empty()) { cout << "영상을 읽을 수 없음" << endl; }
	imshow("src", src);

	Mat dst, output;
	flip(src, output, 1);

	imshow("flip", output);
	imwrite("D:\\workspace\\selfstudy\\cpp\\Project3\\flip.png", output);

	waitKey(0);
	return 0;
}