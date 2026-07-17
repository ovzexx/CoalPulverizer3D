using System.Collections.Generic;
using UnityEngine;

namespace CoalPulverizer
{
    public static class PartDatabase
    {
        private static Dictionary<string, PartData> _byId;
        private static List<(string prefix, string id)> _prefixMap;

        static PartDatabase()
        {
            _byId = new Dictionary<string, PartData>
            {
                ["roll_assembly"] = new PartData
                {
                    Id = "roll_assembly",
                    KoreanName = "롤 타이어 어셈블리",
                    EnglishName = "Roll Wheel Assembly",
                    Description = "3개의 롤 타이어가 불 링 위를 눌러 석탄을 분쇄합니다. 각 롤은 유압 스프링으로 가압되며, 저널 샤프트와 하우징이 하중을 지지합니다.",
                    Specs = new[]
                    {
                        "수량: 3개 (120° 간격)",
                        "재질: 고크롬 내마모강",
                        "마모 한계: 50 mm",
                        "교체 주기: ~5,000 운전시간",
                        "가압 방식: 유압 스프링",
                    },
                    BlueprintZone = 4,
                    HighlightColor = new Color(1f, 0.3f, 0.5f),
                    // AMWI 샘플 센서값 (B호기 2026-07-14 기준)
                    // GP = (P_J 8.3MPa + P_H 6.8MPa) 정규화 → 0.72
                    // CT = 급탄율 55t/h 정규화 → 0.85
                    // BI = Bowl DP 1.24kPa 정규화 → 0.61
                    // RL = (분류기 28rpm + ctrl 0.82) 평균 → 0.78
                    // AMWI = 0.72×0.85×0.61×0.78 ≈ 0.289 (주의 범위)
                    WearData = new WearData
                    {
                        GP = 0.72f,
                        CT = 0.85f,
                        BI = 0.61f,
                        RL = 0.78f,
                        WornMm          = 23.4f,
                        WearLimitMm     = 50.0f,
                        DailyWearRateMm = 0.041f,
                        // [0]=하우징측 분쇄하중 집중 → 마모 심함, [9]=중심측 → 마모 적음
                        ZoneWear = new float[] { 0.84f, 0.78f, 0.71f, 0.65f, 0.57f,
                                                 0.48f, 0.40f, 0.33f, 0.25f, 0.18f },
                    },
                },
                ["bull_ring"] = new PartData
                {
                    Id = "bull_ring",
                    KoreanName = "불 링 (분쇄링)",
                    EnglishName = "Bull Ring / Grinding Ring",
                    Description = "볼 테이블 외곽에 위치하는 환형 트랙으로, 롤 타이어가 이 위를 구르며 석탄을 분쇄합니다. 고크롬 소재로 내마모성이 높습니다.",
                    Specs = new[]
                    {
                        "재질: 고크롬 주철",
                        "경도: HRC 55~62",
                        "회전방식: 볼 테이블과 일체 회전",
                        "교체 시 분해 필요: 상부 하우징 탈거",
                    },
                    BlueprintZone = 3,
                    HighlightColor = new Color(0.2f, 0.2f, 0.2f),
                },
                ["bowl_table"] = new PartData
                {
                    Id = "bowl_table",
                    KoreanName = "볼 테이블 (그라인딩 볼)",
                    EnglishName = "Bowl / Grinding Table",
                    Description = "기어 감속기에 의해 회전하는 주요 분쇄 플레이트입니다. 원료탄이 테이블 위에 낙하하면 원심력으로 외곽으로 이동하며 롤 타이어와 불 링 사이에서 분쇄됩니다.",
                    Specs = new[]
                    {
                        "회전 속도: 약 45 RPM",
                        "재질: 고망간강 + 세라믹 내마모 라이너",
                        "직경: 약 3.2 m (B호기 기준)",
                        "구동: 수직 기어 감속기를 통한 전동기 직결",
                    },
                    BlueprintZone = 2,
                    HighlightColor = new Color(0.8f, 0.1f, 0.1f),
                },
                ["classifier"] = new PartData
                {
                    Id = "classifier",
                    KoreanName = "동적 분급기",
                    EnglishName = "Dynamic Classifier",
                    Description = "상부에 위치하며 분쇄된 석탄 입자를 크기별로 분리합니다. 회전 블레이드가 조대 입자는 다시 분쇄존으로 반송하고, 미세 입자만 출구 파이프로 내보냅니다.",
                    Specs = new[]
                    {
                        "회전 속도: 35 RPM (가변)",
                        "분리 입도: 200메시 (75 μm) 기준",
                        "블레이드 수: 28개",
                        "구동 모터: 4개 독립 구동",
                        "재질: 내마모 세라믹 라이너",
                    },
                    BlueprintZone = 9,
                    HighlightColor = new Color(0.1f, 0.7f, 0.3f),
                },
                ["housing"] = new PartData
                {
                    Id = "housing",
                    KoreanName = "압력 하우징",
                    EnglishName = "Pressure Housing Shell",
                    Description = "1차 공기와 미분탄 흐름을 밀폐하는 원통형 압력 용기입니다. 절개형 구조로 내부 점검이 가능하며 상하 플랜지 볼트로 결합됩니다.",
                    Specs = new[]
                    {
                        "설계 압력: 약 +3 kPa (내압)",
                        "재질: 탄소강 (두께 약 20 mm)",
                        "플랜지 볼트: 상·하 각 28개",
                        "점검구: 측면 접근 도어 1개",
                        "절개 각도: 320° 가시범위",
                    },
                    BlueprintZone = 7,
                    HighlightColor = new Color(0.7f, 0.75f, 0.78f),
                },
                ["primary_air"] = new PartData
                {
                    Id = "primary_air",
                    KoreanName = "1차 공기 공급 시스템",
                    EnglishName = "Primary Air System",
                    Description = "열풍을 미분기 하부 스로트 링을 통해 공급하여 분쇄된 석탄을 공기 부양시켜 분급기 방향으로 운반합니다.",
                    Specs = new[]
                    {
                        "공기 온도: 250~320 °C (열풍)",
                        "유입 방향: 하우징 측면 덕트",
                        "노즐 베인: 24개 (회전 부여)",
                        "목적: 석탄 부양 + 건조",
                    },
                    BlueprintZone = 6,
                    HighlightColor = new Color(0.1f, 0.4f, 0.9f),
                },
                ["coal_feed"] = new PartData
                {
                    Id = "coal_feed",
                    KoreanName = "원료탄 투입관",
                    EnglishName = "Raw Coal Feed Pipe",
                    Description = "급탄기(피더)로부터 원료 석탄을 분급기 중심부로 직접 투입하는 파이프입니다. 하부 피드 스커트를 통해 볼 테이블 중앙에 낙하합니다.",
                    Specs = new[]
                    {
                        "관경: 약 360 mm",
                        "재질: 내마모 탄소강",
                        "급탄률: 10~60 t/h (부하에 따라 가변)",
                        "경로: 상부 → 분급기 중심 → 볼 테이블",
                    },
                    BlueprintZone = 11,
                    HighlightColor = new Color(0.5f, 0.3f, 0.1f),
                },
                ["coal_outlet"] = new PartData
                {
                    Id = "coal_outlet",
                    KoreanName = "미분탄 배출관",
                    EnglishName = "Pulverized Coal Outlet Pipes",
                    Description = "분급기를 통과한 미세 미분탄을 보일러 버너로 공급하는 4개의 출구 파이프입니다.",
                    Specs = new[]
                    {
                        "수량: 4개 (90° 간격)",
                        "유속: 약 20~25 m/s",
                        "입도: 200메시 통과 70% 이상",
                        "연결 방향: 보일러 1차 공기 버너",
                    },
                    BlueprintZone = 12,
                    HighlightColor = new Color(0.8f, 0.5f, 0.2f),
                },
                ["drive"] = new PartData
                {
                    Id = "drive",
                    KoreanName = "구동계 (기어 감속기)",
                    EnglishName = "Drive System / Gear Reducer",
                    Description = "전동기의 고속 회전을 감속하여 볼 테이블을 저속 고토크로 구동합니다. 유성 기어 방식으로 수직 주축과 직결됩니다.",
                    Specs = new[]
                    {
                        "전동기 출력: 약 1,000 kW",
                        "감속비: 약 1:20",
                        "출력 속도: 45 RPM",
                        "윤활: 강제 윤활유 순환 시스템",
                        "방식: 유성 기어 감속기",
                    },
                    BlueprintZone = 1,
                    HighlightColor = new Color(0.2f, 0.4f, 0.9f),
                },
                ["reject_system"] = new PartData
                {
                    Id = "reject_system",
                    KoreanName = "파이라이트 배출 시스템",
                    EnglishName = "Pyrites Reject System",
                    Description = "석탄 내 황철석(파이라이트), 암석 등 분쇄되지 않는 이물질을 스크레이퍼로 걷어내 배출 슈트를 통해 외부로 제거합니다.",
                    Specs = new[]
                    {
                        "스크레이퍼: 볼 테이블과 일체 회전",
                        "배출구: 하우징 측면 슈트",
                        "처리 대상: 황철석, 암석, 금속 이물질",
                        "배출 호퍼: 별도 수거 박스",
                    },
                    BlueprintZone = 14,
                    HighlightColor = new Color(0.2f, 0.2f, 0.25f),
                },
                ["spring_loading"] = new PartData
                {
                    Id = "spring_loading",
                    KoreanName = "유압 가압 시스템",
                    EnglishName = "Hydraulic Loading System",
                    Description = "롤 타이어를 불 링에 일정한 압력으로 눌러주는 외부 유압 실린더 및 스프링 시스템입니다. 저널 압력 센서(MILL_JOURNAL_PR)가 이 압력을 계측합니다.",
                    Specs = new[]
                    {
                        "가압 방식: 유압 + 코일 스프링 조합",
                        "설계 압력: 약 8~12 MPa",
                        "모니터링: MILL_JOURNAL_PR 센서",
                        "풀 로드: 외부 인장봉 3개",
                        "실린더 위치: 하우징 외부 3개소",
                    },
                    BlueprintZone = 13,
                    HighlightColor = new Color(0.1f, 0.7f, 0.2f),
                },
                ["feed_skirt"] = new PartData
                {
                    Id = "feed_skirt",
                    KoreanName = "피드 스커트",
                    EnglishName = "Feed Skirt",
                    Description = "원료탄 투입관 하부에 위치하며 낙하하는 석탄을 볼 테이블 중심으로 안내하는 원뿔형 가이드입니다.",
                    Specs = new[]
                    {
                        "형상: 상광하협 원뿔형",
                        "재질: 내마모강",
                        "위치: 볼 테이블 상부 중심",
                    },
                    BlueprintZone = 8,
                    HighlightColor = new Color(0.95f, 0.72f, 0.1f),
                },
            };

            // GameObject 이름 → PartId 매핑 (접두사 기준)
            _prefixMap = new List<(string, string)>
            {
                ("Roll Wheel Assembly",              "roll_assembly"),
                ("Replaceable Asymmetric Roll Tire", "roll_assembly"),
                ("Roll Wheel Web",                   "roll_assembly"),
                ("Roll Wheel Inboard",               "roll_assembly"),
                ("Roll Wheel Outboard",              "roll_assembly"),
                ("Bolted Journal Hub",               "roll_assembly"),
                ("Journal Shaft",                    "roll_assembly"),
                ("Rectangular Journal Housing",      "roll_assembly"),
                ("Forked Roll Loading Yoke",         "roll_assembly"),
                ("Upper Journal Pin",                "roll_assembly"),
                ("Roll Tire Outer Circumference",    "roll_assembly"),
                ("Sloped High Chrome Bull Ring",     "bull_ring"),
                ("Shallow Conical Bowl Table",       "bowl_table"),
                ("Bowl Rotating Assembly",           "bowl_table"),
                ("Rotating Coal Bed",                "bowl_table"),
                ("Grinding Segment Liner",           "bull_ring"),
                ("Rotating Classifier Cage",         "classifier"),
                ("Dynamic Classifier Blade Ring",    "classifier"),
                ("Classifier Inlet Blade",           "classifier"),
                ("Tall Ceramic Lined Inner Classifier Cone", "classifier"),
                ("Raised Ceramic Lined",             "classifier"),
                ("Classifier Fines Discharge Ring",  "classifier"),
                ("Classifier Drive Turret",          "classifier"),
                ("Classifier Motor",                 "classifier"),
                ("Classifier Cage",                  "classifier"),
                ("Extra Tall Thick Cutaway Pressure Housing Shell", "housing"),
                ("Lower Open Housing Flange Ring",   "housing"),
                ("Upper Open Housing Flange Ring",   "housing"),
                ("Bolted Top Cover Plate",           "housing"),
                ("Grinding Zone Access Door",        "housing"),
                ("Primary Air Inlet Duct",           "primary_air"),
                ("Rotating Throat And Nozzle Ring",  "primary_air"),
                ("Primary Air Nozzle Vane",          "primary_air"),
                ("Sloped Primary Air Nozzle Vanes",  "primary_air"),
                ("Seal Air Header",                  "primary_air"),
                ("Central Raw Coal Feed Pipe",       "coal_feed"),
                ("Raw Coal Feed Chute",              "coal_feed"),
                ("Upper Feed Skirt",                 "feed_skirt"),
                ("Pulverized Coal Outlet Pipe",      "coal_outlet"),
                ("Outlet Diffuser Box",              "coal_outlet"),
                ("Planetary Gear Reducer",           "drive"),
                ("Vertical Main Shaft",              "drive"),
                ("Horizontal Motor Shaft",           "drive"),
                ("External Drive Motor",             "drive"),
                ("Pyrites Reject Box",               "reject_system"),
                ("Reject Discharge Chute",           "reject_system"),
                ("Pyrite Scraper Blade",             "reject_system"),
                ("External Pull Rod",                "spring_loading"),
                ("Hydraulic Cylinder Foot",          "spring_loading"),
                ("Hydraulic Loading Rod",            "spring_loading"),
                ("External Spring Canister",         "spring_loading"),
                ("Exposed Coil Spring",              "spring_loading"),
            };
        }

        public static PartData GetById(string id)
        {
            _byId.TryGetValue(id, out var data);
            return data;
        }

        public static string FindIdByObjectName(string objectName)
        {
            foreach (var (prefix, id) in _prefixMap)
            {
                if (objectName.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                    return id;
            }
            return null;
        }
    }
}
