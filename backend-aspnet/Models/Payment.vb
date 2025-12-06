Namespace CTPBackend.Models

    Public Class Payment
        Public Property OrderId As String
        Public Property ContractorId As String
        Public Property TransactionId As String
        Public Property Amount As Decimal
        Public Property Currency As String
        Public Property Status As String
        Public Property CtpRedirectUrl As String
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime?
        Public Property WebhookReceivedAt As DateTime?
        Public Property RetryCount As Integer
        Public Property FailureReason As String
    End Class

End Namespace


