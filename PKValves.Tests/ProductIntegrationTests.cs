using System.Net;
using PKValves.Models;

namespace PKValves.Tests;

public class ProductIntegrationTests
{
    private static Product SampleProduct()
    {
        return new Product
        {
            Id = 101,
            Name = "Integration Test Valve",
            Category = "Valves",
            Description =
                "A valve used to verify the product details page.",
            Image = "/images/test-valve.png"
        };
    }

    private static void AssertApiCall(
        AccountProductTestFactory factory,
        string path)
    {
        var request = Assert.Single(factory.Requests);

        Assert.Equal("GET", request.Method);
        Assert.Equal(path, request.Path);
    }

    [Fact]
    public async Task ProductList_DisplaysProductsFromApi()
    {
        using var factory =
            new AccountProductTestFactory();

        factory.ApiReply = _ =>
            AccountProductTestFactory.JsonReply(
                HttpStatusCode.OK,
                new[] { SampleProduct() });

        using var client = factory.CreateBrowser();

        using var response =
            await client.GetAsync("/Products");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Contains(
            "Integration Test Valve",
            await response.Content.ReadAsStringAsync());

        AssertApiCall(factory, "/api/products");
    }

    [Fact]
    public async Task ProductDetails_DisplaysRequestedProduct()
    {
        using var factory =
            new AccountProductTestFactory();

        var product = SampleProduct();

        factory.ApiReply = _ =>
            AccountProductTestFactory.JsonReply(
                HttpStatusCode.OK,
                product);

        using var client = factory.CreateBrowser();

        using var response =
            await client.GetAsync("/Products/Details/101");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        string html =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(product.Name, html);
        Assert.Contains(product.Description, html);
        Assert.Contains(product.Image, html);

        AssertApiCall(factory, "/api/products/101");
    }

    [Fact]
    public async Task EmptyCatalogue_LoadsWithoutError()
    {
        using var factory =
            new AccountProductTestFactory();

        factory.ApiReply = _ =>
            AccountProductTestFactory.JsonReply(
                HttpStatusCode.OK,
                Array.Empty<Product>());

        using var client = factory.CreateBrowser();

        using var response =
            await client.GetAsync("/Products");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.DoesNotContain(
            "Integration Test Valve",
            await response.Content.ReadAsStringAsync());

        AssertApiCall(factory, "/api/products");
    }

    [Fact]
    public async Task MissingProduct_ReturnsNotFound()
    {
        using var factory =
            new AccountProductTestFactory();

        factory.ApiReply = _ =>
            AccountProductTestFactory.JsonReply(
                HttpStatusCode.NotFound,
                new
                {
                    success = false,
                    message = "Product not found."
                });

        using var client = factory.CreateBrowser();

        using var response =
            await client.GetAsync("/Products/Details/9999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        AssertApiCall(factory, "/api/products/9999");
    }
}