# rawEx01

* RAW 파일 정보 읽기
    * OpenFileDialog 메소드를 이용한 파일탐색기 열기
    * ushort buffer16 배열에 픽셀 값 저장
* RAW 파일 띄우기
    * ushort buffer16 에 있는 픽셀 값을 읽어 8비트 shift 연산을 통해 byte buffer8 배열에 저장
    * WriteableBitmap 메소드를 이용해 Image 컨트롤에 buffer8 배열을 인자로 사용해 띄우기
* RAW 파일의 Histogram 그리기
    * 진행 중 ... 
* RAW 파일의 LUT 적용 
    * 진행 중 ...