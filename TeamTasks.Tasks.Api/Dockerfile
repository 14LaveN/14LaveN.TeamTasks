FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TeamTasks.Tasks.Api/TeamTasks.Tasks.Api.csproj", "TeamTasks.Tasks.Api/"]
RUN dotnet restore "TeamTasks.Tasks.Api/TeamTasks.Tasks.Api.csproj"
COPY . .
WORKDIR "/src/TeamTasks.Tasks.Api"
RUN dotnet build "TeamTasks.Tasks.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TeamTasks.Tasks.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TeamTasks.Tasks.Api.dll"]
