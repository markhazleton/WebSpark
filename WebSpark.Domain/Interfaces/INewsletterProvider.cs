using WebSpark.Domain.Entities;

namespace WebSpark.Domain.Interfaces;

public interface INewsletterProvider
{
    Task<List<Subscriber>> GetSubscribers();
    Task<bool> AddSubscriber(Subscriber subscriber);
    Task<bool> RemoveSubscriber(int id);

    Task<List<Newsletter>> GetNewsletters();
    Task<bool> SendNewsletter(int postId);
    Task<bool> RemoveNewsletter(int id);

    Task<MailSetting> GetMailSettings();
    Task<bool> SaveMailSettings(MailSetting mail);
}
