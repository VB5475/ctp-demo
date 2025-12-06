Namespace CTPBackend.Models

    Public Class Contractor
        Public Property ContractorId As String
        Public Property ContractorName As String
        Public Property Email As String
        Public Property Phone As String
        Public Property WorkType As String
        Public Property Address As String
        Public Property Status As String
        Public Property PaymentStatus As String
        Public Property RegistrationFee As Decimal
        Public Property CreatedAt As DateTime
        Public Property RegistrationCompletedAt As DateTime?
    End Class

End Namespace


