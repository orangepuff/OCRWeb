FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["OCRWeb.slnx", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["NuGet.config", "./"]
# Backend
COPY ["src/Backend/OCRWeb.API/OCRWeb.API.csproj", "src/Backend/OCRWeb.API/"]
COPY ["src/Backend/OCRWeb.Document/OCRWeb.Document.csproj", "src/Backend/OCRWeb.Document/"]
COPY ["src/Backend/OCRWeb.Document.Api/OCRWeb.Document.Api.csproj", "src/Backend/OCRWeb.Document.Api/"]
COPY ["src/Backend/OCRWeb.Document.Contract/OCRWeb.Document.Contract.csproj", "src/Backend/OCRWeb.Document.Contract/"]
COPY ["src/Backend/OCRWeb.OCR/OCRWeb.OCR.csproj", "src/Backend/OCRWeb.OCR/"]
COPY ["src/Backend/OCRWeb.OCR.Contract/OCRWeb.OCR.Contract.csproj", "src/Backend/OCRWeb.OCR.Contract/"]
COPY ["src/Backend/OCRWeb.Pdf/OCRWeb.Pdf.csproj", "src/Backend/OCRWeb.Pdf/"]
COPY ["src/Backend/OCRWeb.Pdf.Contract/OCRWeb.Pdf.Contract.csproj", "src/Backend/OCRWeb.Pdf.Contract/"]
COPY ["src/Backend/OCRWeb.ProjectManagement/OCRWeb.ProjectManagement.csproj", "src/Backend/OCRWeb.ProjectManagement/"]
COPY ["src/Backend/OCRWeb.ProjectManagement.Api/OCRWeb.ProjectManagement.Api.csproj", "src/Backend/OCRWeb.ProjectManagement.Api/"]
COPY ["src/Backend/OCRWeb.ProjectManagement.Contract/OCRWeb.ProjectManagement.Contract.csproj", "src/Backend/OCRWeb.ProjectManagement.Contract/"]
# OrangepuffPortal (Directory.Build.props provides TargetFramework for all portal projects)
COPY ["src/OrangepuffPortal/Directory.Build.props", "src/OrangepuffPortal/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.Bff/OrangepuffPortal.Bff.csproj", "src/OrangepuffPortal/OrangepuffPortal.Bff/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.Config/OrangepuffPortal.Config.csproj", "src/OrangepuffPortal/OrangepuffPortal.Config/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.Config.Contract/OrangepuffPortal.Config.Contract.csproj", "src/OrangepuffPortal/OrangepuffPortal.Config.Contract/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.ConfigData/OrangepuffPortal.ConfigData.csproj", "src/OrangepuffPortal/OrangepuffPortal.ConfigData/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.ConfigData.Contract/OrangepuffPortal.ConfigData.Contract.csproj", "src/OrangepuffPortal/OrangepuffPortal.ConfigData.Contract/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.ConfigText/OrangepuffPortal.ConfigText.csproj", "src/OrangepuffPortal/OrangepuffPortal.ConfigText/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.ConfigText.Contract/OrangepuffPortal.ConfigText.Contract.csproj", "src/OrangepuffPortal/OrangepuffPortal.ConfigText.Contract/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.Host/OrangepuffPortal.Host.csproj", "src/OrangepuffPortal/OrangepuffPortal.Host/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.Identity/OrangepuffPortal.Identity.csproj", "src/OrangepuffPortal/OrangepuffPortal.Identity/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.Identity.Contract/OrangepuffPortal.Identity.Contract.csproj", "src/OrangepuffPortal/OrangepuffPortal.Identity.Contract/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.Shared/OrangepuffPortal.Shared.csproj", "src/OrangepuffPortal/OrangepuffPortal.Shared/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.Theme/OrangepuffPortal.Theme.csproj", "src/OrangepuffPortal/OrangepuffPortal.Theme/"]
COPY ["src/OrangepuffPortal/OrangepuffPortal.Theme.Contract/OrangepuffPortal.Theme.Contract.csproj", "src/OrangepuffPortal/OrangepuffPortal.Theme.Contract/"]
# DiagnosticLog
COPY ["src/DiagnosticLog/Diagnostics.Abstractions/Diagnostics.Abstractions.csproj", "src/DiagnosticLog/Diagnostics.Abstractions/"]
COPY ["src/DiagnosticLog/Diagnostics.AspNetCore/Diagnostics.AspNetCore.csproj", "src/DiagnosticLog/Diagnostics.AspNetCore/"]
COPY ["src/DiagnosticLog/Diagnostics.NLog/Diagnostics.NLog.csproj", "src/DiagnosticLog/Diagnostics.NLog/"]
RUN dotnet restore "src/Backend/OCRWeb.API/OCRWeb.API.csproj"

COPY . .
RUN dotnet publish "src/Backend/OCRWeb.API/OCRWeb.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OCRWeb.API.dll"]
