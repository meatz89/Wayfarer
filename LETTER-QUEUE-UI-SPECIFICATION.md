# Letter Queue UI Specification

**COMPREHENSIVE SPECIFICATION**: This document defines the complete UI architecture for the letter queue system, including all screens, components, and interactions.

## **OVERVIEW OF UI SCREENS**

### **1. Letter Queue Screen (PRIMARY GAMEPLAY)**
- **Purpose**: Central hub for queue management and daily decision-making
- **Primary User**: Players managing their 8-slot letter queue
- **Key Features**: Queue display, manipulation actions, token management, obligations panel

### **2. Character Relationship Screen (NPC MANAGEMENT)**
- **Purpose**: View and manage relationships with all known NPCs
- **Primary User**: Players planning relationship investments and token strategies
- **Key Features**: NPC overview, per-NPC token display, location information, interaction history

### **3. Standing Obligations Screen (CHARACTER DEVELOPMENT)**
- **Purpose**: Manage permanent character modifications and conflicts
- **Primary User**: Players understanding how obligations shape their gameplay
- **Key Features**: Active obligations, queue effects, conflict detection, acquisition history

---

## **DETAILED SCREEN SPECIFICATIONS**

### **LETTER QUEUE SCREEN**

#### **Layout Structure**
```
┌─────────────────────────────────────────────────────────────────┐
│                    LETTER QUEUE SCREEN                         │
├─────────────────────────────────────────────────────────────────┤
│  Token Balance: 🟢3 🔵2 🟣1 🟤4 ⚫0                            │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────┐  ┌─────────────────────────┐ │
│  │        LETTER QUEUE (8 SLOTS)   │  │   STANDING OBLIGATIONS  │ │
│  │                                 │  │                         │ │
│  │  1. [Elena: Love Letter]        │  │  🟣 Noble's Courtesy    │ │
│  │     💚 2 days │ 3 coins         │  │     Nobles → Slot 5     │ │
│  │                                 │  │                         │ │
│  │  2. [Merchant: Trade Goods]     │  │  ⚫ Shadow's Burden     │ │
│  │     🔵 1 day! │ 8 coins         │  │     Forced shadow/3days │ │
│  │                                 │  │                         │ │
│  │  3. [Patron: Intel Request]     │  │  🔴 CONFLICT WARNING    │ │
│  │     ❓ 5 days │ 20 coins        │  │     Obligations clash   │ │
│  │                                 │  │                         │ │
│  │  4. [Shadow: Package Delivery]  │  │                         │ │
│  │     ⚫ 2 days │ 15 coins         │  │                         │ │
│  │                                 │  │                         │ │
│  │  5. [Empty]                     │  │                         │ │
│  │  6. [Empty]                     │  │                         │ │
│  │  7. [Empty]                     │  │                         │ │
│  │  8. [Empty]                     │  │                         │ │
│  └─────────────────────────────────┘  └─────────────────────────┘ │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                 QUEUE ACTIONS                               │ │
│  │                                                             │ │
│  │  [Purge Bottom] [Priority Move] [Extend Deadline] [Skip]   │ │
│  │     (3 any)        (5 match)       (2 match)      (1 match)│ │
│  │                                                             │ │
│  │  [Morning Swap] [Accept New Letter] [Deliver Letter]       │ │
│  │     (FREE)          (varies)           (position 1)        │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

#### **Letter Card Component**
```
┌─────────────────────────────────────┐
│  [1] Elena: Love Letter             │
│  ┌─────────────────────────────────┐ │
│  │ 💚 TRUST │ 2 days │ 3 coins    │ │
│  │ To: Marcus at Crossbridge       │ │
│  │ Size: Small │ Personal          │ │
│  └─────────────────────────────────┘ │
│                                     │
│  Skip Cost: 1 Trust token           │
│  Expires: Tomorrow morning!         │
└─────────────────────────────────────┘
```

**Letter Card Information Architecture**:
- **Position Number**: Prominently displayed (1-8)
- **Sender Name**: Who sent the letter
- **Letter Type**: Brief description of contents
- **Token Type Icon**: Visual indicator of connection type (💚🔵🟣🟤⚫)
- **Deadline**: Days remaining with visual urgency
- **Payment**: Coins earned for delivery
- **Recipient**: Who receives the letter and where
- **Size**: Inventory impact (Small/Medium/Large)
- **Skip Cost**: Exact token cost to deliver out of order
- **Urgency Indicators**: Visual warnings for expiring letters

#### **Queue Action Buttons**

**Purge Bottom Letter**:
```
┌─────────────────────────────────────┐
│  [Purge Bottom Letter]              │
│  Remove letter from slot 8          │
│  Cost: 3 tokens (any type)          │
│  Current: 🟢3 🔵2 🟣1 🟤4 ⚫0      │
│  ✅ Can afford                      │
└─────────────────────────────────────┘
```

**Priority Move to Slot 1**:
```
┌─────────────────────────────────────┐
│  [Priority Move]                    │
│  Move selected letter to slot 1     │
│  Cost: 5 matching tokens            │
│  Selected: Elena (Trust)            │
│  Need: 5 🟢 │ Have: 3 🟢           │
│  ❌ Insufficient tokens             │
└─────────────────────────────────────┘
```

**Extend Deadline**:
```
┌─────────────────────────────────────┐
│  [Extend Deadline]                  │
│  Add 2 days to selected letter      │
│  Cost: 2 matching tokens            │
│  Selected: Merchant (Trade)         │
│  Need: 2 🔵 │ Have: 2 🔵           │
│  ✅ Can afford                      │
└─────────────────────────────────────┘
```

**Skip Delivery**:
```
┌─────────────────────────────────────┐
│  [Skip to Deliver]                  │
│  Deliver selected letter out of order│
│  Cost: 1 token per position skipped │
│  Selected: Elena (pos 4)            │
│  Skip cost: 3 🟢 tokens             │
│  ✅ Can afford                      │
└─────────────────────────────────────┘
```

#### **Standing Obligations Panel**

```
┌─────────────────────────────────────┐
│        STANDING OBLIGATIONS         │
│                                     │
│  🟣 Noble's Courtesy                │
│     • Noble letters enter at slot 5 │
│     • Cannot refuse noble letters   │
│     • Acquired: Day 12              │
│                                     │
│  ⚫ Shadow's Burden                 │
│     • Shadow letters pay triple     │
│     • Forced shadow letter/3 days   │
│     • Acquired: Day 18              │
│                                     │
│  🔴 CONFLICT WARNING                │
│     Noble + Shadow obligations      │
│     create queue space pressure     │
│                                     │
│  [View All Obligations]             │
└─────────────────────────────────────┘
```

---

### **CHARACTER RELATIONSHIP SCREEN**

#### **Layout Structure**
```
┌─────────────────────────────────────────────────────────────────┐
│                 CHARACTER RELATIONSHIPS                         │
├─────────────────────────────────────────────────────────────────┤
│  Filter: [All] [Trust] [Trade] [Noble] [Common] [Shadow]       │
│  Sort: [Name] [Location] [Relationship] [Tokens]               │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │  MILLBROOK                                                  │ │
│  │                                                             │ │
│  │  ┌─────────────────────────────────────────────────────────┐ │ │
│  │  │ [💚] Elena Messenger        │ Relationship: Warm        │ │ │
│  │  │ Location: Millbrook Tavern  │ Tokens: 🟢4 🔵1          │ │ │
│  │  │ Last Letter: 2 days ago     │ Status: Available         │ │ │
│  │  │                             │ History: 8 delivered      │ │ │
│  │  │ [Visit Elena] [View History] [Letter Offers]           │ │ │
│  │  └─────────────────────────────────────────────────────────┘ │ │
│  │                                                             │ │
│  │  ┌─────────────────────────────────────────────────────────┐ │ │
│  │  │ [🔵] Marcus Trader          │ Relationship: Neutral     │ │ │
│  │  │ Location: Market Square     │ Tokens: 🔵2 🟤1          │ │ │
│  │  │ Last Letter: 5 days ago     │ Status: Available         │ │ │
│  │  │                             │ History: 3 delivered      │ │ │
│  │  │ [Visit Marcus] [View History] [Letter Offers]          │ │ │
│  │  └─────────────────────────────────────────────────────────┘ │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │  CROSSBRIDGE                                                │ │
│  │                                                             │ │
│  │  ┌─────────────────────────────────────────────────────────┐ │ │
│  │  │ [⚫] Shadow Contact         │ Relationship: Cold        │ │ │
│  │  │ Location: Crossbridge Dock  │ Tokens: ⚫3               │ │ │
│  │  │ Last Letter: Never          │ Status: Not Available     │ │ │
│  │  │                             │ History: 0 delivered      │ │ │
│  │  │ [Visit Required] [View History] [No Offers]            │ │ │
│  │  └─────────────────────────────────────────────────────────┘ │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

#### **NPC Relationship Card**
```
┌─────────────────────────────────────────────────────────────────┐
│  [💚] Elena Messenger                                           │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │ Location: Millbrook Tavern        │ Relationship: Warm      │ │
│  │ Connection Tokens: 🟢4 🔵1        │ Last Seen: 2 days ago   │ │
│  │ Letters Delivered: 8              │ Letters Skipped: 1      │ │
│  │ Specialty: Personal letters       │ Offer Frequency: Daily  │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│  Available Actions (when at location):                          │
│  • [Share Meal] → +1 🟢 token                                  │
│  • [Personal Chat] → +1 🟢 token                               │
│  • [Request Letter] → Use 3 🟢 tokens                          │
│  • [Crisis Help] → Major token reward                          │
│                                                                 │
│  Current Status: ✅ Available at Millbrook Tavern              │
│  Travel Time: You are here                                      │
│                                                                 │
│  Recent History:                                                │
│  • Day 20: Delivered love letter (+2 🟢)                       │
│  • Day 18: Skipped birthday letter (-1 relationship)           │
│  • Day 15: Delivered urgent message (+1 🟢)                    │
│                                                                 │
│  [Visit Elena] [View Full History] [Letter Offers]             │
└─────────────────────────────────────────────────────────────────┘
```

#### **Relationship History Panel**
```
┌─────────────────────────────────────────────────────────────────┐
│             ELENA RELATIONSHIP HISTORY                         │
├─────────────────────────────────────────────────────────────────┤
│  Relationship Progression:                                      │
│  Day 1: Stranger → Day 5: Contact → Day 12: Ally → Day 18: Friend│
│                                                                 │
│  Token Earning History:                                         │
│  • Personal letters delivered: 6 (+12 🟢)                      │
│  • Social interactions: 4 (+4 🟢)                              │
│  • Crisis assistance: 1 (+3 🟢)                                │
│  • Total earned: 19 🟢 tokens                                  │
│                                                                 │
│  Letter History (Last 10):                                     │
│  ✅ Day 20: Love letter to Marcus (delivered)                   │
│  ❌ Day 18: Birthday invitation (skipped)                       │
│  ✅ Day 15: Urgent message to father (delivered)                │
│  ✅ Day 12: Thank you note (delivered)                          │
│  ✅ Day 10: Personal request (delivered)                        │
│                                                                 │
│  Relationship Events:                                           │
│  • Day 18: Forgave skipped birthday letter                     │
│  • Day 12: Trusted with family secret                          │
│  • Day 8: First personal letter offered                        │
│                                                                 │
│  [Close] [Export History]                                      │
└─────────────────────────────────────────────────────────────────┘
```

---

### **STANDING OBLIGATIONS SCREEN**

#### **Layout Structure**
```
┌─────────────────────────────────────────────────────────────────┐
│                 STANDING OBLIGATIONS                            │
├─────────────────────────────────────────────────────────────────┤
│  [Active] [Available] [Broken] [History]                       │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                    ACTIVE OBLIGATIONS                       │ │
│  │                                                             │ │
│  │  🟣 Noble's Courtesy                    Acquired: Day 12    │ │
│  │  ┌─────────────────────────────────────────────────────────┐ │ │
│  │  │ BENEFITS:                                               │ │ │
│  │  │ • Noble letters enter at slot 5 (not 8)                │ │ │
│  │  │ • +5 coins bonus for noble deliveries                  │ │ │
│  │  │ • Access to noble-only letter chains                   │ │ │
│  │  │                                                         │ │ │
│  │  │ CONSTRAINTS:                                            │ │ │
│  │  │ • Cannot refuse noble letters                           │ │ │
│  │  │ • Noble letters cannot be purged                       │ │ │
│  │  │ • Must have court attire for noble deliveries          │ │ │
│  │  │                                                         │ │ │
│  │  │ QUEUE IMPACT:                                           │ │ │
│  │  │ • Noble letters skip 3 queue positions                 │ │ │
│  │  │ • Forced acceptance creates queue pressure             │ │ │
│  │  │ • Conflicts with shadow obligations                    │ │ │
│  │  └─────────────────────────────────────────────────────────┘ │ │
│  │                                                             │ │
│  │  ⚫ Shadow's Burden                     Acquired: Day 18    │ │
│  │  ┌─────────────────────────────────────────────────────────┐ │ │
│  │  │ BENEFITS:                                               │ │ │
│  │  │ • Shadow letters pay triple coins                      │ │ │
│  │  │ • Access to exclusive shadow networks                  │ │ │
│  │  │ • Shadow letters jump to slot 6                        │ │ │
│  │  │                                                         │ │ │
│  │  │ CONSTRAINTS:                                            │ │ │
│  │  │ • Forced shadow letter every 3 days                    │ │ │
│  │  │ • Shadow letters cannot be refused                     │ │ │
│  │  │ • Risk of law enforcement attention                    │ │ │
│  │  │                                                         │ │ │
│  │  │ QUEUE IMPACT:                                           │ │ │
│  │  │ • Automatic letter generation fills queue              │ │ │
│  │  │ • High-risk letters create time pressure               │ │ │
│  │  │ • Conflicts with noble obligations                     │ │ │
│  │  └─────────────────────────────────────────────────────────┘ │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                  OBLIGATION CONFLICTS                       │ │
│  │                                                             │ │
│  │  🔴 ACTIVE CONFLICT: Noble's Courtesy ↔ Shadow's Burden    │ │
│  │                                                             │ │
│  │  Problem: Noble letters (slot 5) + Shadow letters (slot 6) │ │
│  │           + Forced shadow generation = Queue overcrowding   │ │
│  │                                                             │ │
│  │  Impact: • Harder to accept other letter types             │ │
│  │          • Increased token spending for queue management   │ │
│  │          • Higher risk of expired letters                  │ │
│  │                                                             │ │
│  │  Strategic Options:                                         │ │
│  │  • Specialize in noble/shadow only                         │ │
│  │  • Break one obligation (permanent consequences)           │ │
│  │  • Manage conflict with increased token spending           │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

#### **Obligation Acquisition Panel**
```
┌─────────────────────────────────────────────────────────────────┐
│                   AVAILABLE OBLIGATIONS                        │
├─────────────────────────────────────────────────────────────────┤
│  Requirements: Build deep relationships or complete special letters│
│                                                                 │
│  💚 Heart's Bond                        Requires: 5 🟢 tokens  │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │ Source: Deep relationship with Elena                        │ │
│  │ Trigger: Deliver 3 consecutive personal letters            │ │
│  │                                                             │ │
│  │ Benefits: • Trust letters can extend deadline free         │ │
│  │          • Personal letters jump to slot 4                 │ │
│  │          • Romantic subplot opportunities                  │ │
│  │                                                             │ │
│  │ Constraints: • Double token cost to skip trust letters    │ │
│  │             • Must prioritize personal over professional   │ │
│  │             • Relationship cooling affects all gameplay    │ │
│  │                                                             │ │
│  │ [Accept Heart's Bond] [Learn More] [Decline]               │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│  🔵 Merchant's Priority                 Requires: 7 🔵 tokens  │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │ Source: Consistent trade letter deliveries                 │ │
│  │ Trigger: Complete merchant guild recommendation            │ │
│  │                                                             │ │
│  │ Benefits: • Trade letters +10 coin bonus                   │ │
│  │          • Access to exclusive trade routes                │ │
│  │          • Bulk letter discounts                           │ │
│  │                                                             │ │
│  │ Constraints: • Trade letters cannot be purged              │ │
│  │             • Tighter deadlines on all trade letters      │ │
│  │             • Guild reputation affects all relationships   │ │
│  │                                                             │ │
│  │ [Accept Merchant's Priority] [Learn More] [Decline]        │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

---

## **CROSS-SCREEN NAVIGATION**

### **Main Navigation Menu**
```
┌─────────────────────────────────────────────────────────────────┐
│  📬 Letter Queue │ 👥 Relationships │ ⚖️ Obligations │ 🎒 Inventory │
├─────────────────────────────────────────────────────────────────┤
│  Current: Letter Queue Screen                                   │
│  Notifications: 🔔 2 letters expire tomorrow                   │
└─────────────────────────────────────────────────────────────────┘
```

### **Context-Sensitive Links**
- **From Letter Queue**: Click NPC name → Character Relationship Screen
- **From Relationships**: Click obligation → Standing Obligations Screen
- **From Obligations**: Click affected letter → Letter Queue Screen
- **Universal**: Queue crisis notifications appear on all screens

### **Keyboard Shortcuts**
- **Q**: Letter Queue Screen
- **R**: Character Relationship Screen  
- **O**: Standing Obligations Screen
- **1-8**: Select letter in queue position
- **Space**: Deliver letter at position 1
- **Tab**: Cycle through queue manipulation actions

---

## **RESPONSIVE DESIGN REQUIREMENTS**

### **Mobile Adaptations**
- **Queue Display**: Vertical scrolling for 8 slots
- **Action Buttons**: Larger touch targets
- **Token Counts**: Simplified icon display
- **Screen Switching**: Swipe gestures between screens

### **Accessibility Features**
- **Color Blindness**: Icons + text for all token types
- **Screen Reader**: Alt text for all queue positions and actions
- **High Contrast**: Option for improved deadline visibility
- **Keyboard Navigation**: Full keyboard support for all actions

---

## **IMPLEMENTATION PRIORITY**

### **Phase 1: Core Queue Screen**
1. **8-slot queue display** with basic letter cards
2. **Token balance display** with type icons
3. **Queue action buttons** with cost validation
4. **Basic deadline countdown** visualization

### **Phase 2: Character Relationships**
1. **NPC list display** with per-NPC token counts
2. **Location information** and travel requirements
3. **Relationship history** tracking
4. **Location-based interaction** availability

### **Phase 3: Standing Obligations**
1. **Active obligations** display with effects
2. **Obligation acquisition** system
3. **Conflict detection** and warnings
4. **Queue behavior** modification visualization

### **Phase 4: Integration Polish**
1. **Cross-screen navigation** and context links
2. **Notification system** for queue crises
3. **Keyboard shortcuts** and accessibility
4. **Mobile responsive** adaptations

This specification provides the complete UI framework for the letter queue system, ensuring all three screens work together to support the strategic letter management experience that makes players feel like Kvothe juggling overwhelming social obligations.