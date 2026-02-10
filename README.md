# 유튜버 키우기 (Idle YouTuber Tycoon)

## 프로젝트 개요
1인 개발 모바일 방치형 경영 시뮬레이션 게임

### 게임 컨셉
팀원을 모아 콘텐츠를 제작하고, 채널을 성장시키는 방치형 경영 게임

## 기술 스택
- **엔진**: Unity 6000.0.50f1 (2D)
- **언어**: C#
- **캐릭터**: ComfyUI (AI 생성)
- **애니메이션**: Spine 2D
- **서버**: MySQL + REST API
- **버전 관리**: Git + GitHub

## 프로젝트 구조
```
Assets/
├── Scripts/           # C# 스크립트
│   ├── Core/         # 코어 시스템
│   ├── Managers/     # 게임 매니저
│   ├── Data/         # 데이터 모델
│   ├── UI/           # UI 관련
│   ├── Studio/       # 스튜디오 시스템
│   ├── Character/    # 캐릭터/팀원 시스템
│   ├── Content/      # 콘텐츠 제작 시스템
│   ├── Equipment/    # 장비 시스템
│   ├── Network/      # 서버 통신
│   └── Utils/        # 유틸리티
├── Scenes/           # Unity 씬
├── Prefabs/          # 프리팹
├── Resources/        # 리소스
│   ├── Characters/   # 캐릭터 이미지
│   ├── UI/          # UI 스프라이트
│   └── Audio/       # 사운드
├── Spine/            # Spine 애니메이션
├── Data/             # ScriptableObject 데이터
└── Editor/           # 에디터 스크립트
```

## 개발 단계
### Phase 1 - MVP (핵심 루프)
- [ ] 스튜디오 뷰 + 팀원 배치 UI
- [ ] 콘텐츠 제작 (방치) 시스템
- [ ] 4스탯 + 품질 판정
- [ ] 구독자/수익 시뮬레이션
- [ ] 기본 팀원 3~4명

### Phase 2 - 수집 & 육성
- [ ] 가챠 시스템
- [ ] 팀원 레벨업/돌파
- [ ] 장비 업그레이드
- [ ] 콘텐츠 장르 확장

### Phase 3 - 경쟁 & 수익화
- [ ] 비동기 PvP (랭킹, 콘텐츠 대결)
- [ ] 광고 보상 시스템
- [ ] 인앱 결제
- [ ] 마일스톤 시스템

## 시작하기
1. Unity Hub에서 프로젝트 열기
2. Unity 6000.0.50f1 버전 사용
3. `Assets/Scenes/MainScene.unity` 실행

## 문서
- [게임 기획서](C:/Users/apdlv/Documents/카카오톡 받은 파일/유튜버키우기.pdf)
- [API 문서](./docs/API.md) (예정)
- [데이터베이스 스키마](./docs/DATABASE.md) (예정)
