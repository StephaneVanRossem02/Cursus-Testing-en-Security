using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using FluentAssertions;
using ShopWave;

namespace ShopWave.Tests
{
    public class ShippingClientWireMockTests
    {
        [Fact]
        public async Task GetShippingRateAsync_WithWireMock_ReturnsTarief()
        {
            // Arrange
            WireMockServer server = WireMockServer.Start();

            server.Given(
                Request.Create()
                       .WithPath("/api/verzending")
                       .UsingGet())
            .RespondWith(
                Response.Create()
                        .WithStatusCode(200)
                        .WithBodyAsJson(new
                        {
                            bestemming = "Antwerpen",
                            gewicht    = 2.5,
                            tarief     = 6.99,
                            vervoerder = "DHL"
                        }));

            HttpClient     httpClient = new HttpClient();
            ShippingClient client     = new ShippingClient(httpClient, server.Url!);

            // Act
            ShippingResponse result = await client.GetShippingRateAsync(
                destination: "Antwerpen",
                weight: 2.5);

            // Assert
            result.Should().NotBeNull();
            result.Tarief.Should().Be(6.99);
            result.Vervoerder.Should().Be("DHL");

            httpClient.Dispose();
            server.Stop();
        }

        [Fact]
        public async Task GetShippingRateAsync_WithWireMock_WhenServerReturns500_ThrowsHttpRequestException()
        {
            // Arrange
            WireMockServer server = WireMockServer.Start();

            server.Given(
                Request.Create()
                       .WithPath("/api/verzending")
                       .UsingGet())
            .RespondWith(
                Response.Create()
                        .WithStatusCode(500));

            HttpClient     httpClient = new HttpClient();
            ShippingClient client     = new ShippingClient(httpClient, server.Url!);

            // Act
            Func<Task> act = async () =>
            {
                await client.GetShippingRateAsync(destination: "Antwerpen", weight: 1.0);
            };

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();

            httpClient.Dispose();
            server.Stop();
        }
    }
}
