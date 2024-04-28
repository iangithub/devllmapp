using Microsoft.AspNetCore.Mvc;

namespace CustomerSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerFeedbackController : ControllerBase
    {
        [Consumes("application/json")]
        [HttpPost]
        public IActionResult ReceiveFeedback(Feedback feedback)
        {
            Console.WriteLine($"Feedback received: {feedback.Summary}, {feedback.Emotion}, {feedback.Classification}");
            return Ok("Feedback received successfully");
        }
    }

    public class Feedback
    {
        public string Summary { get; set; }
        public string Emotion { get; set; }
        public string Classification { get; set; }
    }
}