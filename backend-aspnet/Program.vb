Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Hosting
Imports Microsoft.Extensions.Configuration
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Hosting

Module Program
    Sub Main(args As String())
        Dim builder = WebApplication.CreateBuilder(args)

        ' Add services to the container
        builder.Services.AddControllers()
        builder.Services.AddEndpointsApiExplorer()
        builder.Services.AddSwaggerGen()

        ' Enable CORS
        builder.Services.AddCors(Sub(options)
            options.AddDefaultPolicy(Sub(policy)
                policy.AllowAnyOrigin()
                policy.AllowAnyMethod()
                policy.AllowAnyHeader()
            End Sub)
        End Sub)

        ' Register services
        builder.Services.AddSingleton(Of CTPBackend.Services.IContractorService, CTPBackend.Services.ContractorService)()
        builder.Services.AddSingleton(Of CTPBackend.Services.IPaymentService, CTPBackend.Services.PaymentService)()
        builder.Services.AddSingleton(Of CTPBackend.Services.ICtpService)(Function(provider) New CTPBackend.Services.CtpService(builder.Configuration))
        builder.Services.AddSingleton(Of CTPBackend.Services.IEmailService, CTPBackend.Services.EmailService)()

        Dim app = builder.Build()

        ' Configure the HTTP request pipeline
        If app.Environment.IsDevelopment() Then
            app.UseSwagger()
            app.UseSwaggerUI()
        End If

        app.UseHttpsRedirection()
        app.UseCors()
        app.UseAuthorization()
        app.MapControllers()

        Dim envPort = Environment.GetEnvironmentVariable("PORT")
        Dim configPort = builder.Configuration("Port")
        Dim port = If(Not String.IsNullOrEmpty(envPort), envPort, If(Not String.IsNullOrEmpty(configPort), configPort, "4000"))
        Console.WriteLine($"🚀 CTP Backend running on http://localhost:{port}")
        Console.WriteLine($"📝 Environment: {If(Environment.GetEnvironmentVariable("USE_REAL_CTP") = "true", "PRODUCTION (Real CTP)", "DEVELOPMENT (Mock CTP)")}")

        app.Run($"http://localhost:{port}")
    End Sub
End Module

