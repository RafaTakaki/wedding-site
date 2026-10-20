# wedding-site

## API de convidados

O backend fica isolado em `backend/` e dividido em projetos:

- `WeddingGuests.Domain`: entidades e regras centrais do domínio.
- `WeddingGuests.Application`: casos de uso, validações, contratos e interfaces dos repositórios.
- `WeddingGuests.Repository`: persistência Firestore e implementações das interfaces.
- `WeddingGuests.Api`: rotas HTTP, autenticação administrativa e composição das dependências.

As dependências apontam para dentro: `Api` usa `Application` e `Repository`; `Repository` implementa contratos de `Application` e persiste entidades de `Domain`; `Application` usa `Domain`. A tela principal e os arquivos do frontend não são conectados nesta etapa.

No PowerShell, configure uma chave administrativa e inicie a API:

```powershell
$env:GUEST_ADMIN_KEY = "defina-uma-chave-segura"
$env:FIREBASE_PROJECT_ID = "id-do-projeto-firebase"
$env:RSVP_DEADLINE = "2027-06-06T23:59:00-03:00"
dotnet run --project backend/WeddingGuests.Api
```

No Firebase Console, crie um projeto, habilite o Cloud Firestore em modo nativo e configure a região do banco. A raiz do repositório possui um `.env` local ignorado pelo Git; a API o carrega automaticamente ao iniciar. Edite `FIREBASE_PROJECT_ID` e `GUEST_ADMIN_KEY` nesse arquivo conforme seu ambiente. Localmente, autentique com `gcloud auth application-default login` ou defina `GOOGLE_APPLICATION_CREDENTIALS` apontando para um arquivo JSON de conta de serviço armazenado fora do repositório. A conta precisa das permissões necessárias para ler e gravar no Firestore. Não coloque chaves de serviço no frontend nem as versione.

Os convites são armazenados na coleção `guestInvitations`; índices de RSVP e contadores ficam em `rsvpTokens` e `system`. O backend usa transações do Firestore para reservar IDs e salvar respostas. A API disponibiliza `GET /health`, `GET /api/guests/lookup?name=...` e rotas administrativas sob `/api/admin/guests`, que exigem o cabeçalho `X-Admin-Key`.


Sirva `frontend/` em `http://localhost:5500` (por exemplo, com Live Server no VS Code). As páginas de confirmação e administração usam a API em `http://localhost:5000`; configure `FRONTEND_ORIGINS` se o frontend for servido de outra origem. O valor de `RSVP_DEADLINE` deve incluir fuso horário. O frontend administrativo pede a chave em tempo de execução e não a armazena.

Rotas públicas: `GET /api/guests/lookup?name=...` e `POST /api/guests/rsvp/{rsvpToken}`. O token aleatório retornado na busca autoriza o envio das respostas; o backend aplica o prazo e impede alteração pública depois de definida a resposta. Rotas administrativas: `GET /api/admin/guests`, `GET /api/admin/guests/{id}`, `POST /api/admin/guests`, `PATCH /api/admin/guests/{id}`, `DELETE /api/admin/guests/{id}`, `POST /api/admin/guests/{id}/companions` e `DELETE /api/admin/guests/{id}/companions/{personId}`. Todas as rotas administrativas exigem `X-Admin-Key`.

A página `frontend/pages/presentes.html` ainda usa comportamento demonstrativo no navegador; a integração de presentes depende de endpoints próprios, que ainda não fazem parte desta API.