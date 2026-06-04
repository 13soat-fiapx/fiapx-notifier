# FiapX.Notifier

Serviço responsável por notificar usuários sobre o resultado do processamento de vídeos no ecossistema FIAP X.

Consome a fila SQS `fiapx-{env}-video-status-changed` e envia e-mails via SMTP para os usuários afetados.

## Definição do ambiente

- SDK: .NET 8.0
- Mensageria: Amazon SQS
- SMTP: Mailpit (local)

## Pré-requisitos

Certifique-se de ter provisionado o ambiente executando os scripts do
[repositório de infraestrutura](https://github.com/13soat-fiapx/fiapx-infra).

Para rodar localmente, é necessário ter o Docker em execução. O `docker-compose.yml` sobe o LocalStack e o Mailpit.

```powershell
docker compose up -d
```

## Como executar localmente

```powershell
dotnet run --project .\src\FiapX.Worker
```

## Script de deploy

Execute o script PowerShell para compilar a imagem e publicar no ECR.

```powershell
.\scripts\deploy-image.ps1 dev
```

## Mensageria

### Consumers

| Fila                                    | Descrição                                                  |
|-----------------------------------------|------------------------------------------------------------|
| `fiapx-{env}-video-status-changed`      | Notifica o usuário sobre o resultado do processamento.     |

## Links úteis

- [fiapx-infra](https://github.com/13soat-fiapx/fiapx-infra)
- [fiapx-processor](https://github.com/13soat-fiapx/fiapx-processor)
