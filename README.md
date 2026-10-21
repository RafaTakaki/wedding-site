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

Rotas públicas: `GET /api/guests/lookup?name=...`, `POST /api/guests/rsvp/{rsvpToken}` e `POST /api/guests/rsvp/{rsvpToken}/companions`. O token aleatório retornado na busca autoriza o envio de respostas e a inclusão de acompanhante livre; o backend aplica o prazo e o limite do convite, e impede alteração pública de uma resposta já definida. Rotas administrativas: `GET /api/admin/guests`, `GET /api/admin/guests/{id}`, `POST /api/admin/guests`, `PATCH /api/admin/guests/{id}`, `PATCH /api/admin/guests/{id}/people/{inviteeId}/status`, `DELETE /api/admin/guests/{id}`, `POST /api/admin/guests/{id}/companions` e `DELETE /api/admin/guests/{id}/companions/{personId}`. Todas as rotas administrativas exigem `X-Admin-Key`.

A página `frontend/pages/presentes.html` carrega a lista da coleção Firestore `gifts`. A escolha é reservada atomicamente e registra o nome para a administração; o nome não é exposto na rota pública. A administração lista presentes em `GET /api/admin/gifts`, cadastra em `POST /api/admin/gifts` e libera uma escolha em `POST /api/admin/gifts/{id}/release`. A rota pública lista em `GET /api/gifts` e registra a intenção em `POST /api/gifts/{id}/claim`. Cadastre os itens simbólicos pelo painel autenticado. O status `gifted` representa que alguém escolheu o item, não que o pagamento foi recebido. Este fluxo não recebe dinheiro nem inicia checkout. PagSeguro/PagBank permanece desativado.

### PagSeguro/PagBank (planejado)

Há um serviço inicial e o endpoint `GET /api/payments/pagseguro/status` para inspecionar o estado da integração. Ele não cria cobranças nem chama o PagBank. O `.env` mantém `PAGSEGURO_ENABLED=false` e `PAGSEGURO_ENVIRONMENT=sandbox`; não preencha token até concluir o cadastro e obter credenciais de Sandbox. O checkout real será implementado depois de definidos o produto/valor e as URLs de retorno e após validar a conta e as credenciais. Nunca coloque o token no frontend ou no Git.