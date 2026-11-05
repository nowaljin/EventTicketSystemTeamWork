-- ============================================================
-- イベント・チケット予約管理システム DB セットアップ
-- MySQL 8.x
-- ============================================================

DROP DATABASE IF EXISTS event_system;
CREATE DATABASE event_system
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE event_system;

-- ------------------------------------------------------------
-- users
-- ------------------------------------------------------------
CREATE TABLE users (
    user_id    INT          NOT NULL AUTO_INCREMENT,
    user_code  VARCHAR(20)  NOT NULL UNIQUE,
    name       VARCHAR(50)  NOT NULL,
    email      VARCHAR(100) NOT NULL,
    role       VARCHAR(20)  NOT NULL DEFAULT 'user',   -- user / admin
    created_at DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id)
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- categories
-- ------------------------------------------------------------
CREATE TABLE categories (
    category_id   INT         NOT NULL AUTO_INCREMENT,
    category_name VARCHAR(50) NOT NULL,
    PRIMARY KEY (category_id)
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- events
-- ------------------------------------------------------------
CREATE TABLE events (
    event_id    INT          NOT NULL AUTO_INCREMENT,
    category_id INT          NOT NULL,
    event_name  VARCHAR(100) NOT NULL,
    description TEXT,
    event_date  DATETIME     NOT NULL,
    venue       VARCHAR(100) NOT NULL,
    price       INT          NOT NULL DEFAULT 0,
    capacity    INT          NOT NULL DEFAULT 1,
    status      VARCHAR(20)  NOT NULL DEFAULT '公開中',  -- 公開中 / 公開停止 / 終了
    created_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (event_id),
    FOREIGN KEY (category_id) REFERENCES categories(category_id)
        ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- reservations
-- ------------------------------------------------------------
CREATE TABLE reservations (
    reservation_id   INT          NOT NULL AUTO_INCREMENT,
    reservation_code VARCHAR(30)  NOT NULL UNIQUE,
    user_id          INT          NOT NULL,
    event_id         INT          NOT NULL,
    quantity         INT          NOT NULL DEFAULT 1,
    total_price      INT          NOT NULL DEFAULT 0,
    status           VARCHAR(20)  NOT NULL DEFAULT '予約済',  -- 予約済 / キャンセル済
    reserved_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (reservation_id),
    FOREIGN KEY (user_id)  REFERENCES users(user_id)   ON UPDATE CASCADE ON DELETE RESTRICT,
    FOREIGN KEY (event_id) REFERENCES events(event_id)  ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB;

-- ============================================================
-- 初期データ
-- ============================================================

INSERT INTO categories (category_name) VALUES
  ('ゲーム'), ('音楽'), ('スポーツ'), ('セミナー'), ('その他');

-- 利用者 (user_code 例: U000001 ...)
INSERT INTO users (user_code, name, email, role) VALUES
  ('U000001', '山田 太郎', 'taro@example.com',   'user'),
  ('U000002', '佐藤 花子', 'hanako@example.com', 'user'),
  ('U000003', '鈴木 一郎', 'ichiro@example.com','user'),
  ('A000001', '管理者',     'admin@example.com',  'admin');

-- イベント
INSERT INTO events (category_id, event_name, description, event_date, venue, price, capacity, status) VALUES
  (1, 'eスポーツ大会 2026', '最新タイトルによるeスポーツ大会。', '2026-12-10 13:00:00', '東京ドーム', 3000, 100, '公開中'),
  (2, '秋の音楽フェス', '野外音楽フェスティバル。', '2026-11-20 18:00:00', '代々木公園', 5000, 200, '公開中'),
  (3, '市民マラソン', '10km/ハーフマラソン。', '2026-10-05 08:00:00', '皇居周回', 2000, 500, '終了'),
  (4, 'AIセミナー', '生成AI活用セミナー。', '2026-12-01 10:00:00', '渋谷カンファレンス', 1000, 50, '公開中');

-- サンプル予約
INSERT INTO reservations (reservation_code, user_id, event_id, quantity, total_price, status) VALUES
  ('R202611050001', 1, 1, 2, 6000, '予約済'),
  ('R202611050002', 2, 2, 3, 15000, '予約済');
