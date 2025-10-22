-- CREATE TABLE for SQLite
CREATE TABLE Users (
    UserID INTEGER PRIMARY KEY,
    Username TEXT,
    Email TEXT,
    PasswordHash TEXT,
    Role TEXT DEFAULT 'user'
);
