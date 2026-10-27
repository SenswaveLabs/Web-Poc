# Lokalne uruchomienie Senswave (backend + web)

Instrukcja uruchamia pełną aplikację w Dockerze: API, aplikację web i seed z danymi testowymi.

Przebieg: w każdym repozytorium wejdź do katalogu `docker` i zbuduj obrazy, potem wróć tutaj i uruchom całość.

- backend: https://github.com/SenswaveLabs/Backend
- frontend: https://github.com/SenswaveLabs/Web-Poc

Kod backendu nie wymaga zmian. Pliki uruchomieniowe są w tym katalogu (`deploy` w repozytorium Web-Poc).

## Wymagania

- Docker Desktop z Docker Compose.
- Git.
- Wolne porty: `7162` (web), `7070` (API), `7777` (podgląd e-maili).

## 1. Pobierz oba repozytoria

```sh
git clone https://github.com/SenswaveLabs/Backend.git
git clone https://github.com/SenswaveLabs/Web-Poc.git
```

Obrazy budujemy zawsze z gałęzi `main` w obu repozytoriach. `git clone` pobiera `main` domyślnie.
Jeśli repozytorium było już pobrane, przed budowaniem wykonaj w nim `git checkout main` i `git pull`.

## 2. Zbuduj obrazy backendu

```sh
cd Backend/docker
cp .env.template .env
docker compose build senswave.api senswave.seed
cd ../..
```

Powstają obrazy `senswave-senswave.api` i `senswave-senswave.seed`. Workera nie budujemy, rozwiązanie go nie używa.
Ten krok tylko buduje obrazy. Nie uruchamiaj tu `docker compose up`.

## 3. Zbuduj obraz frontendu

```sh
cd Web-Poc/docker
cp .env.template .env
docker compose build
cd ../..
```

Powstaje obraz `sesnwave-web-senswave.web`.

## 4. Przygotuj `.env` do uruchomienia

```sh
cd Web-Poc/deploy
cp .env.template .env
```

Plik [`docker-compose.yml`](docker-compose.yml) uruchamia: web, API, seed, PostgreSQL i smtp4dev. Bez RabbitMQ i bez workera.
Używa obrazów z kroków 2 i 3, niczego nie buduje. Nazwy obrazów są w `.env` (`API_IMAGE`, `SEED_IMAGE`, `WEB_IMAGE`).
Wartości domyślne działają bez zmian, poza logowaniem Google. Plik `.env` jest w `.gitignore`.


## 5. Uruchom

W katalogu `Web-Poc/deploy`:

```sh
docker compose up -d
```

Kolejność startu: baza, potem API, potem web i seed.

Seed startuje sam, gdy API jest gotowe. Po kilku sekundach kończy pracę i kontener `Senswave.Local.Seed` ma status `Exited (0)`.
Przebieg seeda:

```sh
docker logs Senswave.Local.Seed
```

Linie „Initial seed finished successfully.” i „Initial seed finished.” na końcu logu oznaczają, że wszystko się udało.

Konta po seedzie:

| E-mail | Hasło | Rola |
|---|---|---|
| `user@gmail.com` | `User123456!` | user, z domem „Test Home”, brokerem i urządzeniami |
| `admin@gmail.com` | `Admin123456!` | admin |
| `user1@gmail.com` | `User123456!` | user |

Seed działa tylko raz. Przy kolejnych `docker compose up` loguje „Seed already done, skipping”, żeby nie tworzyć duplikatów.
Aby zacząć od pustej bazy i uruchomić seed ponownie:

```sh
docker compose down -v
docker compose up -d
```

## Adresy

| Usługa | Adres |
|---|---|
| Aplikacja web | http://localhost:7162 |
| API | http://localhost:7070 |
| E-maile (smtp4dev) | http://localhost:7777 |

Rejestracja wysyła e-mail z kodem potwierdzenia. Odczytaj go w smtp4dev.

## CORS

API przyjmuje żądania tylko z adresów z listy `Cors__AllowedOrigins__*` w `.env`.
Domyślnie jest tam `http://localhost:7162` – web w Dockerze.

## Aktualizacja

Po zmianach w kodzie: `git pull` w danym repozytorium, potem ponów krok 2 lub 3.
Na koniec w `Web-Poc/deploy`:

```sh
docker compose up -d
```

## Zatrzymanie i czyszczenie

```sh
docker compose stop          # zatrzymuje, zachowuje dane
docker compose down          # usuwa kontenery, zachowuje dane
docker compose down -v       # usuwa kontenery, bazę danych i znacznik seeda
```
