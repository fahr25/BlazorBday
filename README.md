# BlazorBday

**Birthday Connections** - A point-based birthday assistance platform serving low-income families in Kansas City.

## Overview

BlazorBday helps agencies distribute birthday supplies to children through an intuitive shopping system. Children receive age-based points to select cards, books, treats, and gifts for their special day.

## Features

- **Age-Based Points System** - Automatic point allocation based on child's age
- **Sequential Shopping Flow** - 8-step guided process from agency selection to checkout
- **Admin Dashboard** - Full CRUD management for products, categories, agencies, and orders
- **Mobile Responsive** - Optimized for all device sizes with fixed cart bar on mobile
- **Session-Based Cart** - Secure, stateful shopping experience
- **Inventory Management** - Real-time tracking with transactional checkout
- **Role-Based Access** - ASP.NET Identity with admin authorization

## Technology Stack

- **Framework**: ASP.NET Core 8.0 (Blazor Server + MVC hybrid)
- **Database**: SQLite with Entity Framework Core 8.0
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, jQuery
- **State**: Session-based (HTTP session)
- **Testing**: xUnit (99 tests, 70% passing)

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Installation

```bash
# Clone the repository
git clone https://github.com/fahr25/BlazorBday.git
cd BlazorBday

# Restore dependencies
dotnet restore

# Run the application (auto-migrates database)
dotnet run
```

The application will be available at `https://localhost:5001` (or the port shown in console).


## Architecture

**Dual Architecture Approach:**

- **Blazor Components** - Public-facing pages (About, Contact, Home, News, Volunteer)
- **MVC Controllers** - Shop flow and Admin area for reliability

**Key Directories:**

```
BlazorBday/
├── Components/          # Blazor pages (BdayConnect section)
├── Controllers/         # MVC controllers (Shop, Admin, Account)
├── Models/             # Domain entities (Order, Product, Agency)
├── Repositories/       # Data access layer (repository pattern)
├── Views/              # MVC Razor views (Shop, Admin)
├── Data/               # DbContext and seed data
├── wwwroot/            # Static files (CSS, JS, images)
└── BlazorBday.Tests/   # Unit and integration tests
```

## Business Logic

### Points System

- **Ages 0-11**: 65 points
- **Ages 12-18**: 100 points
- Points calculated from child's date of birth

### Shopping Flow (8 Steps)

1. **Agency Selection** - Select agency and validate 3-letter code
2. **Demographic Intake** - Enter child's DOB (calculates age and points)
3. **Get Ready** - Introduction page
4. **Select Card** - Choose exactly 1 (required)
5. **Select Book** - Choose exactly 1 (required)
6. **Select Treat** - Choose exactly 1 (required)
7. **Select Gifts** - Multiple items with filtering
8. **Review & Checkout** - Confirm order and complete

### Categories

- **Cards** (DisplayOrder: 10) - Required
- **Books** (DisplayOrder: 20) - Required
- **Treats** (DisplayOrder: 30) - Required
- **Gifts** (DisplayOrder: 40) - Multiple allowed with filtering

### Checkout Process

- Transactional inventory decrement
- Session cart converted to persistent Order
- Inventory restored on refunds
- Order status tracking (Pending/Completed/Cancelled)

## Testing

**Test Coverage**: 99 tests total, 70 passing (70%)

**Passing Test Suites:**
- OrderDraft business logic (33/33) - Points calculation, cart operations
- ProductRepository CRUD (14/14) - All database operations
- ShopController (37/45) - Flow enforcement, filtering, validation
- AdminController (33/33) - Dashboard, CRUD, order management

**Known Issues:**
- 8 tests - Session mocking challenges (technical debt)
- 21 tests - Intentionally failing TDD tests for future features

**Run Tests:**

```bash
dotnet test
```

## Recent Updates

- **Mobile Cart Bar** - Fixed bottom cart with offcanvas drawer on mobile (commit 4ea0c96)
- **Test Suite** - Comprehensive xUnit tests with TDD approach (commit 5c3dcdb)
- **Mobile Improvements** - Responsive design, touch targets, iOS safe areas
- **MVC Migration** - Shop flow migrated from Blazor to MVC for reliability (commit 46d9b7d)

## Documentation

- [TEST_RESULTS.md](TEST_RESULTS.md) - Detailed test analysis and coverage
- [MOBILE_IMPROVEMENTS.md](MOBILE_IMPROVEMENTS.md) - Mobile UX documentation
- [CLAUDE.md](CLAUDE.md) - Architecture notes and development guidelines

## Development

**Code Style:**
- Brief comments, descriptive names
- Explain bug fixes in comments
- Repository pattern for data access
- Async/await for all database operations

**Future Enhancements** (tracked via failing TDD tests):
- Admin analytics dashboard (monthly stats, top products)
- Search and filtering in admin inventory
- Bulk inventory updates
- CSV export for orders
- Enhanced validation messages
- Inventory race condition handling
- PWA features

## License

This project is for educational and non-profit use supporting low-income families in Kansas City.
