# Deploying Azure Function

```sh
az functionapp create --functions-version 4 --name plantbaseddockeronpremium  --os-type linux --runtime dotnet-isolated --storage-account ${STORAGE_ACCOUNT_NAME} --image plantpowerjames/plantbasedpizza-function-app:latest --https-only true --plan ${APP_SERVICE_PREMIUM_PLAN_NAME} --resource-group ${RG_NAME}
```