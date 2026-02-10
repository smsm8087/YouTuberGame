# YouTuber Game - Server & Admin Tool

## 프로젝트 구조

```
server/
├── YouTuberGame.sln                      # 솔루션 파일
├── src/
│   ├── YouTuberGame.API/                # REST API 서버 (.NET 9.0)
│   │   ├── Controllers/                 # API 컨트롤러
│   │   ├── Services/                    # 비즈니스 로직
│   │   ├── Data/                        # DbContext, Repositories
│   │   └── appsettings.json            # 설정 파일
│   │
│   ├── YouTuberGame.Admin/              # Blazor 어드민 툴 (.NET 9.0)
│   │   ├── Components/                  # Blazor 컴포넌트
│   │   └── Pages/                       # 페이지
│   │
│   └── YouTuberGame.Shared/             # 공유 라이브러리
│       └── Models/                      # 데이터 모델 (Unity와 공유)
│           ├── Character.cs
│           ├── Content.cs
│           ├── Player.cs
│           └── Equipment.cs
```

## 기술 스택

### API 서버
- **ASP.NET Core 9.0** - Web API
- **Entity Framework Core 9.0** - ORM
- **Pomelo.EntityFrameworkCore.MySql** - MySQL 드라이버
- **JWT Bearer** - 인증
- **BCrypt.Net** - 비밀번호 해싱

### 어드민 툴
- **Blazor Server** - C# 기반 웹 UI
- **Bootstrap 5** - UI 프레임워크

### 공유 라이브러리
- **.NET Standard 2.1** - Unity와 호환
- **Data Models** - Unity 프로젝트와 동일한 모델 사용

## 설정

### 1. MySQL 데이터베이스 설정

```sql
CREATE DATABASE youtubergame;
```

### 2. appsettings.json 수정

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=youtubergame;User=root;Password=your_password;"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters-long",
    "Issuer": "YouTuberGameAPI",
    "Audience": "YouTuberGameClient",
    "ExpiryInDays": 7
  }
}
```

### 3. 데이터베이스 마이그레이션

```bash
cd src/YouTuberGame.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 실행

### API 서버 실행
```bash
cd src/YouTuberGame.API
dotnet run
```
API: https://localhost:5001

### 어드민 툴 실행
```bash
cd src/YouTuberGame.Admin
dotnet run
```
Admin: https://localhost:5002

## API 엔드포인트

### 인증
- `POST /api/auth/register` - 회원가입
- `POST /api/auth/login` - 로그인

### 플레이어
- `GET /api/player/me` - 내 정보
- `PUT /api/player/save` - 데이터 저장

### 캐릭터
- `GET /api/characters` - 캐릭터 목록
- `GET /api/player/characters` - 보유 캐릭터
- `POST /api/gacha/draw` - 가챠
- `POST /api/player/characters/{id}/levelup` - 레벨업

### 콘텐츠
- `POST /api/content/start` - 제작 시작
- `GET /api/content/producing` - 제작 중
- `POST /api/content/{id}/upload` - 업로드

### 장비
- `GET /api/player/equipment` - 장비 정보
- `POST /api/player/equipment/{type}/upgrade` - 업그레이드

### 랭킹
- `GET /api/rankings/weekly` - 주간 랭킹
- `GET /api/rankings/channel-power` - 채널 파워 랭킹

## Unity 연동

Unity 프로젝트에서 API 호출:

```csharp
using UnityEngine.Networking;
using YouTuberGame.Shared.Models;

public class APIClient
{
    private string baseUrl = "https://your-server.com/api";
    private string token;

    public async Task<PlayerData> GetPlayerData()
    {
        using var request = UnityWebRequest.Get($"{baseUrl}/player/me");
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            return JsonUtility.FromJson<PlayerData>(request.downloadHandler.text);
        }
        return null;
    }
}
```

## 어드민 툴 기능

- 📊 **대시보드**: 전체 통계, 일일 활성 유저
- 👥 **유저 관리**: 유저 목록, 검색, 상세 정보
- 🎮 **게임 데이터**: 캐릭터, 콘텐츠, 아이템 관리
- 📈 **통계**: 매출, 접속자, 가챠 통계
- 🎁 **이벤트**: 보상 지급, 공지사항

## 다음 단계

1. [x] 프로젝트 생성
2. [x] NuGet 패키지 설치
3. [ ] 공유 데이터 모델 생성
4. [ ] DbContext 및 Entity 설정
5. [ ] API 컨트롤러 구현
6. [ ] JWT 인증 구현
7. [ ] Blazor 어드민 페이지 구현
8. [ ] 배포 설정

## 개발 팁

### Unity와 코드 공유
`YouTuberGame.Shared` 프로젝트의 모델을 Unity에서도 사용할 수 있습니다:

1. Shared 프로젝트를 .NET Standard 2.1로 컴파일
2. DLL을 Unity의 `Assets/Plugins/` 폴더에 복사
3. Unity에서 동일한 모델 사용

### 로컬 개발 환경
- API: `https://localhost:5001`
- Admin: `https://localhost:5002`
- MySQL: `localhost:3306`

### 배포
- Docker 컨테이너화
- Azure App Service
- AWS Elastic Beanstalk
- 또는 VPS (Ubuntu + Nginx)
