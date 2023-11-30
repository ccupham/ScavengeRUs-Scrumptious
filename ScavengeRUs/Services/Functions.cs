using System;
using System.IO;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Google.Cloud.SecretManager.V1;
using ScavengeRUs.Services;


public class Functions
{
    private readonly IConfiguration _configuration;
    public Functions(IConfiguration configuration)
	{
        _configuration = configuration;
    }

	public async Task SendEmail(string recipientEmail, string subject, string body)
    {
        // Set the environment variable
        string credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "scrum-bums-042e94718ef6.json");
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

        // Get email settings
        string emailFrom = _configuration.GetValue<string>("EmailSettings:EmailFrom");
        // Use the Secret Manager API to get the email password
        string password = GetSecretAsync("etsuscavengerhuntemail").Result;

        Console.WriteLine(emailFrom);
        Console.WriteLine(password);

        // Configure SMTP client
        SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
        client.EnableSsl = true;
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.UseDefaultCredentials = false;
        client.Credentials = new System.Net.NetworkCredential(emailFrom, password);

        // Create email message
        MailMessage message = new MailMessage(emailFrom, recipientEmail);
        message.Subject = subject;
        message.Body = body;

        try
        {
            // Send email
            client.Send(message);
        }
        catch (Exception ex)
        {
            // Handle exceptions
            throw ex;
        }
    }

    private async Task<string> GetSecretAsync(string secretId)
    {
        var client = await SecretManagerServiceClient.CreateAsync();
        var secretName = new SecretName("341278893241", secretId);
        var latestVersion = await client.GetSecretVersionAsync(new GetSecretVersionRequest
        {
            SecretVersionName = new SecretVersionName(secretName.ProjectId, secretName.SecretId, "latest")
        });
        var accessRequest = new AccessSecretVersionRequest
        {
            Name = latestVersion.SecretVersionName.ToString(),
        };
        var response = await client.AccessSecretVersionAsync(accessRequest);
        var passwordBytes = response.Payload.Data;
        string password = passwordBytes.ToStringUtf8();
        return password;
    }

}