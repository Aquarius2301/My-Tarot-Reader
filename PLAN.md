# My Tarot Reader Plan

This document tracks the suggested work for completing and growing the product. Items are grouped by priority so the application can be improved incrementally.

## P0 - Before wider production use

- [ ] Add CSRF protection for cookie-authenticated mutations.
- [ ] Add rate limiting for OAuth, guest draws, AI readings, wallet actions, and other write endpoints.
- [ ] Disable or restrict public test endpoints outside the Development environment.
- [ ] Replace raw request/response logging with structured, redacted logs and a clear retention policy.
- [ ] Define and complete the streak saver flow, or remove the saver UI until the flow is implemented.
- [ ] Add integration tests for authentication cookies, refresh-token rotation, Redis cooldowns, wallet transactions, and AI coin charging/refunds.
- [ ] Run backend tests in CI and add frontend component or end-to-end tests for the main user journeys.
- [ ] Add health checks and monitoring for PostgreSQL, Redis, Gemini, authentication failures, and background workers.

## P1 - High-value product features

- [ ] Let users enter a specific question before requesting an AI reading.
- [ ] Add AI follow-up chat so users can ask questions about an existing reading.
- [ ] Display the complete card spread in AI results, including positions, orientation, and card detail links.
- [ ] Add pagination, filtering, date ranges, and search to tarot and AI reading history.
- [ ] Add favorites or bookmarks for cards and readings.
- [ ] Add an account and privacy page with data export, account deletion, and session/device revocation.
- [ ] Preserve or migrate guest readings when a user signs in, or clearly explain the guest data policy.
- [ ] Add wallet transaction history with coin source, spending, expiry, conversion, and refund details.
- [ ] Add payment or top-up support only after transaction idempotency, webhook verification, receipts, and refunds are designed.

## P2 - Growth and engagement

- [ ] Add shareable reading links, image export, or PDF export with privacy controls.
- [ ] Add a calendar and insights dashboard for draws, streaks, favorite cards, and reading topics.
- [ ] Add email or browser reminders for daily draws, check-ins, and completed cooldowns.
- [ ] Add named spread presets with meaningful card positions, such as love, career, decision, and three-card guidance.
- [ ] Personalize recommendations using timezone, preferences, and reading history.
- [ ] Support additional card decks and configurable visual themes.
- [ ] Consider public or community readings only after moderation, reporting, privacy, and abuse-prevention workflows are ready.

## Recommended implementation order

1. Production security, test coverage, health checks, and logging.
2. Specific questions for AI readings.
3. AI follow-up chat.
4. History pagination, filtering, and favorites.
5. Account privacy controls and wallet transaction history.
6. Sharing, reminders, analytics, and community features.
