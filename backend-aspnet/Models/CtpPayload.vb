Namespace CTPBackend.Models

    Public Class CtpPayload
        Public Property MerchantId As String
        Public Property OrderId As String
        Public Property Amount As Decimal
        Public Property Currency As String
        Public Property CustomerName As String
        Public Property CustomerEmail As String
        Public Property CustomerPhone As String
        Public Property ReturnUrl As String
        Public Property WebhookUrl As String
        Public Property Timestamp As Long
        Public Property Metadata As Dictionary(Of String, String)
    End Class

    Public Class CtpWebhookPayload
        Public Property TransactionId As String
        Public Property OrderId As String
        Public Property Status As String
        Public Property Amount As Decimal
        Public Property Signature As String
        Public Property FailureReason As String
    End Class

    Public Class CtpResponse
        Public Property Success As Boolean
        Public Property TransactionId As String
        Public Property RedirectUrl As String
    End Class

    Public Class CtpStatusResponse
        Public Property Status As String
        Public Property TransactionId As String
    End Class

End Namespace


