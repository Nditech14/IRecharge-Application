Usage
Wallet Endpoints
Create Wallet: POST /api/wallet/create
Get Balance: GET /api/wallet/balance
Add Funds: POST /api/wallet/add-funds
Confirm Payment: GET /api/wallet/confirm-payment?reference={reference}
Remove Funds: POST /api/wallet/remove-funds
Deactivate Wallet: PATCH /api/wallet/deactivate
Reactivate Wallet: PATCH /api/wallet/reactivate


Bill Endpoints
Create Bill: POST /api/bill/create
Pay Bill: POST /api/bill/pay/{billId}

Project Structure
Application.DTO: Data Transfer Objects for requests and responses.
Application.Service.Abstraction: Interfaces for Wallet and Bill services.
Application.Service.Implementation: Implementation of the Wallet and Bill services.
Infrastructure.Utilities: Utilities for caching, communication, and common functionalities.
Domain.Entities: CosmosDB entities.
Domain.Enums: Enums for payment statuses.


Technologies Used
ASP.NET Core
Azure CosmosDB: Data storage
Redis: Caching
Paystack: Payment processing
AutoMapper: Object mapping
Email Service: Notifications
