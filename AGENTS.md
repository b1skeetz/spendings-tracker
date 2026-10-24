# AGENTS.md

Инструкции для AI-агентов (Claude Code, Codex, Cursor и др.), которые работают с этим репозиторием.
Подробная документация для людей — в [README.md](README.md): архитектура, пошаговый пример добавления сущности, порты.

## Проект

- `backend/` — ASP.NET Core 9 по Clean Architecture: `src/TemplateProject.{Domain,Application,Infrastructure,Api}`, решение `backend/TemplateProject.sln`.
- `frontend/` — Vue 3 + Vite. Его конвенции описаны в [frontend/FRONTEND.md](frontend/FRONTEND.md). **Не меняйте фронтенд, если задача прямо этого не требует.**
- БД: PostgreSQL. При старте API применяет миграции и сидит роли и администратора (`Infrastructure/Persistence/DatabaseInitializer.cs`).
- Аутентификация: короткоживущий JWT (claims `sub`, `email`, `role`) плюс refresh-токен в HttpOnly-cookie с ротацией (`/api/connect/token`, `/api/connect/logout`). Логика лежит в `Application/Features/Auth` и `Infrastructure/Identity/RefreshTokenService.cs`, а cookie обрабатывается в `Api/Endpoints/Auth/RefreshTokenCookie.cs`. Подробности в README, раздел «Авторизация».

## Команды

```bash
dotnet build backend/TemplateProject.sln
dotnet run --project backend/src/TemplateProject.Api --launch-profile https   # https://localhost:7262/swagger
dotnet ef migrations add <Name> --project backend/src/TemplateProject.Infrastructure --startup-project backend/src/TemplateProject.Api --output-dir Persistence/Migrations
dotnet ef migrations has-pending-model-changes --project backend/src/TemplateProject.Infrastructure --startup-project backend/src/TemplateProject.Api
```

## Правила архитектуры (обязательные)

Зависимости проектов: `Api → Infrastructure → Application → Domain`. Ссылки в обратную сторону запрещены.

| Слой | Можно | Нельзя |
|---|---|---|
| Domain | чистый C#: сущности, value objects, enum'ы, `DomainException`, константы | любые NuGet-пакеты, EF-атрибуты, ссылки на другие проекты |
| Application | use case'ы (MediatR), интерфейсы в `Common/Abstractions`, `ApiErrors`, DTO | EF Core, `DbContext`, ASP.NET Core, `HttpContext`, `IResult`, HTTP-статусы |
| Infrastructure | реализации интерфейсов Application, EF-конфигурации, репозитории, миграции, Identity, JWT | эндпоинты, бизнес-правила |
| Api | эндпоинты (`IFeatureEndpoint`), HTTP-модели запросов, маппинг ошибок в HTTP, DI | бизнес-логика, прямое использование `DatabaseContext` и репозиториев в эндпоинтах |

- Нужна внешняя возможность в Application (БД, почта, время, файлы)? Объявите интерфейс в `Application/Common/Abstractions`, реализуйте его в Infrastructure (или в Api, если он завязан на HTTP) и зарегистрируйте в `DependencyInjection.cs` этого слоя.
- Новые пакеты добавляйте в самый внешний слой, которому они нужны. В Domain пакеты не добавляются.
- Общие свойства (`TargetFramework`, `Nullable`, `ImplicitUsings`) задаются в `backend/Directory.Build.props`. Не дублируйте их в `.csproj`.

## Как писать код

- **Use case** — файл `Application/Features/<Фича>/<Имя>Command.cs` (или `Query`) со `static class`, внутри вложенные `Request : IRequest<BaseApiResponse<Response>>`, `Response`, `Handler`. Образец: `Features/Auth/LoginCommand.cs`.
- Ожидаемые ошибки возвращайте как `ApiErrors.*` (неявное приведение к `BaseApiResponse<T>`), не бросайте исключения.
- Добавили новый тип в `ApiErrors`? **Добавьте его маппинг в `Api/Filters/ApiErrorResultMapper.cs`** (оба метода: `GetStatusCode` и `ToResult`), иначе запрос упадёт с 500.
- Внутренние операции без HTTP-смысла возвращают `ErrorOr<T>` / `ErrorOrNone` с ошибкой от `BaseError` (`Application/Common/Errors/Errors.cs`).
- Не используйте `Unit` как тип ответа handler'а: `MediatR.Unit` и `Application.Common.Models.Unit` конфликтуют.
- **Эндпоинт** — класс `IFeatureEndpoint` в `Api/Endpoints/<Фича>/`. Он регистрируется автоматически. Эндпоинт только собирает `Request` и вызывает `mediator.Send`. Указывайте `.WithName`, `.WithTags`, `.WithOpenApi()` и, если нужно, `.RequireAuthorization(AuthorizationPolicy.X)`.
- **Сущность** — `Domain/Entities`: `private set`, приватный конструктор для EF, фабричный метод, правила в методах. EF-конфигурация в `Infrastructure/Persistence/Configurations` (`IEntityTypeConfiguration<T>`), подключается в `DatabaseContext.ConfigureBusinessTables`. Доступ к данным — через интерфейс репозитория в Application и реализацию в `Infrastructure/Persistence/Repositories`. Сохранение — через `IUnitOfWork`.
- Handler'ы MediatR и профили AutoMapper регистрируются сканированием сборки Application. Репозитории и сервисы регистрируются вручную в `AddInfrastructure` / `AddApi`.
- Стиль: внедрение через конструктор, поля `private readonly _camelCase`, время через `TimeProvider`, комментарии на русском, `[PublicAPI]` на публичных контрактах фич.
- Пошаговый пример полного вертикального среза (сущность → миграция → эндпоинт) — в README, раздел «Как добавить новую сущность».

## Проверка перед завершением задачи

1. `dotnet build backend/TemplateProject.sln` без ошибок. Допустимое предупреждение одно: NU1903 про AutoMapper, оно было и раньше.
2. Если менялась модель EF, создана миграция, и `has-pending-model-changes` отвечает «No changes».
3. Если менялось поведение API, запустите проект и проверьте эндпоинт через Swagger или curl (`curl -k` из-за dev-сертификата).
4. Если менялся контракт API, проверьте, не сломался ли фронтенд (`frontend/app/src/services/http/`).

## Окружение и подводные камни

- Разработка ведётся на Windows. Путь к проекту содержит кириллицу; если инструмент ломается на пути, проверьте экранирование.
- `dotnet-ef` нужен **версии 9.x**. Версия 10 без рантайма .NET 10 падает с `hostfxr.dll not found`. Если .NET установлен в пользовательскую папку, apphost'у может понадобиться `DOTNET_ROOT`.
- Dev-сертификат HTTPS может быть не доверенным. Для curl используйте `-k`. Доверие (`dotnet dev-certs https --trust`) настраивает пользователь сам.
- **Не поднимайте Docker-контейнеры**, если об этом не попросили. Используйте локальный PostgreSQL.
- `backend/src/TemplateProject.Api/appsettings.Development.json` содержит реальные креды к БД. Не коммитьте его, не выводите его содержимое и не переносите значения в другие файлы. Если кредов нет, попросите пользователя вписать их.
- Порты: API на https 7262 / http 5228, а фронтенд проксирует `/api` на `http://localhost:5000`. Это известное расхождение, подробности в README, раздел «Порты и связь с фронтендом». Для сквозной проверки через фронтенд запускайте API так: `dotnet run --project backend/src/TemplateProject.Api --launch-profile https -- --urls "https://localhost:7262;http://localhost:5000"`.
- Фронтенд: зависимости ставьте через `npm ci` (lock-файл не меняется). Проверки: `npx vue-tsc --noEmit` и `npx eslint <файлы>`. `prettier --check` на Windows ругается на все файлы из-за CRLF после checkout, поэтому проверяйте с `--end-of-line auto`.
- В dev-режиме Vite при первом открытии страницы с новыми зависимостями сам перезагружает её («optimized dependencies changed. reloading»). Если при UI-тесте форма «сбросилась», откройте страницу повторно.
- Для проверки refresh-токена удобно временно задать короткий срок жизни access-токена через переменную окружения `Auth__Lifetime=00:00:20`.
- Коммитьте только по просьбе пользователя.
