# 데이터베이스 스키마 설계

## 개요
유튜버 키우기 게임의 MySQL 데이터베이스 스키마

## 데이터베이스 구조

### 1. users (사용자 테이블)
```sql
CREATE TABLE users (
    user_id VARCHAR(36) PRIMARY KEY,
    player_name VARCHAR(50) NOT NULL,
    channel_name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE,
    password_hash VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_email (email)
);
```

### 2. player_data (플레이어 게임 데이터)
```sql
CREATE TABLE player_data (
    user_id VARCHAR(36) PRIMARY KEY,
    subscribers BIGINT DEFAULT 0,
    total_views BIGINT DEFAULT 0,
    channel_power BIGINT DEFAULT 0,
    gold BIGINT DEFAULT 1000,
    gems INT DEFAULT 100,
    gacha_tickets INT DEFAULT 10,
    exp_chips INT DEFAULT 0,
    studio_level INT DEFAULT 1,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    INDEX idx_subscribers (subscribers),
    INDEX idx_channel_power (channel_power)
);
```

### 3. characters (캐릭터 마스터 데이터)
```sql
CREATE TABLE characters (
    character_id VARCHAR(50) PRIMARY KEY,
    character_name VARCHAR(50) NOT NULL,
    rarity ENUM('C', 'B', 'A', 'S') NOT NULL,
    specialty ENUM('Filming', 'Editing', 'Planning', 'Design') NOT NULL,
    base_filming INT DEFAULT 0,
    base_editing INT DEFAULT 0,
    base_planning INT DEFAULT 0,
    base_design INT DEFAULT 0,
    passive_skill_desc TEXT,
    passive_skill_value FLOAT DEFAULT 0,
    INDEX idx_rarity (rarity)
);
```

### 4. player_characters (플레이어 보유 캐릭터)
```sql
CREATE TABLE player_characters (
    instance_id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    character_id VARCHAR(50) NOT NULL,
    level INT DEFAULT 1,
    experience INT DEFAULT 0,
    breakthrough INT DEFAULT 0,
    acquired_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (character_id) REFERENCES characters(character_id),
    INDEX idx_user_id (user_id),
    INDEX idx_character_id (character_id)
);
```

### 5. content_genres (콘텐츠 장르 마스터)
```sql
CREATE TABLE content_genres (
    genre_id VARCHAR(50) PRIMARY KEY,
    genre_name VARCHAR(50) NOT NULL,
    unlock_subscribers INT NOT NULL,
    production_time_minutes FLOAT NOT NULL,
    filming_weight FLOAT DEFAULT 0.25,
    editing_weight FLOAT DEFAULT 0.25,
    planning_weight FLOAT DEFAULT 0.25,
    design_weight FLOAT DEFAULT 0.25,
    base_views_multiplier INT DEFAULT 100,
    base_revenue_per_view FLOAT DEFAULT 0.5
);
```

### 6. player_contents (플레이어 제작/업로드 콘텐츠)
```sql
CREATE TABLE player_contents (
    content_id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    genre_id VARCHAR(50) NOT NULL,
    quality ENUM('D', 'C', 'B', 'A', 'S') NOT NULL,
    total_stats INT NOT NULL,
    views BIGINT DEFAULT 0,
    revenue BIGINT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    uploaded_at TIMESTAMP NULL,
    is_uploaded BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (genre_id) REFERENCES content_genres(genre_id),
    INDEX idx_user_id (user_id),
    INDEX idx_genre_id (genre_id),
    INDEX idx_uploaded (is_uploaded),
    INDEX idx_views (views)
);
```

### 7. content_team_members (콘텐츠 제작 팀원)
```sql
CREATE TABLE content_team_members (
    content_id VARCHAR(36),
    character_instance_id VARCHAR(36),
    PRIMARY KEY (content_id, character_instance_id),
    FOREIGN KEY (content_id) REFERENCES player_contents(content_id) ON DELETE CASCADE,
    FOREIGN KEY (character_instance_id) REFERENCES player_characters(instance_id) ON DELETE CASCADE
);
```

### 8. equipment (장비)
```sql
CREATE TABLE equipment (
    user_id VARCHAR(36),
    equipment_type ENUM('Camera', 'PC', 'Microphone', 'Lighting', 'Tablet'),
    level INT DEFAULT 1,
    PRIMARY KEY (user_id, equipment_type),
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);
```

### 9. unlocked_genres (해금된 장르)
```sql
CREATE TABLE unlocked_genres (
    user_id VARCHAR(36),
    genre_id VARCHAR(50),
    unlocked_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, genre_id),
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (genre_id) REFERENCES content_genres(genre_id)
);
```

### 10. rankings (랭킹)
```sql
CREATE TABLE rankings (
    ranking_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    ranking_type ENUM('weekly_views', 'channel_power') NOT NULL,
    score BIGINT NOT NULL,
    rank_position INT NOT NULL,
    week_start DATE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    INDEX idx_ranking_type (ranking_type),
    INDEX idx_week (week_start),
    INDEX idx_rank (rank_position)
);
```

### 11. gacha_history (가챠 히스토리)
```sql
CREATE TABLE gacha_history (
    gacha_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    character_id VARCHAR(50) NOT NULL,
    rarity ENUM('C', 'B', 'A', 'S') NOT NULL,
    drawn_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (character_id) REFERENCES characters(character_id),
    INDEX idx_user_id (user_id),
    INDEX idx_drawn_at (drawn_at)
);
```

### 12. purchases (인앱 결제 기록)
```sql
CREATE TABLE purchases (
    purchase_id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    product_id VARCHAR(50) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    purchased_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    INDEX idx_user_id (user_id),
    INDEX idx_purchased_at (purchased_at)
);
```

## 초기 데이터

### 기본 캐릭터 데이터 삽입 예시
```sql
INSERT INTO characters (character_id, character_name, rarity, specialty, base_filming, base_editing, base_planning, base_design) VALUES
('char_001', '김촬영', 'C', 'Filming', 30, 10, 10, 10),
('char_002', '박편집', 'C', 'Editing', 10, 30, 10, 10),
('char_003', '이기획', 'C', 'Planning', 10, 10, 30, 10),
('char_004', '최디자인', 'C', 'Design', 10, 10, 10, 30);
```

### 콘텐츠 장르 데이터 삽입
```sql
INSERT INTO content_genres (genre_id, genre_name, unlock_subscribers, production_time_minutes) VALUES
('vlog', '브이로그', 0, 30),
('gaming', '게임', 1000, 60),
('mukbang', '먹방', 5000, 45),
('education', '교육', 20000, 90),
('shorts', '쇼츠', 50000, 15),
('documentary', '다큐', 200000, 240);
```

## 인덱스 최적화
- 자주 조회되는 컬럼에 인덱스 생성
- user_id, character_id, content_id 등 FK에 인덱스
- 랭킹 조회를 위한 복합 인덱스

## 백업 전략
- 일일 자동 백업
- 중요 이벤트 발생 시 즉시 백업
- 30일 백업 데이터 보관
