# 유튜버 키우기 (Idle YouTuber Tycoon) - Unity Client
## 작업 규칙
1. 코드 작업 완료 후 CLAUDE.md "현재 상태" 섹션 업데이트
2. 새 파일 생성 시 해당 파일의 역할을 간단히 기록
3. 구조 변경 시 "변경 이력"에 날짜 + 이유 기록
4. 커밋 메시지는 한국어로, 무엇을 왜 했는지 명확하게
5. TODO 항목 완료 시 상태를 "완료"로 변경
6. **UI는 반드시 UI_Base/UI_Popup 패턴으로 작성** (SerializeField 금지, Enum 바인딩 사용)

## 변경 이력
- 2026-02-10: 프로젝트 초기 구조 생성 (Sonnet)
- 2026-02-11: CLAUDE.md 추가 (Opus #1)
- 2026-02-11: Network 모듈 추가 - 서버 통신 기반 구현 (Opus #1)
- 2026-02-11: UI 스크립트 + AppEntry 추가 (Opus #1)
- 2026-02-11: APIClient + LoginManager 통합 구현 (Opus #2)
- 2026-02-11: Unity 클라이언트 LoginScreen 구현 (Opus #3)
- 2026-02-11: CharacterListUI 구현 - 캐릭터 목록, 필터링, 레벨업/돌파 UI (Sonnet 4.5)
- 2026-02-12: 코드 정리 - 서버 레포 클라 코드를 클라 레포로 통합, 중복 제거 (Opus #1)
- 2026-02-12: UI 아키텍처 리팩토링 - Enum 바인딩 + 팝업 스택 시스템으로 전환 (Opus #1)

---
## 프로젝트 개요
- **장르**: 방치형 경영 시뮬레이션 (싱글 중심)
- **플랫폼**: Android 우선 (iOS는 반응 보고)
- **엔진**: Unity 6 (2D)
- **서버 레포**: https://github.com/smsm8087/YouTuberGame-Server
- **개발자**: 1인 부업 (주말 3~4시간), Claude가 코드 70~80% 담당

## 핵심 루프
팀원 수집(가챠) → 팀 편성 → 콘텐츠 제작(방치) → 수익 획득 → 장비/스튜디오 업그레이드 → 더 좋은 콘텐츠

## 게임 시스템
- **4대 스탯**: 촬영력, 편집력, 기획력, 디자인력 → 합산 = 채널 파워
- **팀원**: 12~15명, 등급 C(60%)/B(25%)/A(12%)/S(3%), 시너지 시스템
- **장르**: 브이로그(시작) → 게임(1K) → 먹방(5K) → 교육(20K) → 쇼츠(50K) → 다큐(200K)
- **장비**: 카메라/PC/마이크/조명, 각 최대 Lv.10
- **마일스톤**: 100 → 1K → 10K → 100K → 500K → 1M → 10M(엔딩)
- **경쟁**: 주간 랭킹만 (가벼운 보상), PvP 없음

## 재화
- **골드** (광고 수익): 장비, 육성
- **보석**: 가챠, 즉시 완성
- **가챠 티켓**: 팀원 뽑기
- **경험치 칩**: 팀원 레벨업

## 수익 모델
- 광고 보상 (수익 2배, 무료 뽑기, 즉시 완성)
- 광고 제거권 $4.99 (1회)
- 월간 구독 $2.99/월
- 스타터 패키지 $4.99 (1회)
- 보석 상점 $0.99~$9.99

## 기술 결정
- Addressables 안 씀 (50~100MB, 빌드 포함)
- Resources 폴더 + 직접 참조
- 로컬라이징: 한국어만 (반응 좋으면 영어 추가)
- API URL 하드코딩 금지 (서버 이사 대비, ScriptableObject 또는 Config로 관리)
- 데이터 핫 업데이트: 앱 시작 시 서버 버전 체크 → 변경 시 JSON 다운로드
- 커뮤니티: 인앱 채팅 없음, Discord 서버로 대체

## 개발 Phase
1. **Phase 1 - MVP**: 스튜디오 뷰, 콘텐츠 제작, 4스탯, 구독자 시뮬, 기본 팀원 3~4명
2. **Phase 2 - 수집&육성**: 가챠, 레벨업/돌파, 장비, 장르 확장
3. **Phase 3 - 경쟁&수익화**: 랭킹, 광고 보상, 인앱 결제, 마일스톤

## UI 아키텍처
DinoMutation 프로젝트의 검증된 패턴을 적용:

### 기반 클래스
- `UI/Base/UI_Base.cs`: Enum 기반 컴포넌트 바인딩 (Bind<T>, GetButton, GetText 등)
- `UI/Base/UI_Popup.cs`: DOTween 팝업 애니메이션, 터치가드, UIManager 스택 연동
- `UI/Base/UI_Page.cs`: 탭 콘텐츠 기반 클래스

### 핵심 시스템
- `Core/UIManager.cs`: 싱글톤 팝업 스택 관리, Resources/UI/ 로딩, 정렬 순서
- `Utils/GameObserver.cs`: 정적 이벤트 시스템 (On/Off/Emit), 데이터→UI 자동 갱신
- `Utils/Util.cs`: FindChild, GetOrAddComponent, FormatNumber 헬퍼

### UI 작성 패턴
```csharp
public class ExamplePopup : UI_Popup
{
    enum Buttons { CloseBtn, ActionBtn }
    enum Texts { TitleText }

    protected override void FirstSetting()
    {
        base.FirstSetting();
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        GetButton(Buttons.CloseBtn).AddButtonEvent(() => ClosePop());
    }

    public override void Init()
    {
        base.Init();
        OpenPop();
    }
}
// 호출: UIManager.Instance.ShowPopup<ExamplePopup>();
```

### API 호출 패턴
APIClient는 코루틴 기반 (IEnumerator + Action<bool, string> 콜백):
```csharp
StartCoroutine(APIClient.Instance.GetPlayerData((ok, res) =>
{
    if (!ok) return;
    var data = JsonUtility.FromJson<PlayerResponse>(res);
    // UI 갱신
}));
```

## 현재 상태 (2026-02-12)
### Network
- `Network/APIClient.cs`: 올인원 REST API 클라이언트 (코루틴 기반)
  - 인증(Login/Register), 플레이어, 가챠, 캐릭터, 콘텐츠, 장비, 랭킹
  - 응답 DTO: AuthResponse, PlayerResponse, CharacterResponse, GachaResponse 등

### UI (Enum 바인딩 아키텍처)
- `UI/Popup/LoginPopup.cs`: 로그인/회원가입, 자동 로그인, 씬 전환
- `UI/MainHUD.cs`: 메인 씬 상시 UI (재화 바, 메뉴 버튼, 콘텐츠 제작 패널)
- `UI/Popup/GachaPopup.cs`: 가챠 1회/10회
- `UI/Popup/CharacterPopup.cs`: 캐릭터 목록, 등급 필터
- `UI/Popup/EquipmentPopup.cs`: 장비 업그레이드 (4종)
- `UI/Popup/RankingPopup.cs`: 주간/채널파워 랭킹 탭
- `UI/Popup/ContentHistoryPopup.cs`: 콘텐츠 히스토리, 통계

### 기타
- `Core/AppEntry.cs`: 앱 진입점
- 씬, 프리팹은 Unity 에디터에서 작업 필요

## 다음 작업 (TODO)
- Unity 에디터: Login 씬, Main 씬 생성
- Unity 에디터: UI 프리팹 생성 및 스크립트 연결 (각 Popup의 Enum 이름과 프리팹 오브젝트 이름 일치시키기)
- Unity 에디터: Resources/UI/ 폴더에 팝업 프리팹 배치
- 서버 DB 구축 (Oracle Cloud + MySQL) → 집에서 작업
- 리스트 아이템 프리팹: CharacterCard, RankingEntryItem, ContentHistoryCard 등

## 코드 컨벤션
- C# 네이밍: PascalCase (public), _camelCase (private field)
- UI: UI_Base 상속, Enum 바인딩 필수 (SerializeField 사용 금지)
- 팝업: UI_Popup 상속, UIManager.Instance.ShowPopup<T>()로 호출
- 데이터 갱신: GameObserver.Emit() → 구독 중인 UI 자동 갱신
- API 호출: StartCoroutine + 콜백 패턴

## 참고 문서
- API 명세: docs/API.md
- DB 스키마: docs/DATABASE.md
