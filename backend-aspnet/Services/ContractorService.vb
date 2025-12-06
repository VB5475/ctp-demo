Imports System.Collections.Concurrent

Namespace CTPBackend.Services

    Public Class ContractorService
        Implements IContractorService

        Private ReadOnly _contractors As New ConcurrentDictionary(Of String, CTPBackend.Models.Contractor)

        Public Function RegisterContractor(contractorName As String, email As String, phone As String, workType As String, address As String) As CTPBackend.Models.Contractor Implements IContractorService.RegisterContractor
            Dim contractorId = $"CONT-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"

            Dim contractor As New CTPBackend.Models.Contractor With {
                .ContractorId = contractorId,
                .ContractorName = contractorName,
                .Email = email,
                .Phone = phone,
                .WorkType = If(String.IsNullOrEmpty(workType), "Road Cutting", workType),
                .Address = If(String.IsNullOrEmpty(address), "", address),
                .Status = "PENDING_PAYMENT",
                .PaymentStatus = "UNPAID",
                .RegistrationFee = 1500,
                .CreatedAt = DateTime.UtcNow
            }

            _contractors.TryAdd(contractorId, contractor)
            Console.WriteLine($"✅ Contractor registered: {contractorId}")

            Return contractor
        End Function

        Public Function GetContractor(contractorId As String) As CTPBackend.Models.Contractor Implements IContractorService.GetContractor
            Dim contractor As CTPBackend.Models.Contractor = Nothing
            If _contractors.TryGetValue(contractorId, contractor) Then
                Return contractor
            End If
            Return Nothing
        End Function

        Public Function UpdateContractorStatus(contractorId As String, status As String, paymentStatus As String) As CTPBackend.Models.Contractor Implements IContractorService.UpdateContractorStatus
            Dim contractor As CTPBackend.Models.Contractor = Nothing
            If _contractors.TryGetValue(contractorId, contractor) Then
                contractor.Status = status
                contractor.PaymentStatus = paymentStatus
                If status = "ACTIVE" Then
                    contractor.RegistrationCompletedAt = DateTime.UtcNow
                End If
                Return contractor
            End If
            Return Nothing
        End Function
    End Class

End Namespace

