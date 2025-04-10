using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string InvokeUrl = "https://integrate.api.nvidia.com/v1/chat/completions";
    private const string ApiKey = "YOUR_API_KEY"; // Replace with your actual API key
    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public IFormFile? ImageFile { get; set; }
    [TempData]
    public string? ImageBase64 { get; set; }
    public string? ResponseText { get; set; }
    public bool IsProcessing { get; set; }

   public async Task<IActionResult> OnPostAsync()
    {
    IsProcessing = true;

    if (ImageFile == null || ImageFile.Length == 0)
    {
        ResponseText = "No file uploaded.";
        IsProcessing = false;
        return Page();
    }

    try
    {
        using var ms = new MemoryStream();
        await ImageFile.CopyToAsync(ms);
        var imageBytes = ms.ToArray();
        var imageBase64 = Convert.ToBase64String(imageBytes);

        if (imageBase64.Length >= 180000)
        {
            ResponseText = "Image too large for inline upload. Use Assets API.";
            IsProcessing = false;
            return Page();
        }

        var message = $"What is in this image? <img src=\"data:image/png;base64,{imageBase64}\" />";
        var payload = new
        {
            model = "meta/llama-4-maverick-17b-128e-instruct",
            messages = new[] { new { role = "user", content = message } },
            max_tokens = 512,
            temperature = 1.00,
            top_p = 1.00,
            stream = false
        };

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var json = JsonSerializer.Serialize(payload);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(InvokeUrl, httpContent);

        if (response.IsSuccessStatusCode)
{
        var apiResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(apiResponse);

        
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

            // Save both the uploaded image (in Base64) and the extracted content into TempData.
            TempData["ImageBase64"] = imageBase64;
            TempData["ResponseText"] = content;
            IsProcessing = false;

        return RedirectToPage("Privacy");
         }
            else
            {
                ResponseText = $"Error: {response.StatusCode}\n{await response.Content.ReadAsStringAsync()}";
                IsProcessing = false;
                return Page();
            }
            }
            catch (Exception ex)
            {
                ResponseText = $"Unexpected error: {ex.Message}";
                IsProcessing = false;
                return Page();
            }
        }


}
