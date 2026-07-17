# 석탄 미분기 3D 시뮬레이터 — Ver 01

## 프로젝트 개요
- Unity 2022+ 기반 석탄 미분기(Coal Pulverizer) 3D 인터랙티브 시뮬레이터
- 절차적 메시 생성 + CAD 메시 혼합 방식
- 파트 클릭 → 우측 슬라이드 패널에 도면 + 마모도 표시

---

## 롤타이어 형상

### CAD 메시 (우선)
- 소스: `Assets/Coal_Pulverizer.obj` (STP 원본을 OBJ 변환)
- `CoalPulverizerSceneCreator.cs`에서 임포트 후 MeshFilter로 추출해 `builder.rollTireCADMesh`에 할당

### 절차적 폴백 프로파일 (CAD 없을 때)
`CoalPulverizerBuilder.cs` — `RollTireProfile()` 4점 frustum:

| 포인트 | z (축방향) | r (반지름) | 설명 |
|--------|-----------|-----------|------|
| 0 | -0.42 | 0.12 | 인보드 보어 |
| 1 | -0.42 | 0.80 | 아웃보드 외경 (넓은끝) |
| 2 | +0.42 | 0.34 | 인보드 외경 (좁은끝) |
| 3 | +0.42 | 0.12 | 인보드 보어 |

---

## 롤타이어 설치 기하학

**파일**: `Assets/Scripts/CoalPulverizerBuilder.cs` → `AddRollWheelAssembly()`

```csharp
// 수직분=0.25(수평에 가까움) → 인보드(중심부)가 올라가고 아웃보드(외측)가 내려가 원판에 닿음
Vector3 rollAxis = (radial + Vector3.up * 0.25f).normalized;
Vector3 wheelCenter = radial * 1.18f + Vector3.up * 4.150f;
```

### 핵심 수치
- `rollAxis` 수직분: **0.25** (더 작으면 아웃보드가 더 내려감, 더 크면 수직에 가까워짐)
- `wheelCenter.y`: **4.150** (원판 상면 Y=3.38 기준으로 경험적으로 결정)
- radial 거리: `1.18f` (회전축으로부터 타이어 중심까지)
- 롤타이어 3개, 120° 간격 배치

### 조정 이력 (경험적 캘리브레이션)
| wheelCenter.y | 상태 |
|--------------|------|
| 4.000 | 원판에 파묻힘 |
| 4.200 | 인보드(zones 1-2)만 닿고 아웃보드 뜸 |
| 4.600 | 완전히 공중에 뜸 |
| **4.150** | 최종 확정 (rollAxis=0.25 기준) |

---

## 그라인딩 플레이트 (원판)

```csharp
AddRing(center=(0, 3.25, 0), outerR=2.20, innerR=0.40, height=0.26)
```

- 상면 Y = 3.25 + 0.13 = **3.38**
- 롤타이어 아웃보드 외경이 이 Y=3.38 면에 접촉하도록 설계

---

## 인터랙션 시스템

### 파트 클릭 흐름
```
파트 클릭 → PartSelector → PartHighlighter (선택 파트 하이라이트)
                         → PartInfoPanel (우측 슬라이드 패널 열기)
```

### PartInfoPanel (우측 슬라이드 패널)
- **파일**: `Assets/Scripts/UI/PartInfoPanel.cs`
- Canvas: ScreenSpaceOverlay, sortingOrder=10
- CanvasScaler: ScaleWithScreenSize, referenceResolution=1920×1080
- 슬라이드인 애니메이션으로 열림/닫힘
- 내용: 파트명 + 도면 이미지 + WearPanel(마모도)

### WearPanel / WearData
- **파일**: `Assets/Scripts/UI/WearPanel.cs`, `Assets/Scripts/WearData.cs`
- Zones 1-10 구역별 마모도 바 그래프 시각화

### RollTireZone
- **파일**: `Assets/Scripts/RollTireZone.cs`
- 롤타이어를 zone 1~10으로 구역 분리
- 각 구역의 마모 데이터를 WearData와 연동

---

## 씬 구성 (CoalPulverizerSceneCreator.cs)

| 오브젝트 | 역할 |
|----------|------|
| Coal Pulverizer | 루트. CoalPulverizerBuilder, PartHighlighter, PartSelector |
| UI Canvas | PartInfoPanel |
| Orbit Camera | OrbitCamera 컴포넌트, target=root, orbit(12f, -35f, 38f) |
| Key Light | Directional, intensity=1.55 |
| Fill Light | Point, intensity=1.2, range=10 |
| Rim Light | Directional, intensity=0.65 |
| Grinding Zone Light | Point, intensity=1.1, range=5, pos=(0,3.8,0) |

---

## 주요 파일 목록

```
Assets/
├── Coal_Pulverizer.obj          # CAD 롤타이어 메시 (STP→OBJ)
├── Scripts/
│   ├── CoalPulverizerBuilder.cs # 메인 빌더 (기하학 전체)
│   ├── MeshFactory.cs           # Lathe/절차적 메시 생성
│   ├── OrbitCamera.cs           # 카메라 오빗 컨트롤
│   ├── PartDatabase.cs          # 파트 메타데이터
│   ├── PartHighlighter.cs       # 호버/선택 하이라이트
│   ├── PartSelector.cs          # 클릭 선택 처리
│   ├── RollTireZone.cs          # 마모 구역 1-10
│   ├── WearData.cs              # 마모도 데이터 모델
│   └── UI/
│       ├── PartInfoPanel.cs     # 우측 슬라이드 패널
│       └── WearPanel.cs         # 마모도 바 그래프
│   └── Editor/
│       └── CoalPulverizerSceneCreator.cs  # 씬 자동생성 에디터 툴
Packages/
└── manifest.json                # com.unity.ugui + physics
```

---

## 다음 세션을 위한 체크리스트

새 Claude 세션에서 작업 시작 전 확인:
1. `CoalPulverizerBuilder.cs` → `AddRollWheelAssembly()` 현재 rollAxis/wheelCenter 값 확인
2. Unity에서 Play → 롤타이어가 원판 상면(Y=3.38)에 아웃보드 측이 닿는지 육안 확인
3. 파트 클릭 시 우측 패널 슬라이드인 동작 확인
