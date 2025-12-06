Namespace CTPBackend.Services

    Public Interface IContractorService
        Function RegisterContractor(contractorName As String, email As String, phone As String, workType As String, address As String) As CTPBackend.Models.Contractor
        Function GetContractor(contractorId As String) As CTPBackend.Models.Contractor
        Function UpdateContractorStatus(contractorId As String, status As String, paymentStatus As String) As CTPBackend.Models.Contractor
    End Interface

End Namespace

