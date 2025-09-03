#include <opencv2/opencv.hpp>
#include <iostream>

using namespace cv;
using namespace std;

int main() {
	Mat image;
	image = imread("D:\\workspace\\lenna.png", IMREAD_COLOR);
	if (image.empty()) { cout << "영상을 읽을 수 없음" << endl; }

	imshow("출력 영상", image);
	waitKey(0);
	return 0;
}