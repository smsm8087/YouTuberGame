# 유튜버 키우기 (Idle YouTuber Tycoon) - Unity Client
## 작업 규칙
1. 코드 작업 완료 후 CLAUDE.md "현재 상태" 섹션 업데이트
2. 새 파일 생성 시 해당 파일의 역할을 간단히 기록
3. 구조 변경 시 "변경 이력"에 날짜 + 이유 기록
4. 커밋 메시지는 한국어로, 무엇을 왜 했는지 명확하게
5. TODO 항목 완료 시 상태를 "완료"로 변경

## 변경 이력
- 2026-02-10: 프로젝트 초기 구조 생성 (Sonnet)
- 2026-02-11: CLAUDE.md 추가 (Opus #1)
- 2026-02-11: Network 모듈 추가 - 서버 통신 기반 구현 (Opus #1)
- 2026-02-11: UI 스크립트 + AppEntry 추가 (Opus #1)
- 2026-02-11: APIClient + LoginManager 통합 구현 (Opus #2)
- 2026-02-11: Unity 클라이언트 LoginScreen 구현 (Opus #3)
- 2026-02-11: CharacterListUI 구현 - 캐릭터 목록, 필터링, 레벨업/돌파 UI (Sonnet 4.5)
- 2026-02-12: 코드 정리 - 서버 레포 클라 코드를 클라 레포로 통합, 중복 제거 (Opus #1)

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

## 개발 Phase
1. **Phase 1 - MVP**: 스튜디오 뷰, 콘텐츠 제작, 4스탯, 구독자 시뮬, 기본 팀원 3~4명
2. **Phase 2 - 수집&육성**: 가챠, 레벨업/돌파, 장비, 장르 확장
3. **Phase 3 - 경쟁&수익화**: 랭킹, 광고 보상, 인앱 결제, 마일스톤

## 현재 상태 (2026-02-12)
- 데이터 모델 스캐폴딩 완료 (PlayerData, CharacterData, ContentData, EquipmentData)
- 매니저 로직 스캐폴딩 완료 (DataManager, CharacterManager, ContentManager, EquipmentManager, StudioManager)
- GameManager 싱글톤 구조 생성
- **Network 모듈 구현 완료**:
  - `Network/APIClient.cs`: 올인원 REST API 클라이언트 (인증, 플레이어, 가챠, 캐릭터, 콘텐츠, 장비, 랭킹)
- **UI 스크립트 전체 구현**:
  - `UI/LoginScreen.cs`: 로그인/회원가입, 자동 로그인
  - `UI/MainScreen.cs`: 메인 스튜디오 화면, 콘텐츠 제작/업로드, 타이머
  - `UI/CharacterListScreen.cs`: 캐릭터 목록, 등급 필터, 레벨업/돌파
  - `UI/EquipmentScreen.cs`: 장비 업그레이드 (카메라/PC/마이크/조명)
  - `UI/GachaScreen.cs`: 가챠 1회/10회, 티켓/보석 전환
  - `UI/ContentHistoryScreen.cs`: 콘텐츠 히스토리, 정렬, 통계
  - `UI/RankingScreen.cs`: 주간/채널파워 랭킹
- 씬, 프리팹은 Unity 에디터에서 작업 필요

## 다음 작업 (TODO)
- Unity 에디터: Login 씬, Main 씬 생성
- Unity 에디터: UI 프리팹 생성 및 스크립트 연결
- Unity 에디터: Resources/ApiConfig ScriptableObject 에셋 생성
- 서버 DB 구축 (Oracle Cloud + MySQL) → 집에서 작업

## 코드 컨벤션
- C# 네이밍: PascalCase (public), _camelCase (private field)
- 매니저 패턴: 각 시스템별 Manager 클래스
- 데이터: JSON 기반 (서버 마스터데이터 + 로컬 캐시)

## 참고 문서
- API 명세: docs/API.md
- DB 스키마: docs/DATABASE.md
