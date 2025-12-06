Imports System.Collections.Concurrent

Namespace CTPBackend.Services

    Public Class PaymentService
        Implements IPaymentService

        Private ReadOnly _payments As New ConcurrentDictionary(Of String, CTPBackend.Models.Payment)

        Public Function CreatePayment(contractorId As String, amount As Decimal, redirectUrl As String) As CTPBackend.Models.Payment Implements IPaymentService.CreatePayment
            Dim orderId = $"ORD-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{contractorId}"

            Dim payment As New CTPBackend.Models.Payment With {
                .OrderId = orderId,
                .ContractorId = contractorId,
                .TransactionId = "",
                .Amount = amount,
                .Currency = "INR",
                .Status = "PENDING",
                .CtpRedirectUrl = redirectUrl,
                .CreatedAt = DateTime.UtcNow,
                .RetryCount = 0
            }

            _payments.TryAdd(orderId, payment)
            Return payment
        End Function

        Public Function GetPayment(orderId As String) As CTPBackend.Models.Payment Implements IPaymentService.GetPayment
            Dim payment As CTPBackend.Models.Payment = Nothing
            If _payments.TryGetValue(orderId, payment) Then
                Return payment
            End If
            Return Nothing
        End Function

        Public Function UpdatePaymentStatus(orderId As String, status As String, failureReason As String) As CTPBackend.Models.Payment Implements IPaymentService.UpdatePaymentStatus
            Dim payment As CTPBackend.Models.Payment = Nothing
            If _payments.TryGetValue(orderId, payment) Then
                payment.Status = status
                payment.UpdatedAt = DateTime.UtcNow
                payment.WebhookReceivedAt = DateTime.UtcNow
                If status = "FAILED" AndAlso Not String.IsNullOrEmpty(failureReason) Then
                    payment.FailureReason = failureReason
                End If
                Return payment
            End If
            Return Nothing
        End Function

        Public Function RetryPayment(orderId As String) As CTPBackend.Models.Payment Implements IPaymentService.RetryPayment
            Dim payment As CTPBackend.Models.Payment = Nothing
            If _payments.TryGetValue(orderId, payment) Then
                payment.Status = "PENDING"
                payment.UpdatedAt = DateTime.UtcNow
                payment.RetryCount += 1
                Return payment
            End If
            Return Nothing
        End Function

        Public Function UpdateTransactionId(orderId As String, transactionId As String) As CTPBackend.Models.Payment
            Dim payment As CTPBackend.Models.Payment = Nothing
            If _payments.TryGetValue(orderId, payment) Then
                payment.TransactionId = transactionId
                Return payment
            End If
            Return Nothing
        End Function
    End Class

End Namespace

