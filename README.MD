
## https://www.wordflux.online 
Main application; Can be installed as pwa


## Application observability
Currently, app use 2 providers for exporting logs, traces, metrics as well as alert notifications
#### _Aspire dashboard_ (for logs, traces, metrics)
Deployed with docker image to the custom VM in azure. Can be accessed by [this url](http://172.191.101.172:5568) (Frontend token is required).
#### _Seq_ (for logs, traces and alert notifications)
Can be accessed by [this url](http://172.191.101.172/) (Credentials are required).


## Setup local environment


#### Run _Aspire dashboard_ locally
This command will spin the docker container with default keys (that are already used in appsettings.development.json) 
```
docker run -d --restart unless-stopped -it -p 5568:18888 -p 5569:18889 --name aspire-dashboard -e DASHBOARD__OTLP__AUTHMODE=ApiKey -e DASHBOARD__OTLP__PRIMARYAPIKEY=dev -e Dashboard__Frontend__BrowserToken=admin mcr.microsoft.com/dotnet/aspire-dashboard:latest
```


#### Run _Seq_ locally (optional)
```
docker run -d --name seq -e ACCEPT_EULA=Y -p 82:80 -p 5343:5341 --restart unless-stopped -v seq-logs:/seqdata datalust/seq:latest
```

#### Run _postgres_ locally
```
docker run --name postgresDB -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=postgresdb -p 5455:5432 -v postgres-volume:/var/lib/postgresql/data --restart unless-stopped -d postgres
```

### Required external dependencies
#### There are a number of integrations happening in the backend so to make them all word the following keys are required:
- "OpenAIKey" - Open AI integration
- "AzureAiTranslatorKey" - Integration with Azure AI translator
- "DeeplAuthKey" - integration with Deepl
- "UnsplashApiKey" - integration with Unsplash (used for searching images for terms)
- "BingSearchApiKey" - integration with Azure bing search (currently is only used for searching images)


### Backend run with dotnet aspire
* Run WordFlux.AppHost.Https scheme. It will spin up required services (Database, Aspire dashboard)

### Backend run separately
#### You need to provide required dependencies to the API
1) Connection string to the postgres database. By default the following connection is used for development "**Host=localhost;Port=5455;Username=postgres;Password=postgres;Database=postgresdb**"

### Frontend
#### Due to some restrictions pertaining how Blazor Web Assembly works, it is only currently possible to run Blazor standalone app separately, without AppHost file.

* Go to WordFlux.ClientApp -> wwwroot -> appsettings.Development.json
* Change BackendUrl to the url that will correspond backend server.

1) (In case of running API project separately this url is https://localhost:7443/) 
2) (In case of running API project with dotnet aspire this url will be mapped dynamically for the API and will be only know after the start of the HOST project)

