Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.Extensions.Configuration

Namespace CTPBackend.Controllers

    <ApiController>
    <Route("api/[controller]")>
    Public Class PaymentsController
        Inherits ControllerBase

        Private ReadOnly _paymentService As CTPBackend.Services.IPaymentService
        Private ReadOnly _contractorService As CTPBackend.Services.IContractorService
        Private ReadOnly _ctpService As CTPBackend.Services.ICtpService
        Private ReadOnly _emailService As CTPBackend.Services.IEmailService
        Private ReadOnly _configuration As IConfiguration

        Public Sub New(
            paymentService As CTPBackend.Services.IPaymentService,
            contractorService As CTPBackend.Services.IContractorService,
            ctpService As CTPBackend.Services.ICtpService,
            emailService As CTPBackend.Services.IEmailService,
            configuration As IConfiguration
        )
            _paymentService = paymentService
            _contractorService = contractorService
            _ctpService = ctpService
            _emailService = emailService
            _configuration = configuration
        End Sub

        <HttpPost("initiate")>
        Public Async Function InitiatePayment(<FromBody> request As InitiatePaymentRequest) As Task(Of ActionResult)
            Try
                Dim contractor = _contractorService.GetContractor(request.ContractorId)
                If contractor Is Nothing Then
                    Return NotFound(New CTPBackend.Models.ApiResponse With {
                        .Success = False,
                        .Message = "Contractor not found"
                    })
                End If

                If contractor.PaymentStatus = "PAID" Then
                    Return BadRequest(New CTPBackend.Models.ApiResponse With {
                        .Success = False,
                        .Message = "Payment already completed"
                    })
                End If

                Dim envAppUrl = Environment.GetEnvironmentVariable("APP_URL")
                Dim configAppUrl = _configuration("AppSettings:Url")
                Dim appUrl = If(Not String.IsNullOrEmpty(envAppUrl), envAppUrl, If(Not String.IsNullOrEmpty(configAppUrl), configAppUrl, "http://localhost:4000"))
                Dim timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()

                Dim envMerchantId = Environment.GetEnvironmentVariable("CTP_MERCHANT_ID")
                Dim configMerchantId = _configuration("CtpSettings:MerchantId")
                Dim merchantId = If(Not String.IsNullOrEmpty(envMerchantId), envMerchantId, If(Not String.IsNullOrEmpty(configMerchantId), configMerchantId, ""))

                Dim ctpPayload As New CTPBackend.Models.CtpPayload With {
                    .MerchantId = merchantId,
                    .OrderId = "",
                    .Amount = request.Amount,
                    .Currency = "INR",
                    .CustomerName = contractor.ContractorName,
                    .CustomerEmail = contractor.Email,
                    .CustomerPhone = contractor.Phone,
                    .ReturnUrl = $"{appUrl}/payment/callback",
                    .WebhookUrl = $"{appUrl}/api/payments/webhook",
                    .Timestamp = timestamp,
                    .Metadata = New Dictionary(Of String, String) From {
                        {"contractorId", contractor.ContractorId},
                        {"workType", contractor.WorkType}
                    }
                }

                Dim payment = _paymentService.CreatePayment(request.ContractorId, request.Amount, "")
                ctpPayload.OrderId = payment.OrderId

                Console.WriteLine($"🔄 Initiating CTP payment for {request.ContractorId}...")

                Dim ctpResponse = Await _ctpService.CreatePayment(ctpPayload)

                ' Update payment with transaction ID and redirect URL
                Dim paymentServiceCasted = TryCast(_paymentService, CTPBackend.Services.PaymentService)
                If paymentServiceCasted IsNot Nothing Then
                    paymentServiceCasted.UpdateTransactionId(payment.OrderId, ctpResponse.TransactionId)
                End If

                payment = _paymentService.GetPayment(payment.OrderId)
                If payment IsNot Nothing Then
                    payment.CtpRedirectUrl = ctpResponse.RedirectUrl
                End If

                Console.WriteLine($"✅ Payment initiated: {payment.OrderId}")

                Return Ok(New CTPBackend.Models.PaymentInitiateResponse With {
                    .Success = True,
                    .OrderId = payment.OrderId,
                    .TransactionId = ctpResponse.TransactionId,
                    .RedirectUrl = ctpResponse.RedirectUrl
                })

            Catch ex As Exception
                Console.WriteLine($"❌ Payment initiation error: {ex.Message}")
                Return StatusCode(500, New CTPBackend.Models.ApiResponse With {
                    .Success = False,
                    .Message = "Failed to initiate payment. Please try again."
                })
            End Try
        End Function

        <HttpPost("webhook")>
        Public Async Function Webhook(<FromBody> payload As CTPBackend.Models.CtpWebhookPayload) As Task(Of ActionResult)
            Try
                Console.WriteLine($"🔔 Webhook received for {payload.OrderId}: {payload.Status}")

                Dim isValid = _ctpService.VerifyWebhookSignature(payload, payload.Signature)
                If Not isValid Then
                    Console.WriteLine("⚠️ INVALID WEBHOOK SIGNATURE!")
                    Return Unauthorized(New CTPBackend.Models.ApiResponse With {
                        .Success = False,
                        .Message = "Invalid signature"
                    })
                End If

                Dim payment = _paymentService.GetPayment(payload.OrderId)
                If payment Is Nothing Then
                    Console.WriteLine($"❌ Payment not found: {payload.OrderId}")
                    Return NotFound(New CTPBackend.Models.ApiResponse With {
                        .Success = False,
                        .Message = "Payment not found"
                    })
                End If

                If payment.Status = payload.Status Then
                    Console.WriteLine($"⚠️ Duplicate webhook ignored: {payload.OrderId}")
                    Return Ok(New CTPBackend.Models.ApiResponse With {.Success = True, .Message = "Already processed"})
                End If

                _paymentService.UpdatePaymentStatus(payload.OrderId, payload.Status, payload.FailureReason)
                payment = _paymentService.GetPayment(payload.OrderId)

                Dim contractor = _contractorService.GetContractor(payment.ContractorId)
                If contractor Is Nothing Then
                    Console.WriteLine($"❌ Contractor not found: {payment.ContractorId}")
                    Return NotFound(New CTPBackend.Models.ApiResponse With {
                        .Success = False,
                        .Message = "Contractor not found"
                    })
                End If

                If payload.Status = "SUCCESS" Then
                    _contractorService.UpdateContractorStatus(payment.ContractorId, "ACTIVE", "PAID")
                    Console.WriteLine($"✅ Contractor activated: {contractor.ContractorId}")

                    Await _emailService.SendEmailAsync(
                        contractor.Email,
                        "Registration Successful!",
                        $"Hello {contractor.ContractorName},{vbCrLf}{vbCrLf}Your contractor registration is now complete!{vbCrLf}{vbCrLf}Payment Amount: Rs. {payload.Amount}{vbCrLf}Transaction ID: {payload.TransactionId}{vbCrLf}{vbCrLf}You can now access your dashboard.{vbCrLf}{vbCrLf}Thank you!"
                    )
                End If

                If payload.Status = "FAILED" Then
                    _contractorService.UpdateContractorStatus(payment.ContractorId, "PAYMENT_FAILED", "FAILED")
                    Console.WriteLine($"❌ Payment failed for: {contractor.ContractorId}")

                    Dim envAppUrl = Environment.GetEnvironmentVariable("APP_URL")
                    Dim configAppUrl = _configuration("AppSettings:Url")
                    Dim appUrl = If(Not String.IsNullOrEmpty(envAppUrl), envAppUrl, If(Not String.IsNullOrEmpty(configAppUrl), configAppUrl, "http://localhost:4000"))
                    Await _emailService.SendEmailAsync(
                        contractor.Email,
                        "Payment Failed - Please Retry",
                        $"Hello {contractor.ContractorName},{vbCrLf}{vbCrLf}Your payment of Rs. {payload.Amount} failed.{vbCrLf}{vbCrLf}Reason: {If(String.IsNullOrEmpty(payload.FailureReason), "Unknown error", payload.FailureReason)}{vbCrLf}{vbCrLf}Please retry payment to complete registration:{vbCrLf}{appUrl}/contractor/retry-payment/{payload.OrderId}{vbCrLf}{vbCrLf}If issue persists, contact support."
                    )
                End If

                Return Ok(New CTPBackend.Models.ApiResponse With {.Success = True})

            Catch ex As Exception
                Console.WriteLine($"❌ Webhook error: {ex.Message}")
                Return Ok(New CTPBackend.Models.ApiResponse With {.Success = True, .Message = ex.Message})
            End Try
        End Function

        <HttpGet("verify/{orderId}")>
        Public Async Function VerifyPayment(orderId As String) As Task(Of ActionResult)
            Try
                Dim payment = _paymentService.GetPayment(orderId)

                If payment Is Nothing Then
                    Return NotFound(New CTPBackend.Models.PaymentVerifyResponse With {
                        .Success = False,
                        .Status = "NOT_FOUND",
                        .Message = "Payment not found"
                    })
                End If

                If payment.Status = "SUCCESS" Then
                    Dim contractor = _contractorService.GetContractor(payment.ContractorId)
                    Return Ok(New CTPBackend.Models.PaymentVerifyResponse With {
                        .Success = True,
                        .Status = "SUCCESS",
                        .Payment = payment,
                        .Contractor = contractor
                    })
                End If

                If payment.Status = "FAILED" Then
                    Return Ok(New CTPBackend.Models.PaymentVerifyResponse With {
                        .Success = False,
                        .Status = "FAILED",
                        .Reason = payment.FailureReason,
                        .Payment = payment
                    })
                End If

                ' If still PENDING, check with CTP API
                Console.WriteLine($"🔄 Checking CTP status for {orderId}...")

                Try
                    Dim ctpStatus = Await _ctpService.GetPaymentStatus(payment.TransactionId)

                    ' If mock mode, check time-based auto-success
                    Dim useRealCtp = If(Environment.GetEnvironmentVariable("USE_REAL_CTP") = "true", True, False)
                    If Not useRealCtp Then
                        Dim createdTime = payment.CreatedAt
                        Dim now = DateTime.UtcNow
                        If (now - createdTime).TotalSeconds > 10 Then
                            ctpStatus.Status = "SUCCESS"
                        End If
                    End If

                    _paymentService.UpdatePaymentStatus(payment.OrderId, ctpStatus.Status, "")

                    If ctpStatus.Status = "SUCCESS" Then
                        Dim contractor = _contractorService.GetContractor(payment.ContractorId)
                        _contractorService.UpdateContractorStatus(payment.ContractorId, "ACTIVE", "PAID")
                    End If

                    payment = _paymentService.GetPayment(orderId)

                    Return Ok(New CTPBackend.Models.PaymentVerifyResponse With {
                        .Success = payment.Status = "SUCCESS",
                        .Status = payment.Status,
                        .Payment = payment
                    })

                Catch ex As Exception
                    Console.WriteLine($"❌ CTP status check failed: {ex.Message}")
                    Return Ok(New CTPBackend.Models.PaymentVerifyResponse With {
                        .Success = False,
                        .Status = "PENDING",
                        .Message = "Payment is being processed"
                    })
                End Try

            Catch ex As Exception
                Console.WriteLine($"❌ Verify error: {ex.Message}")
                Return StatusCode(500, New CTPBackend.Models.ApiResponse With {
                    .Success = False,
                    .Message = "Error verifying payment"
                })
            End Try
        End Function

        <HttpPost("retry/{orderId}")>
        Public Async Function RetryPayment(orderId As String) As Task(Of ActionResult)
            Try
                Dim payment = _paymentService.GetPayment(orderId)

                If payment Is Nothing Then
                    Return NotFound(New CTPBackend.Models.ApiResponse With {
                        .Success = False,
                        .Message = "Payment not found"
                    })
                End If

                If payment.Status = "SUCCESS" Then
                    Return BadRequest(New CTPBackend.Models.ApiResponse With {
                        .Success = False,
                        .Message = "Payment already completed"
                    })
                End If

                Dim contractor = _contractorService.GetContractor(payment.ContractorId)
                Dim envAppUrl = Environment.GetEnvironmentVariable("APP_URL")
                Dim configAppUrl = _configuration("AppSettings:Url")
                Dim appUrl = If(Not String.IsNullOrEmpty(envAppUrl), envAppUrl, If(Not String.IsNullOrEmpty(configAppUrl), configAppUrl, "http://localhost:4000"))
                Dim timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()

                Dim envMerchantId = Environment.GetEnvironmentVariable("CTP_MERCHANT_ID")
                Dim configMerchantId = _configuration("CtpSettings:MerchantId")
                Dim merchantId = If(Not String.IsNullOrEmpty(envMerchantId), envMerchantId, If(Not String.IsNullOrEmpty(configMerchantId), configMerchantId, ""))

                Dim ctpPayload As New CTPBackend.Models.CtpPayload With {
                    .MerchantId = merchantId,
                    .OrderId = orderId,
                    .Amount = payment.Amount,
                    .Currency = "INR",
                    .CustomerName = contractor.ContractorName,
                    .CustomerEmail = contractor.Email,
                    .CustomerPhone = contractor.Phone,
                    .ReturnUrl = $"{appUrl}/payment/callback",
                    .WebhookUrl = $"{appUrl}/api/payments/webhook",
                    .Timestamp = timestamp
                }

                Dim ctpResponse = Await _ctpService.CreatePayment(ctpPayload)

                _paymentService.RetryPayment(orderId)
                Dim paymentServiceCasted = TryCast(_paymentService, CTPBackend.Services.PaymentService)
                If paymentServiceCasted IsNot Nothing Then
                    paymentServiceCasted.UpdateTransactionId(orderId, ctpResponse.TransactionId)
                End If

                Console.WriteLine($"🔄 Payment retry initiated: {orderId}")

                Return Ok(New CTPBackend.Models.PaymentRetryResponse With {
                    .Success = True,
                    .RedirectUrl = ctpResponse.RedirectUrl
                })

            Catch ex As Exception
                Console.WriteLine($"❌ Retry error: {ex.Message}")
                Return StatusCode(500, New CTPBackend.Models.ApiResponse With {
                    .Success = False,
                    .Message = "Failed to retry payment"
                })
            End Try
        End Function

        Public Class InitiatePaymentRequest
            Public Property ContractorId As String
            Public Property Amount As Decimal
        End Class
    End Class

End Namespace

