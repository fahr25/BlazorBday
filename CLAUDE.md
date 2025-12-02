## Stack
Blazor Server + MVC | SQLite + EF Core | Identity auth | Session-based state

## Architecture
- **Controllers/**: ShopController (MVC shop flow), AdminController, AccountController
- **Repositories/**: Product, Order, Category, Subcategory, Agency (DI injected)
- **Models/**: Product, Order, OrderItem, OrderDraft, Category, Subcategory, Agency, CartItem
- **Components/**: Blazor pages (BirthdayMarket, BdayConnect pages)
- **Session**: OrderDraft stored in session (not scoped service)

## Business Logic
**Points**: Age 0-11=65pts | 12-18=100pts (DOB calc at DemographicIntake)
**Flow**: Agency→DOB→Card→Book→Treat→Gifts(filtered)→Review→Checkout→ThankYou
**Categories**: DisplayOrder=sequence | IsActive=soft delete | Required: Cards(10), Books(20), Treats(30), Gifts(40)
**Checkout**: Transactional inventory decrement

## Style
Brief comments, descriptive names, explain bug fixes