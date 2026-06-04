param (
  [Parameter(HelpMessage = "Environment for deploy: dev, stg, prod")]
  [ValidateSet("dev", "stg", "prod")]
  [string]$Environment = "dev"
)

Write-Host "Fetching data for environment '$environment'..."
$serviceName = 'notifier'
$accountId = aws sts get-caller-identity --query Account --output text
$repositoryUrl = "$accountId.dkr.ecr.us-east-1.amazonaws.com/fiapx-$environment/$serviceName-cr"
$tag = (new-guid).Guid

$password = aws ecr get-login-password --region us-east-1
docker login --username AWS --password $password $repositoryUrl

Write-Host -ForegroundColor Yellow "Building and pushing image with tag '$tag'..."
docker build -t "$serviceName" -t "$($repositoryUrl):$tag" -t "$($repositoryUrl):latest" .
docker push "$($repositoryUrl):$tag"
docker push "$($repositoryUrl):latest"

Write-Host -ForegroundColor Green "Done."
