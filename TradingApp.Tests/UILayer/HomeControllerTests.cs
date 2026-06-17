using Microsoft.AspNetCore.Mvc;
using Moq;
using TradingApp.BusinessLayer.DTOs;
using TradingApp.BusinessLayer.Services;
using TradingApp.UILayer.Controllers;

namespace TradingApp.Tests.UILayer;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsViewWithPairsFromCache()
    {
        var expectedPairs = new List<CurrencyPairDto>
        {
            new()
            {
                Id = 1,
                DisplayName = "USD / ILS",
                BaseCurrencyCode = "USD",
                QuoteCurrencyCode = "ILS",
                CurrentValue = 3.6500m,
                MinValue = 3.6000m,
                MaxValue = 3.7000m
            }
        };

        var cacheMock = new Mock<ITradingCache>();
        cacheMock.Setup(c => c.GetPairsForView()).Returns(expectedPairs);

        var controller = new HomeController(cacheMock.Object);

        var result = controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IReadOnlyList<CurrencyPairDto>>(viewResult.Model);
        Assert.Same(expectedPairs, model);
        cacheMock.Verify(c => c.GetPairsForView(), Times.Once);
    }

    [Fact]
    public void Privacy_ReturnsViewWithoutModel()
    {
        var cacheMock = new Mock<ITradingCache>();
        var controller = new HomeController(cacheMock.Object);

        var result = controller.Privacy();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.Model);
    }
}
