# Docker Compose запуск (Front + API + SQL Server + Redis)

## 1) Підготовка шляху до фронта

За замовчуванням `docker-compose.yml` очікує, що фронт лежить у сусідній папці `../auction_front`.

Якщо у вас інший шлях (наприклад `D:/nulp_lab/auction_front`):

1. Скопіюйте `.env.docker.example` у `.env`
2. Вкажіть правильний `FRONTEND_CONTEXT`

## 2) Запуск одним натисканням / командою

```bash
docker compose up --build -d
```

## 3) Де відкривати

- Frontend: `http://localhost:3000`
- Backend API: `http://localhost:5041`
- Swagger: `http://localhost:5041/swagger`

## 4) Що вже налаштовано

- API підключається до SQL Server по імені сервісу `sqlserver`
- API підключається до Redis по імені сервісу `redis`
- Автоміграції БД запускаються при старті API (з ретраями, поки БД підіймається)
- HTTPS-redirect вимкнений для контейнерного запуску, щоб не було зайвих редіректів всередині compose

## 5) Зупинка

```bash
docker compose down
```

Щоб видалити також томи БД/Redis:

```bash
docker compose down -v
```
