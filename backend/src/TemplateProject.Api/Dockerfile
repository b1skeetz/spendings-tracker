FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/TemplateProject.Domain/TemplateProject.Domain.csproj", "src/TemplateProject.Domain/"]
COPY ["src/TemplateProject.Application/TemplateProject.Application.csproj", "src/TemplateProject.Application/"]
COPY ["src/TemplateProject.Infrastructure/TemplateProject.Infrastructure.csproj", "src/TemplateProject.Infrastructure/"]
COPY ["src/TemplateProject.Api/TemplateProject.Api.csproj", "src/TemplateProject.Api/"]
RUN dotnet restore "src/TemplateProject.Api/TemplateProject.Api.csproj"
COPY . .
WORKDIR "/src/src/TemplateProject.Api"
RUN dotnet build "./TemplateProject.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TemplateProject.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TemplateProject.Api.dll"]
