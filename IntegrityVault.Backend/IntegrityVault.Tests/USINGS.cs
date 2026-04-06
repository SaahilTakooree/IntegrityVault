// Import xUnit globally so all test files can use its attributes and assertions.
global using Xunit;

// Import Moq globally to enable mocking dependencies in unit tests.
global using Moq;

// Import FluentAssertions globally for more readable and expressive assertions.
global using FluentAssertions;

// Import service layer interfaces globally for use in tests.
global using IntegrityVault.Service.Interfaces;

// Import service layer implentation globally for use in tests.
global using IntegrityVault.Service.Implementations;

// Import repository layer interfaces globally for use in tests.
global using IntegrityVault.Repository.Interfaces;

// Import repository layer implentation globally for use in tests.
global using IntegrityVault.Repository.Implementations;

// Import API layer controllers golbally for use in test.
global using IntegrityVault.Api.Controllers;

// Import the DTOs from the common layer.
global using IntegrityVault.Common.DTOs;

// Import Entity Framework Core for database context and LINQ queries in tests.
global using Microsoft.EntityFrameworkCore;

// Import ASP.NET Core MVC for testing Controller actions and ActionResult types.
global using Microsoft.AspNetCore.Mvc;

// Import Configuration to mock or access appsettings and environment variables.
global using Microsoft.Extensions.Configuration;

// Import Domain Entities from the common layer to use as data models in tests.
global using IntegrityVault.Common.Entities;

// Import Shared Enums from the common layer for status and type verification.
global using IntegrityVault.Common.Enums;