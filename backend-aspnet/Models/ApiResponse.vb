Namespace CTPBackend.Models

    Public Class ApiResponse
        Public Property Success As Boolean
        Public Property Message As String

        Public Sub New()
        End Sub

        Public Sub New(success As Boolean, message As String)
            Me.Success = success
            Me.Message = message
        End Sub
    End Class

    Public Class ContractorResponse
        Inherits ApiResponse
        Public Property ContractorId As String
        Public Property RegistrationFee As Decimal
        Public Property Contractor As Contractor
    End Class

    Public Class PaymentInitiateResponse
        Inherits ApiResponse
        Public Property OrderId As String
        Public Property TransactionId As String
        Public Property RedirectUrl As String
    End Class

    Public Class PaymentVerifyResponse
        Inherits ApiResponse
        Public Property Status As String
        Public Property Payment As Payment
        Public Property Contractor As Contractor
        Public Property Reason As String
    End Class

    Public Class PaymentRetryResponse
        Inherits ApiResponse
        Public Property RedirectUrl As String
    End Class

End Namespace


