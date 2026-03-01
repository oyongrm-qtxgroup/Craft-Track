# CraftTrack — Product Requirements Document
**Small Business Order & Operations Manager**  
Version 1.0 | March 2026

---

## Document Overview

| Field | Details |
|---|---|
| Product Name | CraftTrack — Windows Order Tracker Application |
| Version | 1.0 (Initial Release) |
| Date | March 2026 |
| Target User | Small business owner (custom-made / handcrafted items) |
| Platform | Windows 10 / 11 Desktop Application |
| Internet Required | No — fully offline |
| Authentication | None — single-user local app |
| Database | Self-contained embedded database (SQLite) |
| Status | Pending Development |

---

## 1. Executive Summary

CraftTrack is a lightweight, fully offline Windows desktop application designed for a small business owner handling custom-made and handcrafted items. It centralizes order management, customer tracking, inventory monitoring, and financial oversight into a single easy-to-use tool — requiring no internet connection, no cloud service, and no user authentication.

**Core problems it solves:**
- Losing track of order deadlines and payment statuses across multiple customers
- No visibility into business income, expenses, and overall profitability
- Inability to efficiently manage raw material and tool inventory needed to fulfill orders

CraftTrack will run on any Windows 10 or 11 PC or laptop, install via a simple one-click installer, and store all data in an embedded SQLite database file that requires no external server or configuration.

---

## 2. Goals & Objectives

### 2.1 Business Goals
- Reduce missed deadlines and forgotten orders through proactive notifications
- Provide a clear financial picture of income, costs, and profits
- Track raw materials and tools to avoid production delays from stock shortages
- Maintain a reliable history of all customers and their orders

### 2.2 Product Goals
- Deliver a simple, intuitive UI that requires no training to use
- Run fully offline with zero external dependencies after installation
- Be installable on any Windows 10/11 laptop or PC within minutes
- Keep all data stored locally in a single portable SQLite database file
- Send Windows desktop notifications for approaching and overdue deadlines, even when the app is minimized

---

## 3. Scope

### 3.1 In Scope
- Order management module (create, edit, view, delete orders)
- Customer information management (name, contact details)
- Payment tracking (full payment, downpayment, outstanding balance)
- Order severity / priority levels (Rush, High, Low)
- Inventory module for raw materials/supplies and tools & equipment
- Budget & finance module (income tracking, expense tracking, profit & loss summary)
- Windows desktop notifications for order deadlines (even when app is minimized)
- Dashboard with at-a-glance summary of active orders and financials

### 3.2 Out of Scope (V1)
- Internet connectivity or cloud sync features
- Multi-user or authentication features
- Mobile or web-based version
- Integration with external payment gateways or e-commerce platforms
- Automated invoicing or receipt printing
- Export to PDF or Excel (future version)

---

## 4. System & Technical Requirements

### 4.1 Platform Requirements

| Requirement | Specification |
|---|---|
| Operating System | Windows 10 (build 1809+) or Windows 11 |
| Processor | 1 GHz or faster, x64 or x86 |
| RAM | 2 GB minimum (4 GB recommended) |
| Disk Space | 200 MB for installation + database growth |
| Internet | Not required — fully offline |
| Database | SQLite (embedded, self-contained) |
| Runtime | .NET 6+ (bundled in installer) |
| Installer | Single .exe or .msi installer, one-click setup |

### 4.2 Recommended Technology Stack
- **UI Framework:** WPF (Windows Presentation Foundation) or WinUI 3
- **Language:** C# with .NET 6 or later
- **Database:** SQLite via `Microsoft.Data.Sqlite` or Entity Framework Core
- **Notifications:** Windows Toast Notifications API (WinRT) for background alerts
- **Installer:** Inno Setup or MSIX packaging for simple one-click deployment

---

## 5. Functional Requirements

### 5.1 Dashboard

The dashboard is the home screen. It provides an at-a-glance view of business status without requiring navigation into individual modules.

**Dashboard Widgets:**
- Total Active Orders — count of all orders not yet marked complete
- Rush/High Priority Orders — count of orders flagged as Rush or High severity
- Overdue Orders — count of orders past their due date that are not completed
- Upcoming Deadlines — list of orders due within the next 7 days
- Total Revenue (Month) — total income collected in the current calendar month
- Unpaid/Partial Balance — total outstanding balance owed by customers
- Low Stock Alert — count of inventory items flagged as low stock

**Quick Actions:**
- Button: Add New Order
- Button: Add Expense
- Button: Add Inventory Item

---

### 5.2 Order Management Module

This is the core module. It manages all customer orders for custom-made/handcrafted items.

#### 5.2.1 Order List View
- Display all orders in a sortable, filterable table
- **Columns:** Order #, Customer Name, Date Ordered, Due Date, Items Summary, Total Price, Payment Status, Amount Paid, Balance Due, Severity, Order Status
- **Filters:** Severity level, Payment status, Order status (Active / Completed / Cancelled), Date range
- Sort by any column (ascending/descending)
- Color coding by severity: Rush = red, High = orange, Low = green/neutral

#### 5.2.2 Add / Edit Order Form

| Field | Type | Notes |
|---|---|---|
| Customer Name | Text (required) | Linked to customer record |
| Customer Phone | Text | Auto-populated from customer record |
| Date Ordered | Date picker (required) | Defaults to today's date |
| Due Date | Date picker (required) | Target delivery/completion date |
| Severity Level | Dropdown (required) | Rush / High / Low |
| Order Items | Repeatable line items | Item type, description, qty, unit price |
| Total Price | Auto-calculated | Sum of all line item totals |
| Payment Type | Dropdown (required) | Full Payment / Downpayment / Unpaid |
| Amount Paid | Number input | Manually entered; auto-fills total if Full Payment |
| Balance Due | Auto-calculated | Total Price minus Amount Paid |
| Order Status | Dropdown | Active / In Progress / Completed / Cancelled |
| Notes / Remarks | Text area | Special instructions, design details, etc. |

#### 5.2.3 Order Items (Line Items)
- Each order can contain **one or more items**
- Each line item captures: Item Type (dropdown or custom entry), Description / Details (free text), Quantity, Unit Price, Line Total (auto-calculated)
- User can add, edit, or remove line items within the order form
- Total Order Price auto-updates as items are modified

#### 5.2.4 Severity Levels

| Level | Color | Description |
|---|---|---|
| Rush | Red | Urgent — requires immediate attention, tight deadline |
| High | Orange | Important — priority order, monitor closely |
| Low | Green | Standard — normal timeline, no urgency |

#### 5.2.5 Payment Tracking
- **Payment Type options:** Full Payment / Downpayment / Unpaid
- Amount Paid is editable at any time to record additional payments
- **Balance Due** is always auto-calculated: `Balance Due = Total Price - Amount Paid`
- A visual badge shows payment status clearly in the order list (Paid = green, Partial = orange, Unpaid = red)

---

### 5.3 Customer Management Module

#### 5.3.1 Customer Fields
- Full Name (required)
- Phone Number / Contact
- Notes (optional — e.g., preferred contact time, social media handle)

#### 5.3.2 Customer Features
- When creating an order, user can search for existing customers or create a new one inline
- Customer profile shows all associated orders, total revenue, and outstanding balances
- Customers cannot be deleted if they have existing orders (soft delete / archive instead)

---

### 5.4 Inventory Module

Tracks raw materials/supplies and tools & equipment used in production.

#### 5.4.1 Inventory Categories
- **Raw Materials & Supplies** — fabrics, threads, paints, clay, resin, packaging, etc.
- **Tools & Equipment** — sewing machines, cutters, molds, heat guns, etc.

#### 5.4.2 Inventory Item Fields

| Field | Type | Notes |
|---|---|---|
| Item Name | Text (required) | Name of the material or tool |
| Category | Dropdown | Raw Material / Tool & Equipment |
| Unit of Measure | Text | pcs, meters, liters, kg, etc. |
| Current Stock / Qty | Number | Current quantity on hand |
| Low Stock Threshold | Number | Alert triggers at or below this value |
| Unit Cost | Currency | Cost per unit for expense tracking |
| Notes | Text | Supplier name, storage location, etc. |

#### 5.4.3 Inventory Features
- List view of all items with current stock levels
- Visual low-stock indicator when quantity is at or below the threshold
- Manual stock adjustment (add / deduct) with an optional reason note
- Stock adjustment history log per item (date, change amount, reason)

---

### 5.5 Budget & Finance Module

#### 5.5.1 Income Tracking
- Income is automatically recorded whenever a payment is logged on an order
- Income entries display: Date, Customer Name, Order #, Amount Received, Payment Type
- Income list is filterable by date range and customer

#### 5.5.2 Expense Tracking
- User can manually add expense entries at any time
- **Expense fields:** Date (required), Category (Materials / Tools / Packaging / Utilities / Other), Description, Amount
- Expenses can optionally be linked to a specific order
- Expense list is filterable by date range and category

#### 5.5.3 Profit & Loss Summary

| Summary Metric | Description |
|---|---|
| Total Revenue Collected | Sum of all payments received in the selected period |
| Total Expenses | Sum of all expense entries in the selected period |
| Net Profit | Total Revenue minus Total Expenses |
| Total Outstanding Balance | Total unpaid amounts owed by all customers |
| Expense by Category | Breakdown of expenses per category |

- Period selector: current month or custom date range

---

### 5.6 Notification System

Notifications appear as **Windows Toast Notifications** and must fire even when the app is minimized or in the system tray.

#### 5.6.1 Notification Triggers

| Trigger Condition | Timing | Severity Filter |
|---|---|---|
| Order due in 3 days | 9:00 AM daily check | All levels |
| Order due in 1 day | 9:00 AM daily check | All levels |
| Order overdue (past due date) | 9:00 AM daily check | All levels |
| Rush order created | Immediately on save | Rush only |
| Inventory item falls below low stock threshold | On stock adjustment | N/A |

#### 5.6.2 Notification Behavior
- Notifications appear as Windows Toast Notifications in the system notification area
- Clicking a notification opens the relevant order or inventory item in the app
- A background worker (Windows Task Scheduler or background service) checks for triggers daily at 9:00 AM
- App system tray icon shows a badge count for unacknowledged alerts
- User can dismiss or acknowledge notifications within the app
- Notifications respect Windows Focus Assist / Do Not Disturb settings

---

## 6. Non-Functional Requirements

| Category | Requirement |
|---|---|
| Performance | App must launch in under 3 seconds. All screens must load within 1 second. |
| Reliability | No data loss on unexpected app close. SQLite transactions must ensure data integrity. |
| Usability | All core workflows (add order, log payment, update inventory) completable within 3 steps. |
| Installability | Must install via a single .exe or .msi with no manual configuration post-install. |
| Portability | SQLite database stored in a known folder (e.g., `Documents/CraftTrack/`) for easy backup. |
| Offline Operation | Zero dependency on internet connectivity at any point. |
| Data Persistence | All data persists between sessions. No data lost when app is closed and reopened. |
| Maintainability | Database schema must support future version migrations without data loss. |

---

## 7. User Interface Guidelines

### 7.1 Navigation
Left-side navigation panel with icons and labels:
- Dashboard
- Orders
- Customers
- Inventory
- Budget & Finance
- Notifications / Alerts

### 7.2 UI Principles
- Clean, minimal design — avoid visual clutter
- Color-coded severity levels must be immediately visible in order lists
- Payment status badges (Paid / Partial / Unpaid) use distinct colors: green / orange / red
- All monetary values formatted with currency symbol (e.g., ₱1,500.00)
- Consistent date format throughout (e.g., MM/DD/YYYY or DD-MMM-YYYY)
- All tables support sorting and basic text search/filter

### 7.3 Screen List

| Screen / View | Purpose |
|---|---|
| Dashboard | Summary widgets, quick actions, upcoming deadlines |
| Order List | Filterable/sortable table of all orders |
| Order Detail / Add / Edit | Full order form with line items and payment info |
| Customer List | List of all customers |
| Customer Profile | Individual customer with full order history |
| Inventory List | All inventory items with stock levels |
| Inventory Item Detail | Add/edit item, view stock adjustment history |
| Finance — Income | Chronological list of all payments received |
| Finance — Expenses | Expense log with add/edit functionality |
| Finance — P&L Summary | Period summary of income, expenses, and net profit |
| Notifications Center | List of active alerts and acknowledgement controls |

---

## 8. Data Model Overview

All data is stored in a local SQLite database file.

### 8.1 Entities & Key Fields

| Entity | Key Fields |
|---|---|
| Customer | id, full_name, phone, notes, created_at |
| Order | id, customer_id, date_ordered, due_date, severity, status, total_price, amount_paid, balance_due, payment_type, notes, created_at |
| OrderItem | id, order_id, item_type, description, quantity, unit_price, line_total |
| InventoryItem | id, name, category, unit_of_measure, quantity, low_stock_threshold, unit_cost, notes |
| StockAdjustment | id, inventory_item_id, adjustment_type, quantity_change, note, adjusted_at |
| Expense | id, date, category, description, amount, order_id (nullable), created_at |
| Notification | id, type, message, related_order_id, is_acknowledged, created_at |

### 8.2 Key Relationships
- One **Customer** → many **Orders**
- One **Order** → many **OrderItems**
- One **InventoryItem** → many **StockAdjustments**
- One **Expense** → optionally linked to one **Order**
- Payments are tracked directly on the Order (amount_paid / balance_due), not as separate records

---

## 9. Acceptance Criteria

| # | Criteria | Expected Result |
|---|---|---|
| 1 | Install on a clean Windows 10/11 machine | App installs in under 2 minutes with no errors |
| 2 | Create an order with multiple items and a downpayment | Order saved; balance due auto-calculated correctly |
| 3 | Assign Rush severity to an order | Order displays with red indicator in order list |
| 4 | Set a due date for tomorrow on an active order | App sends a Windows toast notification the next morning at 9 AM |
| 5 | Minimize the app and wait for notification trigger | Toast notification appears without app in foreground |
| 6 | Adjust inventory stock below threshold | Low stock badge appears on item and dashboard alert shown |
| 7 | Log a partial payment, then pay the remaining balance | Balance due updates to zero; payment status updates to Paid |
| 8 | Add expenses and view P&L for current month | Net Profit = Total Income − Total Expenses, calculated correctly |
| 9 | Close and reopen the application | All data persists with no loss or corruption |
| 10 | Run app with no internet connection | App functions fully offline |

---

## 10. Future Enhancements (Post V1)

- Export orders and financial reports to PDF or Excel
- Printable order receipts or invoices
- Order status history/timeline (e.g., Accepted → In Progress → Completed)
- Photo attachments per order (reference images, completed product photos)
- Recurring order templates for repeat customers
- Sales performance charts (monthly revenue bar/line graphs)
- Backup and restore database to/from external drive or USB
- Custom expense categories and item type labels

---

## 11. Glossary

| Term | Definition |
|---|---|
| Order | A customer request for one or more custom-made/handcrafted items |
| Severity | Priority level assigned to an order: Rush, High, or Low |
| Downpayment | A partial payment made by a customer at the time of ordering |
| Balance Due | The remaining amount a customer owes after partial or no payment |
| Line Item | A single product/item entry within an order |
| SQLite | A lightweight embedded database that stores all data in a single local file |
| Toast Notification | A Windows pop-up alert appearing in the bottom-right corner of the screen |
| P&L | Profit and Loss — a financial summary comparing income vs. expenses |
| Low Stock Threshold | The minimum quantity level for an inventory item before an alert is triggered |

---

*CraftTrack PRD — Version 1.0 | For internal use only*
