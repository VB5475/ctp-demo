Namespace CTPBackend.Services

    Public Class EmailService
        Implements IEmailService

        Public Async Function SendEmailAsync(toEmail As String, subject As String, body As String) As Task Implements IEmailService.SendEmailAsync
            Console.WriteLine($"📧 EMAIL SENT TO: {toEmail}")
            Console.WriteLine($"Subject: {subject}")
            Console.WriteLine($"Body: {body}")
            ' TODO: Implement actual email sending with SMTP/SendGrid
            Await Task.CompletedTask
        End Function
    End Class

End Namespace


