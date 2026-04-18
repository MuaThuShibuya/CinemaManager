Euphoria Cinema Management System
Overview Euphoria Cinema is a comprehensive, full-stack web application built with ASP.NET Core MVC. It serves as an end-to-end movie theater management and ticket booking platform. This project was developed to showcase a deep understanding of modern web development architectures, relational database management, third-party API integrations, and secure user authentication.

It provides a seamless booking experience for customers while offering robust administrative tools for theater managers to handle movies, showtimes, user roles, and ticket validations.

🚀 Key Features

Dynamic Ticket Booking Flow: Customers can browse movies, view trailers, and use an asynchronous AJAX-powered UI to filter showtimes by city and date without reloading the page.
Interactive Seat Selection: Real-time seat status management with a background task (SeatResetService) to automatically release unpaid reserved seats after a timeout.
Integrated Payment Gateway: Seamless checkout process integrated with the MoMo E-Wallet API, processing real-time transactions and capturing payment callbacks via webhooks.
E-Tickets & QR Code Generation: Upon successful payment, the system generates base64 QR codes representing the tickets and automatically sends a confirmation email to the user via SMTP.
Advanced Ticket Validation System: A dedicated interface for cinema staff to scan/verify ticket validity using Order IDs or user emails, preventing ticket duplication.
Role-Based Access Control (RBAC): Secure authentication and authorization using ASP.NET Core Identity. Differentiates capabilities between Admin (managing theaters, users, and roles) and Customer (booking and reviewing movies).
Theater & Movie Management: Full CRUD operations for Admins to manage cinema branches, rooms, and movie catalogs, including "soft delete" functionalities (Enable/Disable theaters).

💻 Tech Stack & Tools

Backend: C#, ASP.NET Core MVC, Entity Framework Core
Database: MS SQL Server
Frontend: Razor Pages/Views, HTML5, CSS3, Bootstrap 5, jQuery, AJAX
Security & Auth: ASP.NET Core Identity (Cookie-based Auth)
Third-Party Integrations: MoMo Payment API, Gmail SMTP, Webhook notifications
Architecture: MVC Pattern, Dependency Injection, Repository Pattern (partial), Background Hosted Services

💡 Technical Skills Demonstrated

Full-Stack Development: Successfully bridging backend C# logic with dynamic frontend interactions using jQuery and AJAX.
Database Design: Managing complex entity relationships (Movies, Theaters, Rooms, Seats, Showtimes, Tickets) using EF Core Code-First approach.
API Integration & Webhooks: Handling external financial transactions securely and processing asynchronous webhook callbacks.
Clean Code & Best Practices: Utilizing Dependency Injection for services, managing configurations securely via appsettings.json, and writing maintainable, structured MVC code.
Localization & State Management: Implementing application-wide culture formatting and utilizing Distributed Memory Cache for session management.
