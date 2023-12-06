using System;
using System.IO;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using ScavengeRUs.Services;
using System.Net;
using ScavengeRUs.Models.Enums;

public class Functions
{
    private readonly IConfiguration _configuration;
    public Functions(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
	/// most major cell service providers provide a way to send SMS via email.
	/// just send an email to [phone number]@[carrier gateway url]
	/// and the owner of that phone number will recieve your email as a text message
	/// </summary>
	private static readonly Dictionary<Carriers, string> _carrierToGateway = new Dictionary<Carriers, string>{
        { Carriers.ATT, "txt.att.net" },
        { Carriers.BoostMobile, "sms.myboostmobile.com" }, // untested
		{ Carriers.Charter, "vtext.com" },
        { Carriers.Cricket, "sms.cricketwireless.net" }, // untested
		{ Carriers.Sprint, "messaging.sprintpcs.net" }, // untested
		{ Carriers.StraightTalk, "vtext.com" }, // uses verizon's url, contrary to what documentation says
		{ Carriers.TMobile, "tmomail.net" },
        { Carriers.TracFone, "txt.att.net" },
        { Carriers.Verizon, "vtext.com" },
        { Carriers.VirginMobile, "vmobl.com" } // untested
	};


    // these fields are configurable values for our smtp client and secrets
    private static readonly string _applicationEmailAddress = "scavengerus23@gmail.com";
    private static readonly string smtpURL = "smtp.gmail.com";
    private static readonly int smtpPort = 587;
    private static readonly string secretsFileName = "gmail.txt";


    /// <summary>
    /// uses an SMTP client running an existing buc hunt email and google's SMTP service to send emails to recipients email address
    /// </summary>
    /// <param name="recipientEmail"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public static async Task SendEmail(string recipientEmail, string subject, string body)
    {
        // our SMTP server is tied to an gmail account and sends emails on behalf of this account.
        // we keep the password in a file by itself not tracked by source control. if you are part
        // of a future group of SE1 students and need the login info, please contact
        // either SCUTTW@ETSU.EDU, or grantscutt2@gmail.com, preferably from a school account
        string secretsFilePath = Path.Combine(Directory.GetCurrentDirectory(), secretsFileName);
        string emailPassword = File.ReadAllText(secretsFilePath).Trim();

        // Configure SMTP client
        using (SmtpClient client = new SmtpClient(smtpURL, smtpPort))
        {
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_applicationEmailAddress, emailPassword);
            client.EnableSsl = true;

            // Create email message
            using (MailMessage message = new MailMessage(_applicationEmailAddress, recipientEmail))
            {
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                // NOTE: can throw SmtpException, but we don't really know what to do if it does, so we don't catch it
                client.Send(message);
            }
        }
    }


    /// <summary>
    /// sends an email to a phone number to be recieved as SMS
    /// </summary>
    /// <param name="carrier"></param>
    /// <param name="recipientPhNumber"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public static async Task SendSMS(Carriers carrier, string recipientPhNumber, string body)
    {
        string recipientAddress = $"{recipientPhNumber}@{_carrierToGateway[carrier]}";
        await SendEmail(recipientAddress, "", body);
    }

}