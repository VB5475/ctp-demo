Namespace CTPBackend.Services

    Public Interface IPaymentService
        Function CreatePayment(contractorId As String, amount As Decimal, redirectUrl As String) As CTPBackend.Models.Payment
        Function GetPayment(orderId As String) As CTPBackend.Models.Payment
        Function UpdatePaymentStatus(orderId As String, status As String, failureReason As String) As CTPBackend.Models.Payment
        Function RetryPayment(orderId As String) As CTPBackend.Models.Payment
    End Interface

End Namespace

