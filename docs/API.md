# REST API 설계

## 개요
유튜버 키우기 게임의 서버 API 명세

**Base URL**: `https://api.youtubertycoon.com/v1`

## 인증
모든 API 요청은 헤더에 JWT 토큰 포함 필요
```
Authorization: Bearer {token}
```

---

## 1. 인증 (Authentication)

### 1.1 회원가입
```
POST /auth/register
```

**Request Body**:
```json
{
    "email": "user@example.com",
    "password": "password123",
    "player_name": "플레이어",
    "channel_name": "내 채널"
}
```

**Response**:
```json
{
    "success": true,
    "user_id": "uuid-string",
    "token": "jwt-token"
}
```

### 1.2 로그인
```
POST /auth/login
```

**Request Body**:
```json
{
    "email": "user@example.com",
    "password": "password123"
}
```

**Response**:
```json
{
    "success": true,
    "user_id": "uuid-string",
    "token": "jwt-token"
}
```

---

## 2. 플레이어 데이터 (Player)

### 2.1 플레이어 정보 조회
```
GET /player/me
```

**Response**:
```json
{
    "user_id": "uuid",
    "player_name": "플레이어",
    "channel_name": "내 채널",
    "subscribers": 1000,
    "total_views": 50000,
    "channel_power": 500,
    "gold": 10000,
    "gems": 200,
    "gacha_tickets": 5,
    "exp_chips": 100,
    "studio_level": 2,
    "unlocked_genres": ["vlog", "gaming"]
}
```

### 2.2 플레이어 데이터 저장
```
PUT /player/save
```

**Request Body**:
```json
{
    "gold": 10000,
    "gems": 200,
    "subscribers": 1000,
    "studio_level": 2
}
```

---

## 3. 캐릭터 (Characters)

### 3.1 캐릭터 목록 조회 (마스터 데이터)
```
GET /characters
```

**Response**:
```json
{
    "characters": [
        {
            "character_id": "char_001",
            "character_name": "김촬영",
            "rarity": "C",
            "specialty": "Filming",
            "base_stats": {
                "filming": 30,
                "editing": 10,
                "planning": 10,
                "design": 10
            }
        }
    ]
}
```

### 3.2 보유 캐릭터 조회
```
GET /player/characters
```

**Response**:
```json
{
    "owned_characters": [
        {
            "instance_id": "uuid",
            "character_id": "char_001",
            "level": 5,
            "experience": 250,
            "breakthrough": 1,
            "acquired_at": "2026-02-10T10:00:00Z"
        }
    ]
}
```

### 3.3 가챠 (캐릭터 뽑기)
```
POST /gacha/draw
```

**Request Body**:
```json
{
    "draw_count": 1,
    "use_ticket": true
}
```

**Response**:
```json
{
    "success": true,
    "drawn_characters": [
        {
            "instance_id": "uuid",
            "character_id": "char_005",
            "character_name": "강슈퍼",
            "rarity": "A"
        }
    ],
    "remaining_tickets": 4
}
```

### 3.4 캐릭터 레벨업
```
POST /player/characters/{instance_id}/levelup
```

**Request Body**:
```json
{
    "exp_chips": 100
}
```

**Response**:
```json
{
    "success": true,
    "new_level": 6,
    "remaining_exp": 50
}
```

---

## 4. 콘텐츠 (Content)

### 4.1 콘텐츠 제작 시작
```
POST /content/start
```

**Request Body**:
```json
{
    "genre": "gaming",
    "team_member_ids": ["uuid1", "uuid2", "uuid3"]
}
```

**Response**:
```json
{
    "success": true,
    "content_id": "uuid",
    "estimated_completion": "2026-02-10T11:00:00Z"
}
```

### 4.2 제작 중인 콘텐츠 조회
```
GET /content/producing
```

**Response**:
```json
{
    "producing_contents": [
        {
            "content_id": "uuid",
            "genre": "gaming",
            "quality": "A",
            "total_stats": 300,
            "created_at": "2026-02-10T10:00:00Z",
            "estimated_completion": "2026-02-10T11:00:00Z",
            "is_complete": false
        }
    ]
}
```

### 4.3 콘텐츠 업로드
```
POST /content/{content_id}/upload
```

**Response**:
```json
{
    "success": true,
    "views": 5000,
    "revenue": 2500,
    "subscribers_gained": 50
}
```

### 4.4 업로드된 콘텐츠 조회
```
GET /content/uploaded?limit=20&offset=0
```

**Response**:
```json
{
    "uploaded_contents": [
        {
            "content_id": "uuid",
            "genre": "gaming",
            "quality": "A",
            "views": 5000,
            "revenue": 2500,
            "uploaded_at": "2026-02-10T11:00:00Z"
        }
    ],
    "total_count": 100
}
```

---

## 5. 장비 (Equipment)

### 5.1 장비 정보 조회
```
GET /player/equipment
```

**Response**:
```json
{
    "equipment": {
        "Camera": { "level": 2, "name": "미러리스" },
        "PC": { "level": 1, "name": "노트북" },
        "Microphone": { "level": 1, "name": "이어폰" },
        "Lighting": { "level": 1, "name": "스탠드" },
        "Tablet": { "level": 1, "name": "보급형" }
    }
}
```

### 5.2 장비 업그레이드
```
POST /player/equipment/{type}/upgrade
```

**Request Body**:
```json
{
    "equipment_type": "Camera"
}
```

**Response**:
```json
{
    "success": true,
    "new_level": 3,
    "new_name": "DSLR",
    "remaining_gold": 8000
}
```

---

## 6. 스튜디오 (Studio)

### 6.1 스튜디오 업그레이드
```
POST /player/studio/upgrade
```

**Response**:
```json
{
    "success": true,
    "new_level": 3,
    "remaining_gold": 5000
}
```

---

## 7. 랭킹 (Rankings)

### 7.1 주간 조회수 랭킹
```
GET /rankings/weekly?limit=100
```

**Response**:
```json
{
    "rankings": [
        {
            "rank": 1,
            "player_name": "탑유튜버",
            "channel_name": "인기채널",
            "weekly_views": 1000000
        }
    ],
    "my_rank": 42,
    "my_weekly_views": 50000
}
```

### 7.2 채널 파워 랭킹
```
GET /rankings/channel-power?limit=100
```

**Response**:
```json
{
    "rankings": [
        {
            "rank": 1,
            "player_name": "파워유튜버",
            "channel_name": "강한채널",
            "channel_power": 5000
        }
    ],
    "my_rank": 50,
    "my_channel_power": 800
}
```

---

## 8. 일일 트렌드 (Daily Trend)

### 8.1 오늘의 트렌드 조회
```
GET /trend/today
```

**Response**:
```json
{
    "trend_genre": "gaming",
    "bonus_percentage": 50,
    "next_change": "2026-02-11T00:00:00Z"
}
```

---

## 9. 인앱 결제 (Purchase)

### 9.1 결제 검증
```
POST /purchase/verify
```

**Request Body**:
```json
{
    "product_id": "gems_100",
    "receipt": "base64-encoded-receipt",
    "platform": "android"
}
```

**Response**:
```json
{
    "success": true,
    "gems_added": 100,
    "new_gems_total": 300
}
```

---

## 에러 코드

| 코드 | 메시지 | 설명 |
|------|--------|------|
| 1000 | Invalid token | 잘못된 인증 토큰 |
| 1001 | Insufficient gold | 골드 부족 |
| 1002 | Insufficient gems | 보석 부족 |
| 1003 | Character not found | 캐릭터를 찾을 수 없음 |
| 1004 | Content not ready | 콘텐츠가 아직 완성되지 않음 |
| 1005 | Already max level | 이미 최대 레벨 |
| 2000 | Server error | 서버 오류 |

## 응답 형식

### 성공
```json
{
    "success": true,
    "data": { ... }
}
```

### 실패
```json
{
    "success": false,
    "error_code": 1001,
    "message": "Insufficient gold"
}
```

## 보안
- HTTPS 필수
- JWT 토큰 기반 인증
- Rate limiting: 1000 requests/hour per user
- SQL Injection 방어
- XSS 방어
