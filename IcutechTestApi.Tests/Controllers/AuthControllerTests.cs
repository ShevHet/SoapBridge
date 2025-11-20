using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using IcutechTestApi.Controllers;
using IcutechTestApi.Clients;
using IcutechTestApi.DTOs;
using IcutechTestApi.Models;
using AuthResponse = IcutechTestApi.DTOs.AuthResponse<object>;
using RegisterResponse = IcutechTestApi.DTOs.RegisterResponse;
using AuthErrorResponse = IcutechTestApi.DTOs.AuthErrorResponse;

namespace IcutechTestApi.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<ISoapAuthClient> _mockSoapAuthClient;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockSoapAuthClient = new Mock<ISoapAuthClient>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockSoapAuthClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithSuccessResponse()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Login = "testuser",
            Password = "testpassword"
        };

        var loginResult = new LoginResult
        {
            Success = true,
            Message = "Login successful",
            EntityDetails = new { UserId = "123", Name = "Test User" }
        };

        _mockSoapAuthClient
            .Setup(x => x.LoginAsync(request.Login, request.Password))
            .ReturnsAsync(loginResult);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponse<object>>().Subject;
        
        response.Success.Should().BeTrue();
        response.Entity.Should().NotBeNull();
        
        _mockSoapAuthClient.Verify(x => x.LoginAsync(request.Login, request.Password), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401WithErrorResponse()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Login = "testuser",
            Password = "wrongpassword"
        };

        var loginResult = new LoginResult
        {
            Success = false,
            Message = "Invalid credentials",
            EntityDetails = null
        };

        _mockSoapAuthClient
            .Setup(x => x.LoginAsync(request.Login, request.Password))
            .ReturnsAsync(loginResult);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var response = unauthorizedResult.Value.Should().BeOfType<AuthErrorResponse>().Subject;
        
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Invalid credentials");
        
        _mockSoapAuthClient.Verify(x => x.LoginAsync(request.Login, request.Password), Times.Once);
    }

    [Fact]
    public async Task Login_WithSoapClientException_Returns500WithErrorResponse()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Login = "testuser",
            Password = "testpassword"
        };

        _mockSoapAuthClient
            .Setup(x => x.LoginAsync(request.Login, request.Password))
            .ThrowsAsync(new SoapClientException("Network error"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        
        var response = statusCodeResult.Value.Should().BeOfType<AuthErrorResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Network error");
        
        _mockSoapAuthClient.Verify(x => x.LoginAsync(request.Login, request.Password), Times.Once);
    }

    [Fact]
    public async Task Login_WithArgumentException_Returns400WithErrorResponse()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Login = "testuser",
            Password = "testpassword"
        };

        _mockSoapAuthClient
            .Setup(x => x.LoginAsync(request.Login, request.Password))
            .ThrowsAsync(new ArgumentException("Login cannot be empty"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<AuthErrorResponse>().Subject;
        
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Login cannot be empty");
        
        _mockSoapAuthClient.Verify(x => x.LoginAsync(request.Login, request.Password), Times.Once);
    }

    [Fact]
    public async Task Login_WithGenericException_Returns500WithErrorResponse()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Login = "testuser",
            Password = "testpassword"
        };

        _mockSoapAuthClient
            .Setup(x => x.LoginAsync(request.Login, request.Password))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        
        var response = statusCodeResult.Value.Should().BeOfType<AuthErrorResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Внутренняя ошибка сервера");
        
        _mockSoapAuthClient.Verify(x => x.LoginAsync(request.Login, request.Password), Times.Once);
    }

    [Fact]
    public async Task Register_WithValidData_Returns200WithSuccessResponse()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Login = "newuser",
            Password = "securepassword123",
            Email = "newuser@example.com",
            FirstName = "Иван",
            LastName = "Иванов"
        };

        var registerResult = new RegisterResult
        {
            Success = true,
            Message = "Registration successful",
            CreatedCustomerId = "12345"
        };

        _mockSoapAuthClient
            .Setup(x => x.RegisterNewCustomerAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(registerResult);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<RegisterResponse>().Subject;
        
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Registration successful");
        response.CreatedCustomerId.Should().Be("12345");
        
        _mockSoapAuthClient.Verify(x => x.RegisterNewCustomerAsync(It.Is<RegisterRequest>(
            r => r.Login == request.Login && 
                 r.Password == request.Password &&
                 r.Email == request.Email &&
                 r.FirstName == request.FirstName &&
                 r.LastName == request.LastName)), Times.Once);
    }

    [Fact]
    public async Task Register_WithFailedRegistration_Returns400WithErrorResponse()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Login = "existinguser",
            Password = "password123",
            Email = "existing@example.com"
        };

        var registerResult = new RegisterResult
        {
            Success = false,
            Message = "User already exists",
            CreatedCustomerId = null
        };

        _mockSoapAuthClient
            .Setup(x => x.RegisterNewCustomerAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(registerResult);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<AuthErrorResponse>().Subject;
        
        response.Success.Should().BeFalse();
        response.Message.Should().Be("User already exists");
        
        _mockSoapAuthClient.Verify(x => x.RegisterNewCustomerAsync(It.IsAny<RegisterRequest>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithSoapClientException_Returns500WithErrorResponse()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Login = "newuser",
            Password = "password123",
            Email = "user@example.com"
        };

        _mockSoapAuthClient
            .Setup(x => x.RegisterNewCustomerAsync(It.IsAny<RegisterRequest>()))
            .ThrowsAsync(new SoapClientException("Network error"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        
        var response = statusCodeResult.Value.Should().BeOfType<AuthErrorResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Network error");
        
        _mockSoapAuthClient.Verify(x => x.RegisterNewCustomerAsync(It.IsAny<RegisterRequest>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithArgumentException_Returns400WithErrorResponse()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Login = "",
            Password = "password123"
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<AuthErrorResponse>().Subject;
        
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Login and password are required");
        
        _mockSoapAuthClient.Verify(x => x.RegisterNewCustomerAsync(It.IsAny<RegisterRequest>()), Times.Never);
    }

    [Fact]
    public async Task Register_WithGenericException_Returns500WithErrorResponse()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Login = "newuser",
            Password = "password123"
        };

        _mockSoapAuthClient
            .Setup(x => x.RegisterNewCustomerAsync(It.IsAny<RegisterRequest>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        
        var response = statusCodeResult.Value.Should().BeOfType<AuthErrorResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Внутренняя ошибка сервера");
        
        _mockSoapAuthClient.Verify(x => x.RegisterNewCustomerAsync(It.IsAny<RegisterRequest>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithNullOptionalFields_HandlesCorrectly()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Login = "newuser",
            Password = "password123",
            Email = null,
            FirstName = null,
            LastName = null
        };

        var registerResult = new RegisterResult
        {
            Success = true,
            Message = "Registration successful",
            CreatedCustomerId = "12345"
        };

        _mockSoapAuthClient
            .Setup(x => x.RegisterNewCustomerAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(registerResult);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<RegisterResponse>().Subject;
        
        response.Success.Should().BeTrue();
        
        _mockSoapAuthClient.Verify(x => x.RegisterNewCustomerAsync(It.Is<RegisterRequest>(
            r => r.Login == request.Login && 
                 r.Password == request.Password &&
                 r.Email == null &&
                 r.FirstName == null &&
                 r.LastName == null)), Times.Once);
    }
}

