# FiapX Notifier - Helm Chart

O chart do projeto é responsável por criar os recursos no Kubernetes para o processamento de filas SQS utilizando KEDA (`ScaledJob`).

Ao instalar o chart, o seguinte hook é executado:

1. `aws-secret`: Secret que reflete as credenciais da AWS do namespace `external-secrets` para o namespace da aplicação (necessário para o KEDA e ESO).

Somente após o hook ter rodado com sucesso é que os demais recursos são provisionados:

- `email-secret` (ExternalSecret)
- `config-map`
- `keda-auth` (TriggerAuthentication)
- `notifier` (ScaledJob)

O comando abaixo faz deploy em DEV usando a tag `latest`:

```powershell
$accountId = aws sts get-caller-identity --query Account --output text
$repositoryUrl = "$accountId.dkr.ecr.us-east-1.amazonaws.com/fiapx-dev/notifier-cr"
$queueUrl = "https://sqs.us-east-1.amazonaws.com/$accountId/fiapx-dev-video-processing-completed"

helm upgrade --install notifier ./k8s `
  --namespace notifications `
  --create-namespace `
  --set image.repository=$repositoryUrl `
  --set queue.url=$queueUrl
```

## Requisitos

O projeto utiliza [External Secrets Operator (ESO)](https://external-secrets.io/latest/introduction/overview/), [KEDA](https://keda.sh/) e [Reflector](https://github.com/emberstack/kubernetes-reflector) para sincronizar credenciais e escalar conforme a demanda. É importante garantir que os seguintes componentes estejam instalados no cluster:

1. **ESO e KEDA**: Instalados via Helm.
2. **Reflector**: Necessário para refletir a secret `aws-credentials` entre namespaces.
3. **Secret AWS**: Criar uma secret no namespace `external-secrets` chamada `aws-credentials` com os dados da AWS:
   - `access-key-id`
   - `secret-access-key`
   - `session-token`

```powershell
kubectl create secret generic aws-credentials `
  --namespace external-secrets `
  --from-literal=access-key-id="$(aws configure get aws_access_key_id)" `
  --from-literal=secret-access-key="$(aws configure get aws_secret_access_key)" `
  --from-literal=session-token="$(aws configure get aws_session_token)"
```

## Parâmetros

O chart possui os seguintes valores:

| Parâmetro                 | Descrição                                             | Padrão                   |
|---------------------------|-------------------------------------------------------|--------------------------|
| `app.project`             | Nome do projeto pai                                   | `fiapx`                  |
| `app.name`                | Nome do worker                                        | `notifier`               |
| `app.version`             | Versão da aplicação                                   | `1.0.0`                  |
| `app.env`                 | Ambiente (`dev`, `stg` ou `prod`)                     | `dev`                    |
| `image.repository`        | Repositório da imagem Docker                          |                          |
| `image.tag`               | Tag da imagem Docker                                  | `latest`                 |
| `email.smtpServer`        | Endereço do servidor SMTP                             | `mailpit-smtp`           |
| `email.smtpPort`          | Porta do servidor SMTP                                | `25`                     |
| `email.sslRequired`       | Indica se o SMTP requer SSL                           | `false`                  |
| `email.senderName`        | Nome do remetente de e-mail                           | `FIAP X`                 |
| `email.senderAddress`     | Endereço do remetente de e-mail                       | `postmaster@fiapx.io`    |
| `email.logoUrl`           | URL da logomarca no e-mail                            | (veja `values.yaml`)     |
| `email.downloadBaseUrl`   | URL base para download de vídeos no e-mail            | (veja `values.yaml`)     |
| `aws.useLocalstack`       | Indica se deve usar Localstack                        | `false`                  |
| `aws.region`              | Região da AWS                                         | `us-east-1`              |
| `queue.event`             | Chave do evento de mensageria                         | `VideoProcessingCompleted`|
| `queue.name`              | Nome da fila SQS (sufixo)                             | `video-processing-completed`|
| `queue.url`               | URL completa da fila SQS (**Obrigatório**)            |                          |
| `secrets.clusterSecretStore`| Nome do ClusterSecretStore do ESO                   | `fiapx-aws-secrets`      |

O ESO busca as secrets do SMTP no AWS Secrets Manager seguindo o padrão:
- Secret: `$appProject-$appEnv-email` (ex: `fiapx-dev-email`)
  - Propriedade: `userName`
  - Propriedade: `password`
