# wpfEx01

Code-behind 이벤트 처리 방식으로 구현

### 주요 기능

* 영상 파일 불러오기
* 영상 변환
* 영상을 Image 형태로 출력
* Histogram 출력
* LUT 적용한 영상 및 Histogram 출력

<br/>

### 동작 흐름

1. OpenFileDialog 를 이용한 파일 불러오기
2. 확장자 판별 (DICOM 또는 RAW)
3. 선택한 파일의 메타데이터 읽고 16bits 변환 후 8bits 변환
4. ImageBox 컨트롤에 영상 출력
5. Contrast 조절 (1.0 ~ 3.0)
    * 수식: 픽셀 값 * alpha
    * Up일 경우, alpha += 0.1
    * Down일 경우, alpha -= 0.1
    * 초기화
6. Brightness 조절 (0 ~ 100)
    * 수식: 픽셀 값 * alpha + beta
    * Up일 경우, beta += 10
    * Down일 경우, beta -= 10
    * 초기화
7. LUT 적용 후의 영상 및 Histogram 출력
    * Window Width는 13926
    * Window Center는 6225