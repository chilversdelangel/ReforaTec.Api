using System.Reflection;

namespace ReforaTec.Api.Infrastructure.Email;

internal static class EmailTemplates
{
    private static readonly string OtpTemplateHtml =
        LoadEmbeddedTemplate("ReforaTec.Api.Infrastructure.Email.Templates.OtpTemplate.html");

    public static EmailMessage CreateOtpEmail(string recipientEmail, string otpCode)
    {
        var subject = $"ReforaTec — Tu código de verificación es {otpCode}";

        var textBody =
            $"""
             Hola,

             Tu código de verificación para acceder a ReforaTec es:

             {otpCode}

             Este código expirará en 5 minutos y es de un solo uso.

             Si no solicitaste este código, puedes ignorar este mensaje de forma segura.

             — Equipo ReforaTec
             """;

        var htmlBody = OtpTemplateHtml.Replace("{{OTP_CODE}}", otpCode);

        return new EmailMessage(
            To: recipientEmail,
            Subject: subject,
            TextBody: textBody,
            HtmlBody: htmlBody);
    }

    private static string LoadEmbeddedTemplate(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException($"Embedded email template '{resourceName}' not found in assembly.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}