# wedding-site

## API de convidados

O backend fica isolado em `backend/` e dividido em projetos:

- `WeddingGuests.Domain`: entidades e regras centrais do domínio.
- `WeddingGuests.Application`: casos de uso, validações, contratos e interfaces dos repositórios.
- `WeddingGuests.Repository`: persistência SQLite/EF Core e implementações das interfaces.
- `WeddingGuests.Api`: rotas HTTP, autenticação administrativa e composição das dependências.

As dependências apontam para dentro: `Api` usa `Application` e `Repository`; `Repository` implementa contratos de `Application` e persiste entidades de `Domain`; `Application` usa `Domain`. A tela principal e os arquivos do frontend não são conectados nesta etapa.

No PowerShell, configure uma chave administrativa e inicie a API:

```powershell
$env:GUEST_ADMIN_KEY = "defina-uma-chave-segura"
dotnet run --project backend/WeddingGuests.Api
```

O banco é criado em `backend/WeddingGuests.Api/data/guests.db`. Para mudar o caminho, defina `GUESTS_DB_PATH`. A API disponibiliza `GET /health`, `GET /api/guests/lookup?name=...` e rotas administrativas sob `/api/admin/guests`, que exigem o cabeçalho `X-Admin-Key`.

Rotas administrativas: `GET /api/admin/guests`, `GET /api/admin/guests/{id}`, `POST /api/admin/guests`, `PATCH /api/admin/guests/{id}`, `DELETE /api/admin/guests/{id}`, `POST /api/admin/guests/{id}/companions` e `DELETE /api/admin/guests/{id}/companions/{personId}`.