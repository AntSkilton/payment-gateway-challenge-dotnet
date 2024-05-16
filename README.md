# Instructions for candidates

This is the .NET version of the Payment Gateway challenge. If you haven't already read this [README.md](https://github.com/cko-recruitment/.github/tree/beta) on the details of this exercise, please do so now. 

## Template structure
```
src/
    PaymentGateway.Api - a skeleton ASP.NET Core Web API
test/
    PaymentGateway.Api.Tests - an empty xUnit test project
imposters/ - contains the bank simulator configuration. Don't change this

.editorconfig - don't change this. It ensures a consistent set of rules for submissions when reformatting code
docker-compose.yml - configures the bank simulator
PaymentGateway.sln
```

Feel free to change the structure of the solution, use a different test library etc.

---
# Documentation

### Data Types
An `int` won't work for a card number that is 14-19 digits long because an `int` can only hold up to 10 digits. I chose to use a `long` (which can cater upto 19 (or 20 if unsigned)) instead of a string because it's smaller in size.

| long        | string (19 chars) |
|-------------|-------------------|
| 8 bytes     | 38 bytes          |

The variable was called `CardNumberLastFour`, implying 4 characters only, but I wanted it contain true data on the `POST` for accurate and non-truncated record keeping. I renamed the `POST` model to `CardNumber`. When the data is retrieved via the `GET` request however, the safer data security model remains to use the end 4 digits as an `int` version only.

The `Amount` property is already in an `int` format in the backend here, representing the sum in the minor currency unit. If there was an int to float conversion process, I imagine it may occur in the frontend for view purposes only so no floating point inaccuracies could be stored. I too encourage the use of keeping currencies in the lowest most common denomination for data consistency.

### Validation
The `[Required]` attributes and type checking allows the response to throw a `BadRequest 400` error for some of the validation requirements where type checking prevents a `200` response such as the card number field allowing alphabet characters. For other more specific validation checks, I've included the custom logic which passes the response out as a `400` error if encountered too.

To ensure the consumer has a reliable error handling experience, a separate "Error" JSON object model has been made to store the status code and error message for any API consumer.

### Architecture
The repository pattern has been used, and I wanted to extend it as it's a great way to ensure consistency when swapping data sources, and keeps responsibilities seperated.

I've included the use of JSON objects to return a response to the consumer. This allows a consistent way for them to check input validation errors.

### Unit Testing
I've applied my prior experience working in financial tax validation to this system. I've created a validation code lookup system, whereby each code is calculated for the response with a corresponding message on the error.
This not only informs the API consumer on what is exactly wrong, but allows each part of the validation function to be tested in isolation, checking that the code can be asserted under different unit test conditions.

Here is an example response demonstrating 3 errors with their codes and corresponding messages on how to address the errors.
```
{
  "Status": 400,
  "ValidationCodes": [
    6,
    7,
    8
  ],
  "ErrorMessages": [
    "Enter a valid 3 digit ISO code for the currency.",
    "ISO code not recognised for eligible transaction.",
    "CVV needs to be between 3 and 4 characters."
  ]
}
```

I addressed the second provided test from `404` to `204 content not found`, as the response came back valid, just without a data match (as it's a new unique GUID).