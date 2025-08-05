# TASK-01-02-01: Project Structure & Dependencies Setup

**Story**: STORY-01-02 .NET Backend Foundation with Clean Architecture
**Epic**: EPIC-01 Foundation Infrastructure & Development Environment
**Estimated Hours**: 4 hours
**Assignee**: Backend Developer
**Sprint**: 1

## Task Description

Create the .NET 9 solution structure following Clean Architecture principles with proper project organization, package references, and initial configuration setup.

## Acceptance Criteria

- [ ] Solution file created with proper project organization
- [ ] Four main projects created (Domain, Application, Infrastructure, API)
- [ ] Project references configured correctly
- [ ] Essential NuGet packages installed
- [ ] Global configuration files set up
- [ ] Project builds successfully without warnings

## Technical Implementation

### Solution Structure

```xml
<!-- AIPA.sln -->
<Solution>
  <ProjectGroup Name="1. Core">
    <Project Path="src\AIPA.Domain\AIPA.Domain.csproj" />
    <Project Path="src\AIPA.Application\AIPA.Application.csproj" />
  </ProjectGroup>
  <ProjectGroup Name="2. Infrastructure">
    <Project Path="src\AIPA.Infrastructure\AIPA.Infrastructure.csproj" />
  </ProjectGroup>
  <ProjectGroup Name="3. Presentation">
    <Project Path="src\AIPA.API\AIPA.API.csproj" />
  </ProjectGroup>
  <ProjectGroup Name="4. Tests">
    <Project Path="tests\AIPA.Domain.Tests\AIPA.Domain.Tests.csproj" />
    <Project Path="tests\AIPA.Application.Tests\AIPA.Application.Tests.csproj" />
    <Project Path="tests\AIPA.Infrastructure.Tests\AIPA.Infrastructure.Tests.csproj" />
    <Project Path="tests\AIPA.API.Tests\AIPA.API.Tests.csproj" />
  </ProjectGroup>
</Solution>
```

### Key Dependencies by Project

**AIPA.Domain**

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
```

**AIPA.Application**

```xml
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="AutoMapper" Version="13.0.1" />
<PackageReference Include="FluentValidation" Version="11.9.2" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.2" />
```

**AIPA.Infrastructure**

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.AzureKeyVault" Version="9.0.0" />
```

**AIPA.API**

```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.1" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.2" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
```

## Subtasks

1. **Create Solution and Projects (1.5 hours)**

   - Initialize .NET solution
   - Create four main projects with correct templates
   - Set up project references following dependency rules

2. **Configure Package Dependencies (1.5 hours)**

   - Install essential NuGet packages for each project
   - Configure package versions in Directory.Build.props
   - Set up global using statements

3. **Initial Configuration Setup (1 hour)**
   - Create appsettings.json templates
   - Set up launchSettings.json for development
   - Configure global.json for .NET version

## Definition of Done

- [ ] Solution builds without errors or warnings
- [ ] All project references are correctly configured
- [ ] Essential packages are installed and versions are managed
- [ ] Project structure follows Clean Architecture principles
- [ ] Configuration files are set up for all environments
- [ ] Code analysis and formatting rules are configured

## Dependencies

- .NET 9 SDK installed on development machines
- Development environment specifications finalized
- Project naming conventions established

## Resources

- [Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture)
- [.NET Project Structure Best Practices](https://docs.microsoft.com/en-us/dotnet/core/tutorials/top-level-templates)
