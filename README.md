# 🚀 Motix API

API RESTful (.NET 8) para **gestão de Setores, Motos e Movimentações**, com foco em **boas práticas REST**:  
✔️ Status Codes  
✔️ Paginação  
✔️ HATEOAS  
✔️ Swagger/OpenAPI  
✔️ Arquitetura em camadas  

---

## 👥 Integrantes
- **Nicolas Candido Barutti Jacuck** – RM 554944  
- **Kleber da Silva** – RM 557887  
- **Lucas Rainha** – RM 558471  

*(Substitua pelos nomes/RM da sua equipe.)*

---

## 🎯 Justificativa do Domínio
Domínio simples e funcional, porém realista:

- **Sector** `(Id, Code)` → representa uma área/zona.  
- **Motorcycle** `(Id, Plate, SectorId)` → veículo associado a um setor.  
- **Movement** `(Id, MotorcycleId, SectorId, OccurredAt)` → histórico de movimentação de motos entre setores.  

👉 Cenário pequeno que cobre **3 entidades**, relacionamentos **1-N** e operações **CRUD completas**, além de um caso de uso comum: **registrar movimentações**.

---

## 🏗️ Arquitetura
```
src/
 ├── Motix/              -> API (Controllers, Program, Swagger)
 ├── Motix.Domain/       -> Entidades + Exception básica
 ├── Motix.Infrastructure/ -> EF Core + DbContext + Repositórios/DI
 ├── Motix.Application/  -> DTOs e UseCases
tests/
 └── Motix.Tests/        -> Testes (xUnit + InMemory + WebApplicationFactory)
```

**Principais decisões**:
- Camadas separadas (Domain, Infrastructure, Application e API).  
- **Entity Framework Core** com **Oracle (Oracle.EntityFrameworkCore)**.  
- **Swagger/OpenAPI** habilitado com exemplos de payloads.  
- **Paginação** via query `?page=&pageSize=` com `PagedResult`.  
- **HATEOAS** para links relacionados.  
- **DTOs** para separar API de entidades.  
- **Testes de integração** com `EF InMemory`.  

---

## 🔧 Requisitos & Tecnologias
- .NET 8 SDK  
- Oracle DB (string de conexão fornecida pela FIAP)  

**Pacotes principais**:  
- `Oracle.EntityFrameworkCore`  
- `Swashbuckle.AspNetCore`  
- Testes: `xunit`, `FluentAssertions`, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.InMemory`  

---

## ⚙️ Configuração
Arquivo `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Data Source=oracle.fiap.com.br:1521/orcl;User ID=RMXXXXX;Password=SENHA;"
  }
}
```

> ⚠️ Oracle: PKs `Guid` → mapeadas para **RAW(16)** (IDs em hex sem hífens).

---

## 📦 Migrations
```bash
# adicionar migration
dotnet ef migrations add InitialCreate --project src/Motix.Infrastructure/Motix.Infrastructure.csproj --startup-project src/Motix/Motix.csproj

# aplicar no banco
dotnet ef database update --project src/Motix.Infrastructure/Motix.Infrastructure.csproj --startup-project src/Motix/Motix.csproj
```

---

## ▶️ Como Executar
```bash
# restaurar pacotes e compilar
dotnet restore
dotnet build

# rodar a API
dotnet run --project src/Motix/Motix.csproj
```

API disponível em:  
👉 `https://localhost:7040/swagger`

---

## 📌 Endpoints & Exemplos

### **Sectors**
```http
POST /api/sectors
{ "code": "A1" }

GET /api/sectors?page=1&pageSize=10
```

Resposta:
```json
{
  "items": [
    { "data": { "id": "guid", "code": "A1" }, "_links": [...] }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1
}
```

---

### **Motorcycles**
```http
POST /api/motorcycles
{ "plate": "ABC1D23", "sectorId": "GUID_DO_SETOR" }
```
> Se `sectorId` não existir → **400 Bad Request**

---

### **Movements**
```http
POST /api/movements
{ "motorcycleId": "GUID_DA_MOTO", "sectorId": "GUID_DO_SETOR" }
```
> Retorna **201 Created** com `occurredAt (UTC)`.

---

## 🔗 HATEOAS & Paginação
Exemplo de `_links`:
```json
"_links": [
  { "rel": "self", "href": "/api/motorcycles/{id}", "method": "GET" },
  { "rel": "delete", "href": "/api/motorcycles/{id}", "method": "DELETE" }
]
```

---

## 🧪 Testes
Rodar:
```bash
dotnet test
```

**Cenários cobertos**:
- `SectorsController`: criação e listagem paginada.  
- `MotorcyclesController`: criação com setor válido (201) e inválido (400).  
- `MovementsController`: criação com IDs válidos + tratamento de FK inválida.  

---

## 📊 Status Codes
- **200 OK** → consultas e listagens  
- **201 Created** → criação com `Location`  
- **204 No Content** → update/delete ok  
- **400 Bad Request** → validação ou FK inválida  
- **404 Not Found** → recurso inexistente  

---

## 🛠️ Troubleshooting Oracle
- **ORA-00904 "ID" inválido** → coluna criada como `"Id"` (use aspas).  
- **IDs Guid (RAW 16)** → consultas devem usar `hextoraw(replace(...))`.  
- **ORA-02291 (FK inválida)** → tratado como **400 Bad Request**.  


