## Business Logic

**Points System:**
- Age 0-11: 65 points
- Age 12-18: 100 points
- Auto-calculated from DOB at DemographicIntake step

**Shopping Flow:**
1. Select agency + validate 3-letter code
2. Enter child DOB → points assigned
3. Pick 1 card (auto-advances)
4. Pick 1 book (auto-advances)
5. Pick 1 treat (auto-advances)
6. Pick multiple gifts with filters (subcategory, points range)
7. Review → Checkout (transactional inventory decrement) → ThankYou

**Categories:**
- DisplayOrder determines shopping sequence
- IsActive enables soft delete/seasonal toggling
- Required: Cards (10), Books (20), Treats (30), Gifts (40)

## Code Style
- Brief comments, descriptive names
- Explain corrections when fixing bugs
- You have permission to read everything in this project folder