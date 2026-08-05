using FluentAssertions;
using System.Net.Http;
using ShopWave;

namespace ShopWave.Tests
{
    // Integratietests tegen Mockoon. Deze tests doen een echte HTTP-call naar een Mockoon-server
    // op http://localhost:3001 en kunnen daarom niet slagen in een geautomatiseerde `dotnet test`
    // zonder dat Mockoon handmatig gestart is. Ze zijn daarom gemarkeerd met Skip zodat de testrun
    // groen blijft. Verwijder de Skip als Mockoon draait met de routes uit de les. Zie README.
    public class ShippingClientIntegrationTests
    {
        private const string MockoonBaseUrl = "http://localhost:3001";

        private const string MockoonSkip =
            "Vereist een draaiende Mockoon-server op http://localhost:3001 (zie README).";

        [Fact(Skip = MockoonSkip)]
        public async Task GetShippingRateAsync_WithValidRequest_ReturnsTarief()
        {
            // Arrange
            HttpClient     httpClient = new HttpClient();
            ShippingClient client     = new ShippingClient(httpClient, MockoonBaseUrl);

            // Act
            ShippingResponse result = await client.GetShippingRateAsync(
                destination: "Antwerpen",
                weight: 2.5);

            // Assert
            result.Should().NotBeNull();
            result.Tarief.Should().Be(6.99);
            result.Vervoerder.Should().Be("DHL");

            httpClient.Dispose();
        }

        [Theory(Skip = MockoonSkip)]
        [InlineData("Antwerpen", 2.5, 6.99, "DHL")]
        [InlineData("Brussel",   1.0, 4.49, "bpost")]
        public async Task GetShippingRateAsync_WithKnownDestination_ReturnsCorrectTarief(
            string destination,
            double weight,
            double expectedTarief,
            string expectedVervoerder)
        {
            // Arrange
            HttpClient     httpClient = new HttpClient();
            ShippingClient client     = new ShippingClient(httpClient, MockoonBaseUrl);

            // Act
            ShippingResponse result = await client.GetShippingRateAsync(
                destination: destination,
                weight:      weight);

            // Assert
            result.Should().NotBeNull();
            result.Tarief.Should().Be(expectedTarief);
            result.Vervoerder.Should().Be(expectedVervoerder);

            httpClient.Dispose();
        }

        [Fact(Skip = MockoonSkip)]
        public async Task GetShippingRateAsync_WhenServerReturns500_ThrowsHttpRequestException()
        {
            // Arrange
            HttpClient     httpClient = new HttpClient();
            ShippingClient client     = new ShippingClient(httpClient, MockoonBaseUrl);

            // Act
            Func<Task> act = async () =>
            {
                await client.GetShippingRateAsync(destination: "FOUT", weight: 1.0);
            };

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();

            httpClient.Dispose();
        }

        [Theory(Skip = MockoonSkip)]
        [InlineData("FOUT")]
        [InlineData("ONBEKEND")]
        [InlineData("OFFLINE")]
        public async Task GetShippingRateAsync_WithErrorDestination_ThrowsHttpRequestException(
            string destination)
        {
            // Arrange
            HttpClient     httpClient = new HttpClient();
            ShippingClient client     = new ShippingClient(httpClient, MockoonBaseUrl);

            // Act
            Func<Task> act = async () =>
            {
                await client.GetShippingRateAsync(destination: destination, weight: 1.0);
            };

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();

            httpClient.Dispose();
        }

        [Fact(Skip = MockoonSkip)]
        public async Task GetShippingRateAsync_WhenRequestTimesOut_ThrowsTaskCanceledException()
        {
            // Arrange
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout    = TimeSpan.FromSeconds(2);
            ShippingClient client = new ShippingClient(httpClient, MockoonBaseUrl);

            // Act
            Func<Task> act = async () =>
            {
                await client.GetShippingRateAsync(destination: "Antwerpen", weight: 1.0);
            };

            // Assert
            await act.Should().ThrowAsync<TaskCanceledException>();

            httpClient.Dispose();
        }
    }
}
