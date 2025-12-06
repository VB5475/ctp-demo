Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Net.Http
Imports Microsoft.Extensions.Configuration

Namespace CTPBackend.Services

    Public Class CtpService
        Implements ICtpService

        Private ReadOnly _configuration As IConfiguration
        Private ReadOnly _httpClient As HttpClient

        Public Sub New(configuration As IConfiguration)
            _configuration = configuration
            _httpClient = New HttpClient()
        End Sub

        Public Function GenerateSignature(payload As CTPBackend.Models.CtpPayload) As String Implements ICtpService.GenerateSignature
            Dim envSecret = Environment.GetEnvironmentVariable("CTP_API_SECRET")
            Dim configSecret = _configuration("CtpSettings:ApiSecret")
            Dim secret = If(Not String.IsNullOrEmpty(envSecret), envSecret, If(Not String.IsNullOrEmpty(configSecret), configSecret, ""))

            Dim dataString = $"{payload.MerchantId}|{payload.OrderId}|{payload.Amount}|{payload.Timestamp}"

            Using hmac = New HMACSHA256(Encoding.UTF8.GetBytes(secret))
                Dim hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString))
                Return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant()
            End Using
        End Function

        Public Function VerifyWebhookSignature(payload As CTPBackend.Models.CtpWebhookPayload, receivedSignature As String) As Boolean Implements ICtpService.VerifyWebhookSignature
            Dim envSecret = Environment.GetEnvironmentVariable("CTP_WEBHOOK_SECRET")
            Dim configSecret = _configuration("CtpSettings:WebhookSecret")
            Dim secret = If(Not String.IsNullOrEmpty(envSecret), envSecret, If(Not String.IsNullOrEmpty(configSecret), configSecret, ""))

            Dim dataString = $"{payload.TransactionId}|{payload.OrderId}|{payload.Status}|{payload.Amount}"

            Using hmac = New HMACSHA256(Encoding.UTF8.GetBytes(secret))
                Dim hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString))
                Dim calculatedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant()
                Return calculatedSignature = receivedSignature
            End Using
        End Function

        Public Async Function CreatePayment(payload As CTPBackend.Models.CtpPayload) As Task(Of CTPBackend.Models.CtpResponse) Implements ICtpService.CreatePayment
            Dim useRealCtp = If(Environment.GetEnvironmentVariable("USE_REAL_CTP") = "true", True, False)

            If useRealCtp Then
                Dim envApiKey = Environment.GetEnvironmentVariable("CTP_API_KEY")
                Dim configApiKey = _configuration("CtpSettings:ApiKey")
                Dim apiKey = If(Not String.IsNullOrEmpty(envApiKey), envApiKey, If(Not String.IsNullOrEmpty(configApiKey), configApiKey, ""))

                Dim envBaseUrl = Environment.GetEnvironmentVariable("CTP_API_BASE_URL")
                Dim configBaseUrl = _configuration("CtpSettings:BaseUrl")
                Dim baseUrl = If(Not String.IsNullOrEmpty(envBaseUrl), envBaseUrl, If(Not String.IsNullOrEmpty(configBaseUrl), configBaseUrl, ""))

                Dim signature = GenerateSignature(payload)

                _httpClient.DefaultRequestHeaders.Clear()
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}")
                _httpClient.DefaultRequestHeaders.Add("X-Signature", signature)

                Dim json = JsonSerializer.Serialize(payload)
                Dim content = New StringContent(json, Encoding.UTF8, "application/json")

                Dim response = Await _httpClient.PostAsync($"{baseUrl}/payments/create", content)
                Dim responseContent = Await response.Content.ReadAsStringAsync()

                Return JsonSerializer.Deserialize(Of CTPBackend.Models.CtpResponse)(responseContent)
            Else
                ' Mock response for testing
                Return New CTPBackend.Models.CtpResponse With {
                    .Success = True,
                    .TransactionId = $"CTP-TXN-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    .RedirectUrl = $"http://localhost:4000/mock-ctp-payment?orderId={payload.OrderId}&txnId=CTP-TXN-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
                }
            End If
        End Function

        Public Async Function GetPaymentStatus(transactionId As String) As Task(Of CTPBackend.Models.CtpStatusResponse) Implements ICtpService.GetPaymentStatus
            Dim useRealCtp = If(Environment.GetEnvironmentVariable("USE_REAL_CTP") = "true", True, False)

            If useRealCtp Then
                Dim envApiKey = Environment.GetEnvironmentVariable("CTP_API_KEY")
                Dim configApiKey = _configuration("CtpSettings:ApiKey")
                Dim apiKey = If(Not String.IsNullOrEmpty(envApiKey), envApiKey, If(Not String.IsNullOrEmpty(configApiKey), configApiKey, ""))

                Dim envBaseUrl = Environment.GetEnvironmentVariable("CTP_API_BASE_URL")
                Dim configBaseUrl = _configuration("CtpSettings:BaseUrl")
                Dim baseUrl = If(Not String.IsNullOrEmpty(envBaseUrl), envBaseUrl, If(Not String.IsNullOrEmpty(configBaseUrl), configBaseUrl, ""))

                _httpClient.DefaultRequestHeaders.Clear()
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}")

                Dim response = Await _httpClient.GetAsync($"{baseUrl}/payments/status/{transactionId}")
                Dim responseContent = Await response.Content.ReadAsStringAsync()

                Return JsonSerializer.Deserialize(Of CTPBackend.Models.CtpStatusResponse)(responseContent)
            Else
                ' Mock status - returns PENDING by default (will be updated by payment service)
                Return New CTPBackend.Models.CtpStatusResponse With {
                    .Status = "PENDING",
                    .TransactionId = transactionId
                }
            End If
        End Function
    End Class

End Namespace

