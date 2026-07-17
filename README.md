# NEXTRO — 석탄 미분기 3D 디지털 트윈 대시보드

실시간 마모 모니터링 + 잔여수명(RUL) 예측을 위한 인터랙티브 WebGL 대시보드.

**라이브 데모**: https://ovzexx.github.io/CoalPulverizer3D/

---

## 전체 구조

```
┌─────────────────────────────────────────────────────┐
│              브라우저 (docs/index.html)              │
│                                                     │
│  ┌──────────────────┐    ┌───────────────────────┐  │
│  │  Unity WebGL 뷰어 │    │   우측 정보 패널       │  │
│  │  (3D 인터랙션)    │◄──►│   - 경고/교체 예측     │  │
│  │                  │    │   - 마모 히트맵        │  │
│  │  SendMessage()   │    │   - 실시간 그래프      │  │
│  └──────────────────┘    │   - 운전 변수         │  │
│                          └───────────────────────┘  │
│  ┌──────────────────────────────────────────────┐   │
│  │  하단 패널 (엔지니어링 도면 / 그래프)           │   │
│  └──────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

---

## 파일 구조

```
CoalPulverizer3D/
├── docs/                          ← GitHub Pages 서빙 폴더
│   ├── index.html                 ← 대시보드 메인 (HTML/CSS/JS 단일 파일)
│   ├── Build/                     ← Unity WebGL 빌드 결과물
│   │   ├── unity_webgl.data
│   │   ├── unity_webgl.framework.js
│   │   ├── unity_webgl.loader.js
│   │   └── unity_webgl.wasm
│   └── TemplateData/              ← Unity 로더 UI 에셋
│
├── Assets/
│   ├── Coal_Pulverizer.obj        ← CAD 원본 (STP→OBJ 변환)
│   ├── Scenes/
│   │   └── CoalPulverizerDemo.unity
│   ├── Scripts/
│   │   ├── CoalPulverizerBuilder.cs  ← 3D 기하학 생성 (메인 빌더)
│   │   ├── WearController.cs         ← JS→Unity 마모 데이터 수신
│   │   ├── RollTireZone.cs           ← 롤타이어 Zone 오버레이 (색상)
│   │   ├── OrbitCamera.cs            ← 마우스 오빗 카메라
│   │   ├── PartSelector.cs           ← 파트 클릭 선택
│   │   ├── PartHighlighter.cs        ← 호버/선택 하이라이트
│   │   ├── JSBridge.cs               ← Unity→JS 이벤트 발신
│   │   ├── WearData.cs               ← 마모 데이터 모델
│   │   └── Editor/
│   │       ├── CoalPulverizerSceneCreator.cs  ← 씬 자동 생성 툴
│   │       └── WebGLBuild.cs                  ← 빌드 자동화
│   ├── Shaders/
│   │   └── ZoneOverlay.shader     ← 마모 구역 투명 오버레이 셰이더
│   └── Plugins/WebGL/
│       └── JSBridge.jslib         ← Unity↔JS 브릿지 플러그인
│
├── Packages/
│   └── manifest.json
├── ProjectSettings/
├── README.md                      ← 이 파일
└── TECHNICAL_NOTES.md             ← 롤타이어 기하학 캘리브레이션 기록
```

---

## JS ↔ Unity 브릿지 API

### JS → Unity (마모 데이터 전송)

```javascript
// 롤러별 Zone 1~10 마모율 (0.0~1.0) CSV로 전송
unityInstance.SendMessage('Coal Pulverizer', 'UpdateR1Wear', "0.12,0.25,0.45,0.60,0.71,0.55,0.38,0.22,0.18,0.10");
unityInstance.SendMessage('Coal Pulverizer', 'UpdateR2Wear', "...");
unityInstance.SendMessage('Coal Pulverizer', 'UpdateR3Wear', "...");
```

- CSV 순서: Zone 인덱스 0~9 (0 = 와이드 끝/Zone 10, 9 = 내로우 끝/Zone 1)
- 값 범위: `0.0` (정상) ~ `1.0` (완전 마모)

### Unity → JS (파트 선택 이벤트)

```javascript
// index.html 에서 수신
function onUnityPartSelected(partName) {
  // partName: "Replaceable Asymmetric Roll Tire 1" 등
}
```

---

## Unity WebGL 빌드 방법

> **주의**: Unity 빌드는 팀 내 Unity 설치 환경에서만 가능합니다.  
> HTML 대시보드만 수정할 경우 Unity 없이 `docs/index.html` 편집만으로 됩니다.

1. Unity Hub에서 프로젝트 추가: 이 폴더 경로
2. Unity 버전: `6000.5.2f1` (Unity 6)
3. 씬 열기: `Assets/Scenes/CoalPulverizerDemo.unity`
4. 메뉴 → `Tools > Coal Pulverizer > Build WebGL to docs/`
5. 빌드 결과물이 `docs/Build/`에 자동 출력됨
6. `docs/index.html` + `docs/Build/` 를 함께 GitHub에 푸시

---

## HTML 대시보드만 수정하는 경우

Unity 없이도 대시보드 UI 수정 가능:

```bash
# 로컬 서버 실행
cd docs
python3 -m http.server 8877
# http://localhost:8877 에서 확인
```

수정 대상: `docs/index.html` (CSS/JS/HTML 모두 이 파일 안에 있음)

---

## 주요 기술 노트

- **셰이더**: `Assets/Shaders/ZoneOverlay.shader` — 커스텀 Unlit 투명 셰이더 사용.  
  Unity WebGL 빌드 시 표준 셰이더 변형(variants)이 strip되므로 반드시 커스텀 셰이더 필요.
- **런타임 Rebuild 방지**: `CoalPulverizerBuilder.OnEnable()`에서 `Application.isPlaying`이면 즉시 return.  
  WebGL 초기화 중 무거운 GO 생성 → 메인 스레드 블로킹 방지.
- **캐시 버스팅**: Unity 파일 URL에 `?v=N` 쿼리 파라미터로 CDN 캐시 우회.
- 롤타이어 기하학 수치 상세: `TECHNICAL_NOTES.md` 참고
