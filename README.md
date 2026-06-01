# StarCorp Travel API

Backend de reserva de passagens aéreas feito como teste técnico. A API cobre o fluxo inteiro: o cliente busca voos, cria uma reserva com um ou mais passageiros, paga e pode cancelar com reembolso conforme a política da classe tarifária.

Foi construída em .NET 10, com acesso a dados via Dapper sobre SQL Server, numa arquitetura em três camadas.

## Stack

* Linguagem: C# (.NET 10)
* API: ASP.NET Core com controllers
* Acesso a dados: Dapper com SQL escrito à mão (sem ORM)
* Banco: SQL Server 2022
* Validação: FluentValidation
* Testes: xUnit, com integração subindo um SQL Server real via Testcontainers
* Documentação: Swagger (OpenAPI)
* Container: Docker e docker compose

## Arquitetura

São três camadas, com a dependência apontando sempre numa direção só:

```
StarCorp.WebApi    apresentação e composition root
      |
      v
StarCorp.Business  regras de negócio, serviços, validação, notificações
      |
      v
StarCorp.Data      persistência com Dapper, entidades e paginação
```

* **WebApi**: controllers finos, configuração de injeção de dependência, Swagger e o mapeamento das notificações para status HTTP. Não tem regra de negócio, só recebe a requisição e chama o serviço.
* **Business**: onde a lógica vive. Os cálculos de preço e de cancelamento ficam isolados em serviços puros, sem nenhuma dependência de banco, o que deixa as regras fáceis de testar e de auditar.
* **Data**: repositórios com Dapper, a fábrica de conexão, as entidades (records imutáveis) e os modelos de leitura das consultas.

Optei por três camadas em vez de uma Clean Architecture completa porque o domínio é objetivo: uma única fonte de dados e um conjunto de regras de cálculo bem definidas. Três camadas entregam a separação que importa sem cerimônia desnecessária, e é o formato que costumo usar nos meus projetos.

### Padrões aplicados

* **Notification**: validação e erro de negócio não viram exceção. Um `NotificationContext` por requisição acumula as notificações e o controller base decide o status a partir do tipo do erro (400, 404, 409 ou 422). Isso mantém o controller limpo e dá um contrato de erro único.
* **Paginação**: `PageQuery` e `PagedResult<T>` carregam os metadados (página atual, tamanho, total de itens e total de páginas).
* **DTOs como records** imutáveis, separados das entidades.
* **Mapeamento manual**, sem AutoMapper, num método estático por serviço.
* **TimeProvider** injetado para o tempo, então a regra das 24h e a contagem de dias até a partida são testáveis sem depender do relógio do sistema.

## Estrutura de pastas

```
StarCorp/
  db/
    schema.sql        cria database, tabelas, índices e dados de referência
    seed.sql          dados de exemplo
  src/
    StarCorp.WebApi/      controllers, configurações, Program, health check
    StarCorp.Business/    serviços, DTOs, validações, pricing, notificações
    StarCorp.Data/        conexão, entidades, repositórios Dapper, paginação
  tests/
    StarCorp.UnitTests/         regras de preço, cancelamento e notificações
    StarCorp.IntegrationTests/  endpoints contra um SQL Server real
  Dockerfile
  docker-compose.yml
  StarCorp.slnx
```

## Como rodar

Pré requisitos: .NET 10 SDK e Docker.

### Opção 1: tudo no Docker (caminho mais simples)

```
docker compose up --build
```

Isso sobe o SQL Server e a API. A API espera o banco ficar saudável, cria o schema e carrega os dados de exemplo no startup. Depois é só abrir:

* Swagger: http://localhost:8080/swagger
* Health check: http://localhost:8080/health

### Opção 2: API local com o banco no Docker

Suba só o banco:

```
docker compose up db
```

E rode a API pelo SDK:

```
dotnet run --project src/StarCorp.WebApi
```

A connection string padrão já aponta para `localhost,1433`, o mesmo banco do compose. Em ambiente Development a API cria o schema e aplica o seed sozinha.

### Banco de dados

Os scripts ficam na pasta `db/`:

* `db/schema.sql` cria o database, as tabelas, os índices e os dados de referência (as classes tarifárias e seus multiplicadores).
* `db/seed.sql` traz dados de exemplo: companhias, clientes (um deles inativo, de propósito) e voos com datas sempre no futuro.

No startup a aplicação roda o `schema.sql` através de um bootstrapper, que faz o papel de um migrate para o cenário de Dapper. Os scripts são idempotentes, então podem rodar quantas vezes for preciso sem quebrar.

### Testes

```
dotnet test
```

* **Unitários**: cobrem o cálculo de preço e a política de cancelamento de ponta a ponta, sem tocar no banco. São a garantia das regras de negócio.
* **Integração**: sobem um SQL Server real em container via Testcontainers, aplicam o `schema.sql` e exercitam os cinco endpoints pela HTTP de verdade, incluindo os casos de erro. Precisam do Docker rodando.

## Endpoints

### Buscar voos

```
GET /api/flights
```

Filtros opcionais e combináveis: `originCity`, `destinationCity`, `date`, `minPrice`, `maxPrice`, `fareClass`, mais `page` e `pageSize`. Cada resultado representa um voo numa classe específica, já com o preço calculado e os assentos disponíveis.

```
GET /api/flights?originCity=Sao Paulo&fareClass=Executiva&page=1&pageSize=10
```

### Criar reserva

```
POST /api/bookings
```

```json
{
  "customerId": 1,
  "flightId": 1,
  "fareClass": "Economica",
  "passengers": [
    { "name": "Ana Souza", "document": "11111111111" }
  ]
}
```

Resposta (201) traz a reserva com o breakdown do cálculo:

```json
{
  "id": 1,
  "customerId": 1,
  "flightId": 1,
  "fareClass": "Economica",
  "status": "Pending",
  "passengerCount": 1,
  "passengers": [{ "name": "Ana Souza", "document": "11111111111" }],
  "breakdown": {
    "farePricePerPassenger": 1000.00,
    "passengers": 1,
    "subtotal": 1000.00,
    "taxes": 125.00,
    "serviceFee": 56.25,
    "amountDue": 1181.25
  },
  "payment": null,
  "createdAt": "2026-06-01T12:00:00Z"
}
```

### Consultar reserva

```
GET /api/bookings/{id}
```

### Processar pagamento

```
POST /api/bookings/{id}/payment
```

```json
{ "method": "Pix" }
```

Métodos aceitos: `CreditCard`, `Pix` e `Boleto`.

### Cancelar reserva

```
POST /api/bookings/{id}/cancel
```

Calcula o reembolso pela política da classe e devolve os assentos ao estoque.

## Regras de negócio

### Composição do preço

```
Subtotal     = preço base do voo * multiplicador da classe * número de passageiros
Impostos     = 8% do subtotal + R$ 45 fixos por passageiro
Taxa serviço = 5% sobre o subtotal já com impostos
Total devido = Subtotal + Impostos + Taxa de serviço
```

O ajuste do método de pagamento entra no momento do pagamento, não na criação (veja as decisões técnicas).

### Classes tarifárias

* Econômica: multiplicador 1,0x
* Executiva: multiplicador 2,5x

### Métodos de pagamento

* Cartão de crédito: mais 3% sobre o total
* Pix: menos 5% (desconto)
* Boleto: mais 1% sobre o total

### Política de cancelamento

Percentual de reembolso sobre o valor pago:

```
                 mais de 7 dias    de 2 a 7 dias    menos de 2 dias
Econômica            100%               50%               0%
Executiva            100%               75%              25%
```

Regra especial: cancelamento em até 24h após o pagamento devolve 100%, independentemente da tabela acima.

## Tratamento de erros

Todo erro responde no mesmo formato, seja de validação, de negócio ou de binding:

```json
{
  "status": 422,
  "errors": [
    { "key": "customer", "message": "Cliente inativo não pode realizar reservas." }
  ]
}
```

Status usados:

* **400** entrada inválida (corpo malformado, faixa de preço incoerente, lista de passageiros vazia)
* **404** cliente, voo ou reserva inexistente
* **409** conflito de estado (sem assentos, reserva já paga, reserva já cancelada)
* **422** regra de negócio violada (cliente inativo tentando reservar)

## Decisões técnicas

Algumas escolhas e interpretações que fiz, já que o enunciado deixa parte da modelagem em aberto:

* **Dapper sem ORM**: os repositórios usam SQL parametrizado. A busca de voos monta o filtro dinâmico uma vez só e reaproveita o mesmo bloco para a contagem e para a página, num único round trip com `QueryMultiple`.

* **Entidades como records, regras em serviços puros**: como o ORM é Dapper, deixei as entidades como records imutáveis mapeados por construtor. As regras de cálculo ficaram em serviços puros (`PricingCalculator` e `CancellationPolicy`), o que isola o negócio do banco e permite testar tudo sem infraestrutura.

* **Modelagem das classes tarifárias**: não usei herança nem single table. A diferença entre Econômica e Executiva é dado (o multiplicador de preço, a quantidade de assentos) e comportamento (a política de cancelamento), não estrutura. Então modelei a classe como dado de referência na tabela `FareClasses`, com a disponibilidade por classe em `FlightSeats`. Fica mais simples de consultar e de evoluir.

* **O ajuste de pagamento entra no pagamento, não na criação**: o corpo de criação da reserva não informa o método de pagamento, então a reserva nasce com o total devido (subtotal, impostos e taxa de serviço). O ajuste de Cartão, Pix ou Boleto é aplicado no passo de pagamento, e o reembolso no cancelamento usa o valor que de fato foi pago.

* **Notification estendido com 422**: o padrão de notificação que uso carrega o tipo do erro, cujo valor inteiro é o próprio status HTTP. Acrescentei o 422 para separar regra de negócio (cliente inativo) de simples conflito de estado.

* **Busca retorna oferta por classe**: cada linha do resultado é a combinação de voo e classe, com o preço já calculado pelo multiplicador. Assim os filtros de preço ficam sem ambiguidade, sempre sobre o valor que o cliente vê.

* **Baixa de assentos atômica**: a reserva acontece dentro de uma transação e o desconto de assentos é um `UPDATE` condicional (`SeatsAvailable >= passageiros`). Se nenhuma linha for afetada, não havia assento e a operação volta atrás. Isso evita corrida sem precisar de lock manual. O cancelamento devolve os assentos na mesma transação.

* **Arredondamento**: valores monetários usam `decimal` arredondado para duas casas, meio para cima (away from zero), componente a componente, então o breakdown sempre fecha com o total.

* **Dias até a partida**: medidos pela diferença exata entre a partida e o instante do cancelamento. Mais de 7 dias, de 2 a 7 dias (inclusive) e menos de 2 dias.

* **Reserva não paga**: pode ser cancelada, só que sem reembolso, já que nada foi pago. Os assentos voltam ao estoque normalmente.

## O que eu faria com mais tempo

* Autenticação e autorização com JWT.
* Chave de idempotência no pagamento, para reenvio seguro.
* Migrations versionadas (por exemplo com DbUp) no lugar do bootstrapper de schema.
* Integração real com um provedor de pagamento, no lugar do cálculo do ajuste.
* Logs estruturados com correlação de requisição e algumas métricas.
* Pipeline de CI no GitHub Actions rodando build e testes a cada push.
* Mais testes de borda e um teste de concorrência para a baixa de assentos.
