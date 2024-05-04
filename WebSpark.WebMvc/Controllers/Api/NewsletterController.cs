using Microsoft.AspNetCore.Authorization;
using WebSpark.Domain.Entities;
using WebSpark.Domain.Interfaces;

namespace WebSpark.WebMvc.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class NewsletterController(INewsletterProvider newsletterProvider) : ControllerBase
{
    protected readonly INewsletterProvider _newsletterProvider = newsletterProvider;

    [Authorize]
    [HttpGet("mailsettings")]
    public async Task<MailSetting> GetMailSettings()
    {
        return await _newsletterProvider.GetMailSettings();
    }

    [Authorize]
    [HttpGet("newsletters")]
    public async Task<List<Newsletter>> GetNewsletters()
    {
        return await _newsletterProvider.GetNewsletters();
    }

    [Authorize]
    [HttpGet("subscribers")]
    public async Task<List<Subscriber>> GetSubscribers()
    {
        return await _newsletterProvider.GetSubscribers();
    }

    [Authorize]
    [HttpDelete("remove/{id:int}")]
    public async Task<ActionResult<bool>> RemoveNewsletter(int id)
    {
        return await _newsletterProvider.RemoveNewsletter(id);
    }

    [HttpDelete("unsubscribe/{id:int}")]
    public async Task<ActionResult<bool>> RemoveSubscriber(int id)
    {
        return await _newsletterProvider.RemoveSubscriber(id);
    }

    [Authorize]
    [HttpPut("mailsettings")]
    public async Task<ActionResult<bool>> SaveMailSettings([FromBody] MailSetting mailSettings)
    {
        return await _newsletterProvider.SaveMailSettings(mailSettings);
    }

    [Authorize]
    [HttpGet("send/{postId:int}")]
    public async Task<bool> SendNewsletter(int postId)
    {
        return await _newsletterProvider.SendNewsletter(postId);
    }

    [HttpPost("subscribe")]
    public async Task<ActionResult<bool>> Subscribe([FromBody] Subscriber subscriber)
    {
        return await _newsletterProvider.AddSubscriber(subscriber);
    }
}
