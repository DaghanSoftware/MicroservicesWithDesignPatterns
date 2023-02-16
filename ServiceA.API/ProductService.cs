using ServiceA.API.Models;

namespace ServiceA.API
{
    public class ProductService
    {
        private readonly HttpClient _client;
        private readonly ILogger<ProductService> _logger;

        public ProductService(HttpClient client, ILogger<ProductService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Product> GetProducts(int id)
        {
            var products = await _client.GetFromJsonAsync<Product>($"{id}");
            _logger.LogInformation($"Products: {products.Id} - {products.Name}");
            return products;
        }
    }
}
