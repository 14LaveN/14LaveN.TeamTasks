using System;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace TeamTasks.Identity.Tests.Api.e2e;

/// <summary>
/// Represents the e2e authorization test.
/// </summary>
public sealed class AuthorizationEndToEndTest
    : IDisposable
{
    private readonly IWebDriver _driver = new EdgeDriver();

    [Fact]
    public void HomePage_Should_Display_WelcomeMessage()
    {
        _driver.Navigate().GoToUrl("https://localhost:7135");

        var welcomeMessage = _driver.FindElement(By.XPath("//h1[contains(text(), 'Welcome')]"));

        welcomeMessage.Should().NotBeNull();
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}