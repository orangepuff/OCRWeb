FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["OCRWeb.slnx", "./"]
COPY ["src/Backend/OCRWeb.Bff/OCRWeb.Bff.csproj", "src/Backend/OCRWeb.Bff/"]
COPY ["src/Backend/OCRWeb.Document.Contract/OCRWeb.Document.Contract.csproj", "src/Backend/OCRWeb.Document.Contract/"]
COPY ["src/Backend/OCRWeb.Identity.Contract/OCRWeb.Identity.Contract.csproj", "src/Backend/OCRWeb.Identity.Contract/"]
COPY ["src/Backend/OCRWeb.ProjectManagement.Contract/OCRWeb.ProjectManagement.Contract.csproj", "src/Backend/OCRWeb.ProjectManagement.Contract/"]
COPY ["src/Backend/OCRWeb.OCR.Contract/OCRWeb.OCR.Contract.csproj", "src/Backend/OCRWeb.OCR.Contract/"]
RUN dotnet restore "src/Backend/OCRWeb.Bff/OCRWeb.Bff.csproj"

COPY . .
RUN dotnet publish "src/Backend/OCRWeb.Bff/OCRWeb.Bff.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OCRWeb.Bff.dll"]
