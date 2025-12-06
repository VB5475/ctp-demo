Namespace CTPBackend.Services

    Public Interface ICtpService
        Function GenerateSignature(payload As CTPBackend.Models.CtpPayload) As String
        Function VerifyWebhookSignature(payload As CTPBackend.Models.CtpWebhookPayload, receivedSignature As String) As Boolean
        Function CreatePayment(payload As CTPBackend.Models.CtpPayload) As Task(Of CTPBackend.Models.CtpResponse)
        Function GetPaymentStatus(transactionId As String) As Task(Of CTPBackend.Models.CtpStatusResponse)
    End Interface

End Namespace

