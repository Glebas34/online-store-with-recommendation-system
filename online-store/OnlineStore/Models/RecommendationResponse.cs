namespace OnlineStore.Models;

public class RecommendationResponse
{
    public string UserId { get; set; }
    public List<string> Recommendations { get; set; }
}
