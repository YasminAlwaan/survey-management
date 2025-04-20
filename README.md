# Survey Management Subsystem

A cloud-native survey management system for healthcare platforms, enabling clinics, hospitals, and health tech providers to create, manage, and analyze patient surveys.

## Features

- **Survey Management**
  - Create, edit, and delete surveys with multiple question types
  - Schedule automated survey delivery
  - Associate surveys with departments or events
  - Support for various question types (text, rating, multiple choice, etc.)

- **Response Handling**
  - Secure submission and storage of survey responses
  - Real-time response validation
  - Support for partial responses and completion tracking

- **Analytics & Reporting**
  - Basic analytics (completion rates, average ratings)
  - Response aggregation by question type
  - Export capabilities (CSV/JSON)

- **Security & Access Control**
  - Role-based access control (RBAC)
  - Secure data handling
  - Audit logging

## Architecture

The solution follows Clean Architecture principles and is divided into four projects:

1. **SurveyManagement.Core**
   - Domain entities
   - Interface definitions
   - Business rules

2. **SurveyManagement.Application**
   - Application services
   - Business logic implementation
   - Interface implementations

3. **SurveyManagement.Infrastructure**
   - Data access implementation
   - External service integrations
   - Technical concerns

4. **SurveyManagement.API**
   - REST API endpoints
   - Request/response handling
   - Authentication/authorization

## Technology Stack

- **Backend**: ASP.NET Core 6.0
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Documentation**: Swagger/OpenAPI
- **Cloud Platform**: Azure

## Getting Started

### Prerequisites

- .NET 6.0 SDK
- SQL Server
- Azure subscription (for deployment)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/survey-management.git
   ```

2. Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=SurveyManagement;Trusted_Connection=True;"
     }
   }
   ```

3. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

4. Run the application:
   ```bash
   dotnet run --project SurveyManagement.API
   ```

### Configuration

The application can be configured through `appsettings.json` and environment variables:

- `JWT:Secret`: Secret key for JWT token generation
- `JWT:Issuer`: Token issuer
- `JWT:Audience`: Token audience
- `ConnectionStrings:DefaultConnection`: Database connection string

## API Documentation

The API is documented using Swagger/OpenAPI. Once the application is running, visit `/swagger` to view the interactive documentation.

### Key Endpoints

- `POST /api/surveys`: Create a new survey
- `GET /api/surveys/{id}`: Get survey by ID
- `GET /api/surveys/department/{department}`: Get surveys by department
- `POST /api/surveys/{id}/assign/{patientId}`: Assign survey to patient
- `POST /api/surveys/responses`: Submit survey response
- `GET /api/surveys/{id}/analytics`: Get survey analytics
- `GET /api/surveys/{id}/export`: Export survey responses

## Security

- JWT-based authentication
- Role-based access control
- Input validation and sanitization
- Secure password hashing
- HTTPS enforcement
- Audit logging

## Deployment

The application is designed to be deployed to Azure App Service:

1. Create required Azure resources:
   - App Service
   - SQL Database
   - Key Vault (for secrets)

2. Configure CI/CD pipeline:
   - Build the application
   - Run tests
   - Deploy to Azure
   - Apply database migrations

3. Set up monitoring:
   - Application Insights
   - Azure Monitor
   - Log Analytics

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request
 