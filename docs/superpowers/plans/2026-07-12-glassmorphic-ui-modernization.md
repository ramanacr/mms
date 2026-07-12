# Glassmorphic UI Modernization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Modernize the MeetingScheduler Angular client with a restrained frosted-glass dashboard UI while preserving current workflows.

**Architecture:** This is a visual modernization pass on the existing standalone Angular shell. The work stays in the current component template/style files and global stylesheet, preserving component state, services, forms, calendar options, and routes.

**Tech Stack:** Angular 22, SCSS, FullCalendar, Phosphor Icons, Vitest.

## Global Constraints

- Keep the current information architecture, data flow, Angular signals/forms, FullCalendar usage, and booking/room/admin workflows intact.
- Use a frosted enterprise glass style: cool atmospheric page background, translucent navigation and content surfaces, crisp teal/blue accents, compact operational density, and readable typography.
- The modernization should not add inert decorative controls.
- Maintain native form controls, semantic buttons, visible focus states, and sufficient contrast on translucent surfaces.
- Mobile layouts must preserve readable controls and avoid horizontal overflow except for the existing responsive table wrapper.

---

### Task 1: Global Design Tokens And Base Controls

**Files:**
- Modify: `MeetingScheduler.Client/src/styles.scss`

**Interfaces:**
- Consumes: Existing global classes `.btn`, `.btn-secondary`, `.btn-block`, `.btn-small`, `.login-page`, `.onboarding-page`, `.brand-mark`, `.auth-card`.
- Produces: Updated global button and auth styling used by the shell, login, and onboarding screens.

- [ ] **Step 1: Check current tests before visual edits**

Run: `npm test -- --run`

Expected: Vitest exits successfully or exposes unrelated pre-existing failures to record before editing.

- [ ] **Step 2: Update global SCSS tokens and buttons**

Replace the global background/button/auth-card styling with cool glass-compatible colors, restrained gradients, stronger focus states, and no markup changes.

- [ ] **Step 3: Run the focused client test command**

Run: `npm test -- --run`

Expected: Existing tests keep the same pass/fail status as Step 1; no new failures from the style-only change.

### Task 2: Scheduler Shell Glass Surfaces

**Files:**
- Modify: `MeetingScheduler.Client/src/app/features/shell/scheduler-shell.component.scss`
- Optionally modify: `MeetingScheduler.Client/src/app/features/shell/scheduler-shell.component.html`

**Interfaces:**
- Consumes: Existing shell class names such as `.app-shell`, `.sidebar`, `.workspace`, `.topbar`, `.stats-grid`, `.stat-card`, `.panel`, `.status-pill`, `.data-table`, `.modal-panel`, `.meeting-drawer`.
- Produces: A frosted dashboard shell that keeps existing Angular bindings and event handlers unchanged.

- [ ] **Step 1: Modernize the app background and sidebar**

Update `:host`, `.app-shell`, `.sidebar`, `.brand`, `.brand-logo`, `.sidebar button`, and responsive sidebar rules to create a glass rail with active/hover states and stable mobile scrolling.

- [ ] **Step 2: Modernize workspace panels and metrics**

Update `.workspace`, `.topbar`, `.status-pill`, `.notice`, `.stats-grid`, `.stat-card`, `.panel`, `.panel-header`, `.panel-body`, and utility rows to use translucent surfaces, softer shadows, and compact readable spacing.

- [ ] **Step 3: Modernize table, forms, chips, and editor**

Update `.data-table`, `.form-control`, `.chip-input`, `.chip`, `.rich-editor`, `.editor-toolbar`, and `.editor-surface` to match the glass surface system while preserving native controls.

- [ ] **Step 4: Modernize overlays**

Update `.modal-backdrop`, `.drawer-backdrop`, `.modal-panel`, `.meeting-drawer`, `.modal-header`, `.drawer-header`, and `.icon-button` for blur, elevation, and accessible hit targets.

- [ ] **Step 5: Blend FullCalendar into the design system**

Add scoped `:host ::ng-deep` rules for `.fc` buttons, toolbar title, table cells, events, and scrollgrid borders so the calendar no longer reads as a default embedded widget.

- [ ] **Step 6: Run the focused client test command**

Run: `npm test -- --run`

Expected: Existing tests keep the same pass/fail status as Task 1.

### Task 3: Rendered QA And Build Verification

**Files:**
- Modify only if QA reveals a visual defect in: `MeetingScheduler.Client/src/styles.scss`
- Modify only if QA reveals a visual defect in: `MeetingScheduler.Client/src/app/features/shell/scheduler-shell.component.scss`

**Interfaces:**
- Consumes: The implemented styles from Tasks 1 and 2.
- Produces: Verified desktop and mobile layouts without clipping, overlap, or unusable controls.

- [ ] **Step 1: Run production build**

Run: `npm run build`

Expected: Angular build exits with code 0.

- [ ] **Step 2: Start the dev server**

Run: `npm start -- --host 127.0.0.1 --port 4200`

Expected: Angular dev server serves the client at `http://127.0.0.1:4200/`.

- [ ] **Step 3: Inspect desktop and mobile render**

Use browser/Playwright screenshots at desktop and mobile widths. Check dashboard, rooms table, calendar, modal, and meeting drawer for overlap, clipping, unreadable text, default-looking calendar controls, and broken responsive layout.

- [ ] **Step 4: Fix any visual defects and rerun verification**

Make only targeted SCSS fixes, then rerun `npm test -- --run` and `npm run build`.
