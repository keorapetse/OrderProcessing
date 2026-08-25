# Order Processing API

A .NET Web API for processing customer orders, including inventory validation,
inventory reservation, payment processing, and order confirmation.

## Technologies

- ASP.NET Core Web API
- Entity Framework Core
- EF Core In-Memory Database
- HttpClient
- Swagger 

## Features

- Create orders
- Retrieve orders by ID
- Retrieve paginated orders
- Update order status
- Validate inventory availability
- Reserve inventory
- Release reserved inventory when order processing fails
- Process payments
- Cancel orders when processing fails
- Clear API error responses

## Order Processing Flow

1. Validate the order request
2. Check inventory availability
3. Reserve inventory
4. Process payment
5. Confirm the order
6. Release reserved inventory if payment or processing fails

## Running the Application

Clone the repository and run:

```bash
dotnet restore
dotnet build
dotnet run