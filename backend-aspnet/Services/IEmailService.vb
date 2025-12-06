Namespace CTPBackend.Services

    Public Interface IEmailService
        Function SendEmailAsync(toEmail As String, subject As String, body As String) As Task
    End Interface

End Namespace


