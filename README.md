# 🏢 PRISM - Enterprise Resource Planning System

> A comprehensive ERP solution designed for small and medium-sized businesses to streamline operations, manage finances, and drive growth.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Development](#development)
- [Deployment](#deployment)
- [Team](#team)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [API Documentation](#api-documentation)
- [License](#license)

---

## 🎯 Overview

PRISM is a modern, scalable ERP system built to help small and medium-sized businesses manage their entire operation from a single platform. From inventory management to financial reporting, PRISM provides the tools businesses need to operate efficiently and make data-driven decisions.

### Why PRISM?

- **🚀 Modern Architecture**: Built with .NET 9 and clean architecture principles
- **💼 Business-Focused**: Designed specifically for SMB needs
- **📊 Real-Time Insights**: Live dashboards and comprehensive reporting
- **🔐 Secure**: Enterprise-grade security with role-based access control
- **📱 Responsive**: Works seamlessly across desktop and mobile devices

---

## ✨ Features

### Core Modules

#### 📦 Inventory Management
- Multi-branch inventory tracking
- Real-time stock levels and alerts
- Item categorization and SKU management
- Supplier management and relationships

#### 🛒 Order Management
- Complete order lifecycle tracking
- Customer order history
- Dynamic pricing and discounts
- Order status workflows

#### 💰 Financial Management
- Expense tracking and categorization
- Payment processing and reconciliation
- Revenue and profit analytics
- Multi-business financial oversight

#### 📈 Business Intelligence
- Interactive dashboards
- Custom report generation
- Excel export functionality
- Top-selling items analytics
- Customer insights

#### 👥 Multi-Business Support
- Manage multiple businesses from one account
- Branch-level operations
- Consolidated reporting
- Business-specific configurations

#### 🔒 Security & Authentication
- JWT-based authentication
- Cookie-based session management
- Role-based access control (RBAC)
- Secure password management
- Audit logging

---

## 🛠 Tech Stack

### Backend
- **Framework**: .NET 9.0 (ASP.NET Core MVC)
- **ORM**: Entity Framework Core 9.0
- **Database**: Microsoft SQL Server (Latest)
- **Authentication**: ASP.NET Core Identity + JWT
- **Architecture**: Clean Architecture with Repository & Unit of Work patterns

### Frontend
- **UI Framework**: Bootstrap 5
- **JavaScript**: jQuery
- **Icons**: Font Awesome
- **Charts**: Chart.js, Recharts
- **Data Export**: ClosedXML (Excel)

### DevOps & Infrastructure
- **Hosting**: MonsterASP
- **CI/CD**: GitHub Actions
- **Version Control**: Git/GitHub
- **Container**: Docker support

---

## 🏗 Architecture

PRISM follows **Clean Architecture** principles with clear separation of concerns:

```
PRISM/
├── Controllers/          # HTTP request handlers
├── Services/            # Business logic layer
│   ├── IServices/      # Service interfaces
│   └── *Service.cs     # Service implementations
├── Repositories/        # Data access layer
│   ├── IRepositories/  # Repository interfaces
│   └── Repository.cs   # Generic repository
├── DataAccess/         # Database context & migrations
│   ├── IRepositories/  # Unit of Work interface
│   └── UnitOfWork.cs   # Unit of Work implementation
├── Models/             # Domain entities
├── Dto/                # Data transfer objects
├── Helpers/            # Utility classes
└── Views/              # Razor views
```

### Key Patterns Implemented

- ✅ **Repository Pattern**: Abstraction over data access
- ✅ **Unit of Work Pattern**: Transaction management
- ✅ **Service Layer Pattern**: Business logic separation
- ✅ **Dependency Injection**: Loose coupling
- ✅ **SOLID Principles**: Maintainable, extensible code

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server 2019+](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server Express
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/prism.git
   cd prism
   ```

2. **Configure Database Connection**
   
   Update `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=PRISM_DB;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;"
     }
   }
   ```

3. **Configure JWT Settings**
   ```json
   {
     "jwt": {
       "key": "YOUR_SECRET_KEY_HERE_MIN_32_CHARS",
       "Issuer": "yourdomain.com",
       "Audience": "yourdomain.com",
       "DurationInDays": 30
     }
   }
   ```

4. **Run Migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the Application**
   ```bash
   dotnet run
   ```

6. **Access the Application**
   - Navigate to: `https://localhost:8081`
   - Or: `http://localhost:8080`

---

## 💻 Development

### Branch Strategy

We follow **Git Flow** branching model:

- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/*` - New features (e.g., `feature/order-management`)
- `bugfix/*` - Bug fixes (e.g., `bugfix/login-issue`)
- `hotfix/*` - Critical production fixes
- `release/*` - Release preparation

### Commit Message Convention

We use **Conventional Commits**:

```
type(scope): subject

body (optional)

footer (optional)
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```bash
feat(orders): add UTC datetime field with edit capability
fix(auth): resolve cookie authentication issue
docs(readme): update installation instructions
refactor(orders): implement repository pattern and clean architecture
```

### Code Style Guidelines

- **C# Naming Conventions**: Follow Microsoft C# conventions
- **Indentation**: 4 spaces
- **Line Length**: Max 120 characters
- **Comments**: XML documentation for public APIs
- **Async/Await**: Use async methods for I/O operations
- **Null Safety**: Use nullable reference types

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test PRISM.Tests

# With coverage
dotnet test /p:CollectCoverage=true
```

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Rollback migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

---

## 🚢 Deployment

### Production Deployment (MonsterASP)

The application is automatically deployed via **GitHub Actions** on push to `main`.

#### Manual Deployment

1. **Publish the Application**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Configure Production Settings**
   
   Ensure `appsettings.Production.json` has correct values:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Production_Connection_String"
     },
     "jwt": {
       "key": "Production_JWT_Secret_Key"
     }
   }
   ```

3. **Deploy via FTP or Web Deploy**
   - Upload contents of `./publish` folder
   - Ensure `web.config` is configured
   - Restart the application pool

#### Environment Variables

Set these in your hosting environment:

| Variable | Description | Required |
|----------|-------------|----------|
| `ASPNETCORE_ENVIRONMENT` | Environment name (Production) | ✅ |
| `ConnectionStrings__DefaultConnection` | Database connection string | ✅ |
| `jwt__key` | JWT signing key | ✅ |
| `jwt__Issuer` | JWT issuer | ✅ |
| `jwt__Audience` | JWT audience | ✅ |

---

## 👥 Team

### Project Leadership
- **Abdelrahman Adel** - Project Manager, DevOps Engineer & Full-Stack Developer
  - 📧 [Email](Abdelrahmanadelelghndour@gmail.com)
  - 🔗 [LinkedIn](https://linkedin.com/in/abdelrhman-adel8)
### Development Team
- **Beshoy Gamal Waheb** - Full-Stack Developer
- **Salah Eldin Mohamed** - Full-Stack Developer  
- **Mohamed Bahaa Mohamed** - Full-Stack Developer

### Quality Assurance
- **Aya Yehya** - QA Engineer & Unit Testing Specialist

---

## 🗺 Roadmap

### ✅ Phase 1: Foundation (Completed - Q4 2024)
- [x] Core authentication and authorization
- [x] Multi-business and branch management
- [x] Basic CRUD operations for all entities
- [x] Database design and migrations
- [x] Initial UI/UX implementation

### ✅ Phase 2: Core Features (Completed - Q1 2025)
- [x] Order management system
- [x] Inventory tracking
- [x] Customer management
- [x] Supplier management
- [x] Expense tracking
- [x] Payment processing
- [x] Dashboard with real-time metrics
- [x] Report generation and Excel export

### ✅ Phase 3: Architecture & Optimization (Completed - Q1 2025)
- [x] Implement Repository Pattern
- [x] Implement Unit of Work Pattern
- [x] Service Layer extraction
- [x] Clean Architecture refactoring
- [x] Performance optimization
- [x] Transaction management

### 🔄 Phase 4: Advanced Features (In Progress - Q2 2025)
- [ ] Advanced analytics and BI dashboards
- [ ] Forecasting and predictive analytics
- [ ] Automated inventory replenishment
- [ ] Multi-currency support
- [ ] Tax calculation and compliance
- [ ] Email notifications system
- [ ] Document management (invoices, receipts)
- [ ] Barcode/QR code scanning

### 📋 Phase 5: API & Integration (Q2 2025)
- [ ] RESTful API with Swagger documentation
- [ ] API rate limiting and versioning
- [ ] Webhook support
- [ ] Third-party integrations (payment gateways)
- [ ] Mobile app API endpoints
- [ ] Export/Import functionality (CSV, JSON)

### 🔐 Phase 6: Enhanced Security & Compliance (Q3 2025)
- [ ] Two-factor authentication (2FA)
- [ ] Advanced audit logging
- [ ] Data encryption at rest
- [ ] GDPR compliance features
- [ ] Backup and disaster recovery
- [ ] Security penetration testing

### 🧪 Phase 7: Testing & Quality (Q3 2025)
- [ ] Comprehensive unit test coverage (80%+)
- [ ] Integration tests
- [ ] End-to-end testing
- [ ] Performance testing and benchmarking
- [ ] Load testing
- [ ] Automated testing pipeline

### 🚀 Phase 8: Scale & Enterprise Features (Q4 2025)
- [ ] Microservices architecture evaluation
- [ ] Containerization with Kubernetes
- [ ] Caching layer (Redis)
- [ ] Message queue implementation
- [ ] Multi-tenant architecture
- [ ] White-label capability
- [ ] API marketplace

### 🌍 Phase 9: Internationalization (2026)
- [ ] Multi-language support (i18n)
- [ ] Regional date/time formats
- [ ] Currency localization
- [ ] Right-to-left (RTL) language support
- [ ] Regional compliance features

---

## 🤝 Contributing

We welcome contributions! Please follow these guidelines:

### Pull Request Process

1. **Create a Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make Your Changes**
   - Write clean, documented code
   - Follow our code style guidelines
   - Add/update tests as needed

3. **Commit Your Changes**
   ```bash
   git commit -m "feat(module): description of changes"
   ```

4. **Push to Your Branch**
   ```bash
   git push origin feature/your-feature-name
   ```

5. **Open a Pull Request**
   - Use the PR template
   - Link related issues
   - Request review from team members
   - Ensure CI/CD passes

### Code Review Guidelines

- **Required Reviewers**: Minimum 1 team member approval
- **Review Checklist**:
  - [ ] Code follows style guidelines
  - [ ] No console.log or debug code
  - [ ] Tests pass locally
  - [ ] Documentation updated if needed
  - [ ] No merge conflicts
  - [ ] Performance considerations addressed

### Issue Reporting

Use GitHub Issues with appropriate labels:
- `bug` - Something isn't working
- `enhancement` - New feature or request
- `documentation` - Documentation improvements
- `good first issue` - Good for newcomers
- `help wanted` - Extra attention needed
- `priority-high` - Critical issues

---

## 📚 API Documentation

### Authentication

All API endpoints require authentication via JWT token.

**Login Endpoint:**
```http
POST /api/Authentication/Login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "your-password"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "refresh_token_here",
  "email": "user@example.com",
  "userName": "user@example.com",
  "roles": ["Admin"],
  "expiresOn": "2025-02-22T10:30:00Z"
}
```

### Using the API

Include the token in request headers:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Available Endpoints

#### Orders
- `GET /Order` - List all orders
- `GET /Order/Details/{id}` - Get order details
- `POST /Order/Create` - Create new order
- `POST /Order/Edit/{id}` - Update order
- `POST /Order/Delete/{id}` - Delete order (soft delete)

#### Business Management
- `GET /Business` - List businesses
- `POST /Business/Create` - Create business
- `PUT /Business/Edit/{id}` - Update business
- `DELETE /Business/Delete/{id}` - Archive business

#### Inventory
- `GET /Items` - List items
- `POST /Items/Create` - Create item
- `PUT /Items/Edit/{id}` - Update item
- `DELETE /Items/Delete/{id}` - Archive item

> **Note**: Full Swagger/OpenAPI documentation coming in Phase 5

---

## 🛡 Security

### Reporting Security Issues

If you discover a security vulnerability, please email security@example.com instead of using the issue tracker.

### Security Features

- ✅ JWT token authentication
- ✅ Password hashing with ASP.NET Core Identity
- ✅ HTTPS enforcement
- ✅ SQL injection prevention (parameterized queries)
- ✅ XSS protection
- ✅ CSRF tokens
- ✅ Rate limiting
- ✅ Audit logging

---

## 🙏 Acknowledgments

- Built with ❤️ by the PRISM Team
- Special thanks to all contributors
- Powered by .NET and open-source technologies

---

## 📞 Contact & Support

- **Project Repository**: [GitHub](https://github.com/ABDO-ADELL/My-Net-Worth)
- **Production Site**: [PRISM ERP](https://prism-financial-management.runasp.net/)

---

<div align="center">
  <strong>Made with 💼 for Small and Medium Businesses</strong>
  <br>
  <sub>© 2024-2025 PRISM Team. All rights reserved.</sub>
</div>
