# Software Requirements Specification

## BMB Web-Based Ordering System

| Document information | Value |
|---|---|
| Version | 1.1 |
| Status | Implementation baseline; deployment pending |
| Prepared by | Demiana |
| Target platform | ASP.NET Core Web API (.NET 6), SQL Server, IIS |

## 1. Introduction

### 1.1 Purpose

This document defines the functional and non-functional requirements for the BMB Web-Based Ordering System. The system allows customers to register, authenticate, create orders, view orders, and delete their own orders. It also monitors order deletions and temporarily prevents abusive customers from creating new orders.

### 1.2 Scope

The solution consists of:

- A frontend built with HTML, CSS, and JavaScript.
- A RESTful ASP.NET Core Web API targeting .NET 6.
- A Microsoft SQL Server database accessed through Entity Framework Core.
- JWT-based authentication and authorization.
- IIS hosting with HTTPS-only access.

Payment processing, product inventory management, shipping, email verification, and password recovery are outside the initial scope.

### 1.3 Important technology note

.NET 6 is used because it is explicitly required by the technical task. It is no longer supported by Microsoft and should be upgraded to a currently supported LTS release before using the application in a real production environment.

## 2. Actors

### 2.1 Customer

A registered user who can:

- Log in.
- Create orders when not temporarily banned.
- View their orders.
- Retrieve one of their orders by ID.
- Delete their own orders.

### 2.2 Administrator

An authorized user who can retrieve orders belonging to all customers.

The administrator can also retrieve registered customers, inspect their current ban status, and view a selected customer's complete order history.

## 3. Functional Requirements

### FR-01: Customer registration

The system shall allow a new customer to register using:

- Full name.
- Email address.
- Password.
- Password confirmation.

The system shall reject an email address that is already registered. Email comparison shall be case-insensitive.

### FR-02: Customer login

The system shall allow a registered customer to log in using their email address and password. Successful authentication shall return a JWT access token.

### FR-03: Create an order

An authenticated customer who is not temporarily banned shall be able to create an order containing one or more order items.

Order totals shall be calculated and validated by the backend. The API shall not trust a total supplied by the frontend.

### FR-04: Retrieve the current customer's orders

An authenticated customer shall be able to retrieve their orders. A customer shall not receive another customer's orders.

### FR-05: Retrieve an order by ID

An authenticated customer shall be able to retrieve one of their orders using its ID. The system shall not disclose an order that belongs to another customer.

### FR-06: Retrieve all orders

An authenticated administrator shall be able to retrieve orders belonging to all customers. This operation shall not be available to ordinary customers.

### FR-07: Delete an order

An authenticated customer shall be able to delete an order that belongs to them. Orders shall be soft-deleted so the system can retain an audit trail.

Deleting an order that is already deleted shall not create another deletion event or increase the qualifying deletion count.

### FR-08: Monitor qualifying deletions

A deletion qualifies for monitoring when the order is created and deleted on the same UTC calendar date.

The system shall count qualifying deletions separately for each customer and calendar date.

### FR-09: Apply the temporary ban

When a customer's third qualifying deletion occurs on the same calendar date, the system shall immediately set the customer's ban expiry to six hours after that deletion.

During the ban, the customer:

- Shall not be able to create new orders.
- Shall be able to log in.
- Shall be able to view existing orders.
- Shall be able to delete an existing order.

The ban shall expire automatically when its expiry time is reached. No background job or manual administrator action shall be required.

## 4. Order Data

Each order shall contain:

- Order ID.
- Unique order number.
- Customer ID.
- One or more order items.
- Status.
- Total amount.
- Creation date and time in UTC.
- Deletion date and time in UTC, when applicable.

Each order item shall contain:

- Product name.
- Quantity greater than zero.
- Unit price greater than or equal to zero.
- Calculated line total.

## 5. API Requirements

The initial API shall provide the following operations:

| Method | Endpoint | Access | Purpose |
|---|---|---|---|
| `POST` | `/api/v1/auth/register` | Anonymous | Register a customer |
| `POST` | `/api/v1/auth/login` | Anonymous | Authenticate a customer |
| `POST` | `/api/v1/orders` | Customer | Create an order |
| `GET` | `/api/v1/orders` | Customer | Retrieve the current customer's orders |
| `GET` | `/api/v1/orders/{id}` | Customer | Retrieve an owned order by ID |
| `DELETE` | `/api/v1/orders/{id}` | Customer | Delete an owned order |
| `GET` | `/api/v1/orders/all` | Administrator | Retrieve all customers' orders |
| `GET` | `/api/v1/admin/customers` | Administrator | Retrieve registered customers and ban status |
| `GET` | `/api/v1/admin/customers/{customerId}/orders` | Administrator | Retrieve one customer's complete order history |

API errors shall use a consistent Problem Details response format.

## 6. Security Requirements

- Registration and login shall allow anonymous access.
- All order operations shall require a valid authenticated identity.
- Authentication shall use JWT bearer tokens.
- Passwords shall be hashed and never stored or logged as plaintext.
- The API shall obtain the customer ID from the authenticated token, not from a customer ID supplied by the request body.
- Resource ownership shall be checked on the server.
- Administrative operations shall use role- or policy-based authorization.
- JWT issuer, audience, signature, and expiry shall be validated.
- JWT signing keys, production connection strings, and credentials shall not be committed to source control.
- Production access shall use HTTPS only.
- The bundled frontend shall use same-origin API calls. If a separate cross-origin client is introduced, CORS shall be restricted to approved origins.
- Logs shall not contain passwords, tokens, or other sensitive credentials.

## 7. Data and Time Requirements

- Microsoft SQL Server shall be used for persistent storage.
- Entity Framework Core migrations shall define and version the database schema.
- All timestamps shall be stored in UTC.
- The user interface may display UTC timestamps in the user's local timezone.
- Time-dependent business rules shall use a centralized clock abstraction so they can be tested reliably.
- Deletion and ban changes shall be saved atomically.
- Concurrency shall be handled so simultaneous delete requests cannot bypass the ban rule.

## 8. Non-Functional Requirements

### 8.1 Maintainability

- The solution shall follow Clean Architecture dependency rules.
- Business rules shall be separated from API and database implementation details.
- Public classes and methods shall have clear, intention-revealing names.
- Request and response DTOs shall be separated from database entities.

### 8.2 Reliability

- Unexpected failures shall return safe error responses without exposing stack traces.
- Database operations involved in the deletion rule shall be transactional.
- Important business rules shall be covered by automated tests.

### 8.3 Performance

- The technical-task implementation assumes a small dataset. Pagination should be added before use with large production datasets.
- Appropriate indexes shall be added for customer, order, and deletion queries.
- Database queries shall avoid loading unnecessary data.

### 8.4 Usability

- Forms shall provide clear validation messages.
- Destructive operations shall require confirmation.
- The frontend shall provide loading, empty, success, and error states.
- A temporarily banned customer shall see the ban expiry time.

### 8.5 Deployment

- The API shall be published in Release configuration.
- The application shall be hosted on Microsoft IIS.
- IIS shall use a valid TLS certificate and an HTTPS binding.
- Plain HTTP shall be disabled or redirected to HTTPS.
- Production configuration shall be provided through environment-specific configuration or IIS settings.

## 9. Assumptions

- Email addresses uniquely identify customers.
- An order belongs to exactly one customer.
- A customer can manage only their own orders.
- Deleted orders are retained for audit purposes and excluded from the normal active-order list.
- "Same day" means the same UTC calendar date until the stakeholder specifies another timezone.
- The third qualifying deletion activates the ban immediately.
- A new qualifying deletion does not extend an already active ban unless the business owner requests this behavior.
- Authentication remains available while a customer is banned.
- The administrator role is included to secure the requirement to retrieve all orders.
- Product catalog, stock control, checkout, payment, shipping, email verification, and password reset are outside the requested scope.

## 10. Acceptance Criteria

The implementation shall be accepted when:

1. A new customer can register with a unique email address.
2. A registered customer can log in and obtain a valid access token.
3. Anonymous requests to protected endpoints receive an unauthorized response.
4. An authenticated, non-banned customer can create an order.
5. An authenticated customer can retrieve only their own orders.
6. A customer cannot retrieve or delete another customer's order.
7. Deleting an order retains a deletion audit record.
8. The third qualifying deletion activates a six-hour creation ban.
9. A banned customer receives a clear error when attempting to create an order.
10. Order creation succeeds after the ban expires.
11. Only an administrator can retrieve all customers' orders.
12. Automated unit, integration, and architecture tests cover authentication, authorization, ownership, deletion counting, ban expiry, persistence, and dependency rules.
13. The database can be created from version-controlled migrations.
14. The API is deployed to IIS and is accessible through HTTPS only.

## 11. Implementation Decisions Requiring Stakeholder Confirmation

| Topic | Implemented decision |
|---|---|
| Definition of same day | UTC calendar date |
| Customer actions during a ban | Login, viewing, and further deletion remain available; creation is blocked |
| Deletion during an active ban | Does not extend the existing six-hour period |
| Order content | Product name, quantity, and unit price per item; totals generated by the server |
| Deleted-order visibility | Hidden from the customer's normal list and retained for administrator audit views |
| Administrative access | Configuration-approved administrator emails receive an Administrator JWT role at login |
| Framework version | .NET 6 is retained to satisfy the task despite being out of support |

## 12. Traceability Summary

| Original requirement | Specification coverage |
|---|---|
| Register and log in customers | FR-01, FR-02 |
| Place a new order | FR-03 |
| View created orders | FR-04, FR-05, FR-06 |
| Delete an order | FR-07 |
| Monitor three same-day deletions | FR-08, FR-09 |
| Administrator customer and order review | FR-06, Section 5 |
| Secure API access | Section 6 |
| SQL Server | Section 7 |
| IIS and HTTPS only | Section 8.5 |
