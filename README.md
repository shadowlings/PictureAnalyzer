[![CI/CD Azure App Service](https://github.com/shadowlings/PictureAnalyzer/actions/workflows/main_picturesanalyzer.yml/badge.svg)](https://github.com/shadowlings/PictureAnalyzer/actions/workflows/main_picturesanalyzer.yml)

# PictureAnalyzer

### Demo:
No demo yet. The application works, but I'm not ready to deploy it yet.

### Setup:
Clone the repo. Auth0 settings are set in the appSettings.json files. This public repo does not include those values - set those values using your specific Auth0 values/id's. On the initial startup of the application, be sure the DB is correctly created. You will need MSSQL Server installed (I use MSSQL Developer Edition). Assuming the connection string is correct in appSettings.json, the application should create the DB on startup.

### Startup:
Set the 'Server' project as the start up project.

### DB Migration Commands:
Add-Migration -Project PictureAnalyzer.Data -StartupProject PictureAnalyzer.Server -Name InitialCreate

Update-Database -Project PictureAnalyzer.Data -StartupProject PictureAnalyzer.Server

### Ignore Local Changes to AppSettings.json:
Sensative configuration data, such as the DB connection strings, or Azure B2C config to the appsettings.json files. AppSettings existin the API and Web projects. Do not check in these values to the repo. Use the following commands to ignore changes to the appsettings.json files:
 - git update-index --assume-unchanged .\Server\appsettings.json            
 - git update-index --assume-unchanged .\Client\wwwroot\appsettings.json
 - use '--no-assume-unchanged' to reverse the ignore.

