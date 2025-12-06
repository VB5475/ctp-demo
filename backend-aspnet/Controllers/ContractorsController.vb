Imports Microsoft.AspNetCore.Mvc

Namespace CTPBackend.Controllers

    <ApiController>
    <Route("api/[controller]")>
    Public Class ContractorsController
        Inherits ControllerBase

        Private ReadOnly _contractorService As CTPBackend.Services.IContractorService

        Public Sub New(contractorService As CTPBackend.Services.IContractorService)
            _contractorService = contractorService
        End Sub

        <HttpPost("register")>
        Public Function Register(<FromBody> request As RegisterContractorRequest) As ActionResult
            Try
                If String.IsNullOrEmpty(request.ContractorName) OrElse
                   String.IsNullOrEmpty(request.Email) OrElse
                   String.IsNullOrEmpty(request.Phone) Then
                    Return BadRequest(New CTPBackend.Models.ApiResponse With {
                        .Success = False,
                        .Message = "contractorName, email, and phone are required"
                    })
                End If

                Dim contractor = _contractorService.RegisterContractor(
                    request.ContractorName,
                    request.Email,
                    request.Phone,
                    request.WorkType,
                    request.Address
                )

                Return Ok(New CTPBackend.Models.ContractorResponse With {
                    .Success = True,
                    .ContractorId = contractor.ContractorId,
                    .RegistrationFee = contractor.RegistrationFee,
                    .Message = "Registration saved. Please complete payment."
                })

            Catch ex As Exception
                Console.WriteLine($"❌ Registration error: {ex.Message}")
                Return StatusCode(500, New CTPBackend.Models.ApiResponse With {
                    .Success = False,
                    .Message = "Internal server error"
                })
            End Try
        End Function

        <HttpGet("{contractorId}")>
        Public Function GetContractor(contractorId As String) As ActionResult
            Dim contractor = _contractorService.GetContractor(contractorId)

            If contractor Is Nothing Then
                Return NotFound(New CTPBackend.Models.ApiResponse With {
                    .Success = False,
                    .Message = "Contractor not found"
                })
            End If

            Return Ok(New CTPBackend.Models.ContractorResponse With {
                .Success = True,
                .Contractor = contractor
            })
        End Function

        Public Class RegisterContractorRequest
            Public Property ContractorName As String
            Public Property Email As String
            Public Property Phone As String
            Public Property WorkType As String
            Public Property Address As String
        End Class
    End Class

End Namespace

