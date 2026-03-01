<p align="center">
  <img src="src/Assets/ink logo.png" alt="CraftTrack Logo" width="150" />
</p>

<h1 align="center">CraftTrack</h1>

<p align="center">
  <strong>Windows Order Tracker for Small Craft Businesses</strong><br/>
  Built by <em>The Oyong's Ink &amp; Print Studio</em>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet" alt=".NET 10" />
  <img src="https://img.shields.io/badge/WPF-Windows-0078D4?logo=windows" alt="WPF" />
  <img src="https://img.shields.io/badge/SQLite-Embedded-003B57?logo=sqlite" alt="SQLite" />
  <img src="https://img.shields.io/badge/License-Proprietary-red" alt="License" />
</p>

---

## Overview

**CraftTrack** is a fully offline, single-user Windows desktop application designed for small business owners who sell custom-made or handcrafted items. It provides a centralized hub to manage orders, customers, inventory, products, and finances — all without requiring an internet connection.

### Key Features

| Module                  | Description                                                                                            |
| ----------------------- | ------------------------------------------------------------------------------------------------------ |
| **Dashboard**           | At-a-glance view of active orders, revenue, pending payments, low-stock alerts, and upcoming deadlines |
| **Order Management**    | Create, track, and manage customer orders with multi-item support, discounts, and payment tracking     |
| **Customer Management** | Maintain a customer database with contact details and full order history                               |
| **Product Catalog**     | Define reusable products with linked material requirements for automatic cost estimation               |
| **Inventory**           | Track raw materials and tools with stock levels, adjustments, and low-stock threshold alerts           |
| **Budget & Finance**    | Record expenses, view income vs. expense breakdowns, and category-based spending analysis              |
| **Notifications**       | In-app + Windows Toast Notifications for approaching deadlines, overdue orders, and low stock          |
| **Settings**            | Light / Dark / System theme toggle with Material Design UI                                             |

---

## Tech Stack

| Component         | Technology                                                                                                                                               |
| ----------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Framework**     | .NET 10 (Windows Desktop)                                                                                                                                |
| **UI**            | WPF + [Material Design In XAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)                                                     |
| **Architecture**  | MVVM ([CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/))                                                                |
| **Database**      | SQLite via [EF Core 10](https://learn.microsoft.com/ef/core/)                                                                                            |
| **DI**            | Microsoft.Extensions.DependencyInjection + Hosting                                                                                                       |
| **Notifications** | [Microsoft.Toolkit.Uwp.Notifications](https://learn.microsoft.com/windows/apps/design/shell/tiles-and-notifications/send-local-toast) + Background Agent |
| **System Tray**   | [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon)                                                                                  |
| **Installer**     | [Inno Setup 6](https://jrsoftware.org/isinfo.php)                                                                                                        |

---

## Prerequisites

- **Windows 10** (version 1809 / build 17763) or later
- [**.NET 10 SDK**](https://dotnet.microsoft.com/download/dotnet/10.0) (Preview)
- **Visual Studio 2022 17.12+** (recommended) with the **.NET Desktop Development** workload  
  _— or —_ any editor + the .NET CLI
- [**Inno Setup 6**](https://jrsoftware.org/isdl.php) (only if building the installer)

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/<your-username>/CraftTrack.git
cd CraftTrack
```

### 2. Restore dependencies

```bash
dotnet restore CraftTrack.slnx
```

### 3. Build the solution

```bash
dotnet build CraftTrack.slnx
```

### 4. Run the application

```bash
dotnet run --project src/CraftTrack/CraftTrack.csproj
```

The app will automatically:

- Create the database directory at `Documents/CraftTrack/`
- Run EF Core migrations to initialize `crafttrack.db`
- Launch the main window

---

## Project Structure

```
CraftTrack/
├── CraftTrack.slnx                  # Solution file
├── CraftTrack_PRD.md                # Product Requirements Document
├── installer.iss                    # Inno Setup installer script
│
└── src/
    ├── CraftTrack/                  # Main WPF application
    │   ├── App.xaml(.cs)            # Application entry point & DI setup
    │   ├── MainWindow.xaml(.cs)     # Shell window with navigation
    │   ├── Assets/                  # Icons and images
    │   ├── Converters/              # XAML value converters
    │   ├── Data/                    # EF Core DbContext & Migrations
    │   ├── Helpers/                 # Utility classes (currency, images)
    │   ├── Models/                  # Domain models & enums
    │   │   └── Enums/               # OrderStatus, PaymentType, etc.
    │   ├── Services/                # Business logic (interfaces + implementations)
    │   ├── Themes/                  # Light/Dark XAML resource dictionaries
    │   ├── ViewModels/              # MVVM ViewModels
    │   └── Views/                   # XAML Views (pages)
    │
    └── CraftTrack.NotificationAgent/  # Background notification console app
        └── Program.cs               # Deadline & low-stock checker
```

---

## Database

CraftTrack uses an embedded **SQLite** database stored at:

```
%USERPROFILE%\Documents\CraftTrack\crafttrack.db
```

The database is created and migrated automatically on first launch. No manual setup is required.

### EF Core Migrations

To add a new migration after modifying models:

```bash
cd src/CraftTrack
dotnet ef migrations add <MigrationName>
```

To apply migrations manually:

```bash
dotnet ef database update
```

### Schema Overview

| Table              | Description                                           |
| ------------------ | ----------------------------------------------------- |
| `Customers`        | Customer contact information                          |
| `Orders`           | Order header (status, deadline, payment, discount)    |
| `OrderItems`       | Line items linked to orders and optional products     |
| `Products`         | Reusable product catalog with pricing                 |
| `ProductMaterials` | Bill of materials linking products to inventory items |
| `InventoryItems`   | Raw materials and tools with stock quantities         |
| `StockAdjustments` | Audit trail for inventory changes                     |
| `Expenses`         | Financial expense records by category                 |
| `Notifications`    | In-app notification log                               |

---

## Notification Agent

The **CraftTrack.NotificationAgent** is a standalone console application that checks for:

- Overdue orders
- Orders due within 1 day
- Orders due within 3 days
- Low-stock inventory items

It sends **Windows Toast Notifications** and logs them to the database.

### Running manually

```bash
dotnet run --project src/CraftTrack.NotificationAgent/CraftTrack.NotificationAgent.csproj
```

### Scheduling with Windows Task Scheduler

For automated daily notifications, create a scheduled task:

1. Open **Task Scheduler** → **Create Basic Task**
2. Set trigger to **Daily** at your preferred time (e.g., 9:00 AM)
3. Set action to **Start a program**
4. Browse to the compiled `CraftTrack.NotificationAgent.exe`
5. Finish the wizard

---

## Building the Installer

An [Inno Setup](https://jrsoftware.org/isinfo.php) script is included to create a Windows installer.

### 1. Publish the application

```bash
dotnet publish src/CraftTrack/CraftTrack.csproj -c Release -r win-x64 --self-contained -o publish
```

### 2. Compile the installer

```bash
iscc installer.iss
```

The installer will be generated at `installer_output/CraftTrack_Setup_1.0.0.exe`.

---

## Configuration

CraftTrack stores user preferences (theme setting) at:

```
%USERPROFILE%\Documents\CraftTrack\settings.json
```

Example:

```json
{
  "Theme": "Dark"
}
```

Available themes: `Light`, `Dark`, `System`

---

## Contributing

This is currently a private project. If you'd like to contribute, please open an issue to discuss proposed changes before submitting a pull request.

---

## License

This project is proprietary software developed by **The Oyong's Ink & Print Studio**. All rights reserved.
