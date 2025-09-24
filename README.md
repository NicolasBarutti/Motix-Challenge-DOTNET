Motix

API RESTful (.NET 8) para gestão de Setores, Motos e Movimentações.
Foco em boas práticas REST: status codes, paginação, HATEOAS, Swagger/OpenAPI e arquitetura em camadas.

Integrantes

Nicolas Candido Barutti Jacuck 1 – RM 554944

Kleber da Silva  2 – RM 557887

Lucas Rainha 3 – RM 558471

Substitua pelos nomes/RM da equipe.

Justificativa do domínio

Escolhemos um domínio simples e funcional que ainda permite relações reais:

Sector (Id, Code) – área/zona.

Motorcycle (Id, Plate, SectorId) – veículo associado a um setor.

Movement (Id, MotorcycleId, SectorId, OccurredAt) – histórico de movimentação da moto entre setores.

É um cenário pequeno, porém cobre 3 entidades, relacionamentos 1-N e operações CRUD completas, além de um caso de uso comum (registrar movimentações). Isso permite demonstrar paginação, HATEOAS, códigos de status, DTOs e testes de integração.

Arquitetura

Camadas do projeto

src/
  Motix/               -> API (Controllers, Program, Swagger)
  Motix.Domain/        -> Entidades + Exception básica
  Motix.Infrastructure/-> EF Core + DbContext + Repositórios/DI
  Motix.Application/   -> DTOs e UseCases (se necessário)
  Tests/
    Motix.Tests/       -> Testes (xUnit + InMemory + WebApplicationFactory)


Principais decisões

Camadas separadas para isolar responsabilidades (Domain, Infrastructure, Application e API).

Entity Framework Core com Oracle (Oracle.EntityFrameworkCore) para persistência.

Swagger/OpenAPI habilitado (descrição, exemplos de payloads e modelos).

Paginação via query ?page=&pageSize= + objeto PagedResult<T>.

HATEOAS: cada item retorna uma coleção _links com ações relacionadas.

DTOs para entrada e saída, separando contratos da API das entidades.

Testes de integração usando Microsoft.AspNetCore.Mvc.Testing e EF InMemory (sem depender do Oracle em CI).

Requisitos & Tecnologias

.NET 8 SDK

Oracle DB (string de conexão fornecida pela FIAP)

Pacotes principais:

Oracle.EntityFrameworkCore

Swashbuckle.AspNetCore

Testes: xunit, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing, Microsoft.EntityFrameworkCore.InMemory

Configuração
1) Connection string

Arquivo appsettings.json (projeto Motix):

{
  "ConnectionStrings": {
    "Default": "Data Source=oracle.fiap.com.br:1521/orcl;User ID=RMXXXXX;Password=SENHA;"
  }
}


Importante (Oracle): as PKs Guid são mapeadas para RAW(16); no banco, você verá os IDs em hex sem hífens.
Se fizer consultas manuais, lembre-se de que as colunas foram criadas com aspas (ex.: "Id").

2) Migrations (opcional se já estiver com o banco criado)

No diretório raiz da solução:

# adicionar migration
dotnet ef migrations add InitialCreate --project src/Motix.Infrastructure/Motix.Infrastructure.csproj --startup-project src/Motix/Motix.csproj

# aplicar no banco
dotnet ef database update --project src/Motix.Infrastructure/Motix.Infrastructure.csproj --startup-project src/Motix/Motix.csproj

Como executar
# restaurar pacotes e compilar
dotnet restore
dotnet build

# rodar a API
dotnet run --project src/Motix/Motix.csproj


A API sobe (por padrão) em https://localhost:7040 (ou porta similar).
Acesse o Swagger em:

https://localhost:7040/swagger

Endpoints & Exemplos
1) Sectors

POST /api/sectors

{
  "code": "A1"
}


GET /api/sectors?page=1&pageSize=10

Resposta (200):

{
  "items": [
    { "data": { "id": "guid", "code": "A1" }, "_links": [...] }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1
}


GET /api/sectors/{id} → 200/404
PUT /api/sectors/{id} → 204/404
DELETE /api/sectors/{id} → 204/404

2) Motorcycles

POST /api/motorcycles

{
  "plate": "ABC1D23",
  "sectorId": "GUID_DO_SETOR"
}


Se o sectorId não existir, retorna 400 com mensagem clara (violação de FK).

GET /api/motorcycles?page=1&pageSize=10
GET /api/motorcycles/{id}
PUT /api/motorcycles/{id}
DELETE /api/motorcycles/{id}

3) Movements

POST /api/movements

{
  "motorcycleId": "GUID_DA_MOTO",
  "sectorId": "GUID_DO_SETOR"
}


Retorna 201 Created com occurredAt (UTC).
Também há GET /api/movements (paginado), GET /api/movements/{id} e DELETE.

HATEOAS & Paginação

Todos os recursos listados retornam _links com ações relacionadas:

"_links": [
  { "rel": "self", "href": "/api/motorcycles/{id}", "method": "GET" },
  { "rel": "delete", "href": "/api/motorcycles/{id}", "method": "DELETE" }
]


Paginação via ?page=&pageSize= retorna um wrapper com items, page, pageSize e totalCount.

Testes
Instalação de pacotes (já configurado no projeto Motix.Tests)

xunit, xunit.runner.visualstudio

FluentAssertions

Microsoft.AspNetCore.Mvc.Testing

Microsoft.EntityFrameworkCore.InMemory

Rodar os testes
dotnet test

O que é testado

SectorsController: criação e listagem paginada.

MotorcyclesController: criação com setor válido (201) e inválido (400).

MovementsController: criação com IDs válidos e tratamento de erro para FK inválida.

Os testes usam EF Core InMemory (não dependem do Oracle) e WebApplicationFactory para subir a API em memória.

Status Codes

200 OK – consultas e listagens.

201 Created – criação de recursos, com Location/CreatedAtAction.

204 No Content – atualizações e exclusões bem-sucedidas.

400 Bad Request – validação ou violação de FK.

404 Not Found – recurso inexistente.

Troubleshooting (Oracle)

ORA-00904 "ID" inválido: a coluna foi criada como "Id" (com aspas). Use "Id" nas consultas manuais.

IDs Guid em RAW(16): ao consultar manualmente por um Guid, use:

SELECT COUNT(*) FROM "SECTORS"
WHERE "Id" = hextoraw(replace('3FA85F64-5717-4562-B3FC-2C963F66AFA6','-'));


FK inválida (ORA-02291): a API converte em 400 Bad Request com mensagem amigável.
