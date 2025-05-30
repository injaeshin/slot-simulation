# Slot Simulation - 확률 시뮬레이션 속도 검증 도구

**Slot Simulation**은 슬롯 게임에서 사용되는 확률 로직에 대한  
이론 수치와 실제 시뮬레이션 결과를 비교/검증하기 위한 콘솔 기반 테스트 도구입니다.

단순 슬롯 구조를 기반으로, 대규모 반복 시뮬레이션을 빠르게 처리하기 위해  
멀티스레딩 및 Thread Local Storage(TLS)를 활용하여 병렬 처리 후 결과를 통합하는 구조로 설계되었습니다.

---

## 🔧 주요 기술 구성

- C# (.NET 8)
- Console Application
- `System.Threading` 기반 병렬 처리
- Thread Local Storage (TLS) 활용
- 고속 통계 시뮬레이션

---

## 주요 기능

### 슬롯 시뮬레이션
- 미리 정의된 확률 테이블 기반으로 슬롯 회전
- 고정된 확률 모델을 바탕으로 실제 통계 수치 산출

### 멀티스레드 기반 고속 시뮬레이션
- 시뮬레이션을 다수의 Thread로 분할 처리
- 각 Thread는 TLS에 자체 결과를 저장하고, 완료 후 통합

### 통계 출력
- 각 보상의 출현 횟수, 출현 비율 출력
- 시뮬레이션 총 회차 대비 실측 확률 출력
- 이론값과의 차이 비교 가능
