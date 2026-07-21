\# 🔗 URL Shortner



A production-ready URL shortener built with ASP.NET Core MVC (.NET 9) using Clean Architecture.



!\[.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square\&logo=dotnet)

!\[SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=flat-square\&logo=microsoft-sql-server)

!\[Redis](https://img.shields.io/badge/Redis-7-DC382D?style=flat-square\&logo=redis)

!\[Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=flat-square\&logo=docker)

!\[License](https://img.shields.io/badge/License-MIT-green?style=flat-square)



\## ✨ Features



\- \*\*🔗 URL Shortening\*\* - Create short URLs with optional custom aliases

\- \*\*⚡ Fast Redirects\*\* - Redis caching for sub-millisecond redirects

\- \*\*🔐 Authentication\*\* - Email + Password with OTP 2FA

\- \*\*🔑 JWT Tokens\*\* - Secure HTTP-only cookies with refresh token rotation

\- \*\*📊 Dashboard\*\* - Track clicks, manage URLs, view analytics

\- \*\*🛡️ Rate Limiting\*\* - Redis-based rate limiting

\- \*\*🏗️ Clean Architecture\*\* - Domain, Application, Infrastructure, Web layers

\- \*\*🐳 Docker Ready\*\* - SQL Server \& Redis via Docker Compose



\## 🏗️ Architecture



UrlShortner/

├── src/

│ ├── UrlShortner.Domain/ # Entities \& Interfaces

│ ├── UrlShortner.Application/ # Business Logic \& Services

│ ├── UrlShortner.Infrastructure/ # Data Access, Redis, Email

│ └── UrlShortner.Web/ # MVC Application

└── docker-compose.yml



text



\## 🚀 Quick Start



\### Prerequisites

\- \[.NET 9 SDK](https://dotnet.microsoft.com/download)

\- \[Docker Desktop](https://www.docker.com/products/docker-desktop)



\### 1. Clone \& Setup

```bash

git clone https://github.com/yourusername/UrlShortner.git

cd UrlShortner

2\. Start Infrastructure

bash

docker-compose up -d

3\. Create Database Tables

Connect to SQL Server and run database/init.sql



4\. Run the Application

bash

dotnet run --project src/UrlShortner.Web

5\. Open Browser

text

https://localhost:7000

🔧 Configuration

appsettings.json

json

{

&#x20; "ConnectionStrings": {

&#x20;   "DefaultConnection": "Server=localhost,1433;Database=UrlShortnerDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;",

&#x20;   "Redis": "localhost:6379"

&#x20; },

&#x20; "JwtSettings": {

&#x20;   "SecretKey": "your-secret-key-min-32-chars",

&#x20;   "Issuer": "UrlShortner",

&#x20;   "Audience": "UrlShortner"

&#x20; }

}

Email Setup (Production)

For production, configure SMTP in appsettings.Production.json:



json

"EmailSettings": {

&#x20; "SmtpHost": "smtp.brevo.com",

&#x20; "SmtpPort": 587,

&#x20; "SmtpUsername": "your-email@domain.com",

&#x20; "SmtpPassword": "your-smtp-password"

}

📚 Tech Stack

Layer	Technology

Framework	ASP.NET Core MVC (.NET 9)

Database	MS SQL Server 2022

ORM	Dapper

Cache	Redis 7

Auth	JWT + Refresh Tokens

Email	MailKit

UI	Bootstrap 5 + Razor Views

🔑 Authentication Flow

User registers with email + password



OTP sent to email (dev mode: shown in console)



User verifies OTP



JWT access token (15 min) + Refresh token (7 days) issued



Tokens stored in HTTP-only secure cookies



Auto-refresh on token expiry



📊 Database Schema

Users - User accounts



OTPCodes - One-time passwords



RefreshTokens - JWT refresh tokens



ShortUrls - Shortened URLs



ClickLogs - Click analytics



🎯 API Endpoints

Method	Path	Description

GET	/	Homepage with shorten form

POST	/Url/Create	Create short URL

GET	/{shortCode}	Redirect to original URL

GET	/Auth/Register	Registration page

POST	/Auth/Register	Submit registration

GET	/Auth/Login	Login page

POST	/Auth/Login	Submit login

POST	/Auth/VerifyOtp	Verify OTP

POST	/Auth/Logout	Logout

GET	/Dashboard	User dashboard

GET	/Dashboard/Profile	User profile

GET	/Health	Health check

📝 License

This project is licensed under the MIT License.



🙏 Acknowledgments

Built with ASP.NET Core MVC



Styled with Bootstrap 5



Powered by Dapper \& Redis



text



\---



\### Step 6: Create .env.example



```bash

@'

\# Database

DB\_CONNECTION=Server=localhost,1433;Database=UrlShortnerDb;User Id=sa;Password=UrlShortner@2024!;TrustServerCertificate=True;



\# Redis

REDIS\_CONNECTION=localhost:6379



\# JWT

JWT\_SECRET\_KEY=change-this-to-a-very-long-random-string-32-chars-minimum!



\# Email (Brevo)

EMAIL\_FROM=noreply@urlshortner.com

EMAIL\_SMTP\_HOST=smtp.brevo.com

EMAIL\_SMTP\_PORT=587

EMAIL\_SMTP\_USERNAME=your-email@domain.com

EMAIL\_SMTP\_PASSWORD=your-smtp-password

'@ | Out-File -FilePath .env.example -Encoding UTF8

