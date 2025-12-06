Imports Microsoft.AspNetCore.Mvc

Namespace CTPBackend.Controllers

    Public Class MockPaymentController
        Inherits ControllerBase

        <HttpGet("/mock-ctp-payment")>
        Public Function MockPaymentPage(<FromQuery> orderId As String, <FromQuery> txnId As String) As ContentResult
            Dim html = $"
<!DOCTYPE html>
<html>
<head>
    <title>Mock CTP Payment Gateway</title>
    <style>
        body {{ font-family: Arial; max-width: 500px; margin: 50px auto; padding: 20px; }}
        .btn {{ padding: 10px 20px; margin: 10px; cursor: pointer; border-radius: 5px; border: none; font-size: 16px; }}
        .success {{ background: #22c55e; color: white; }}
        .failed {{ background: #ef4444; color: white; }}
    </style>
</head>
<body>
    <h2> Mock CTP Payment Gateway</h2>
    <p>Order ID: {orderId}</p>
    <p>Transaction ID: {txnId}</p>
    <p>Amount: Rs. 1500</p>
    <hr>
    <p>Simulate payment result:</p>
    <button class=""btn success"" onclick=""simulatePayment('SUCCESS')""> Pay Successfully</button>
    <button class=""btn failed"" onclick=""simulatePayment('FAILED')""> Fail Payment</button>
    
    <script>
        async function simulatePayment(status) {{
            await fetch('http://localhost:4000/api/payments/webhook', {{
                method: 'POST',
                headers: {{ 'Content-Type': 'application/json' }},
                body: JSON.stringify({{
                    transactionId: '{txnId}',
                    orderId: '{orderId}',
                    status: status,
                    amount: 1500,
                    signature: 'mock-signature-for-testing',
                    failureReason: status === 'FAILED' ? 'Insufficient funds' : null
                }})
            }});
            
            window.location.href = 'http://localhost:4000/payment/callback?orderId={orderId}&status=' + status.toLowerCase();
        }}
    </script>
</body>
</html>"

            Return Content(html, "text/html")
        End Function

        <HttpGet("/payment/callback")>
        Public Function PaymentCallback(<FromQuery> orderId As String, <FromQuery> status As String) As ContentResult
            Dim statusLower = If(Not String.IsNullOrEmpty(status), status.ToLowerInvariant(), "")
            Dim isSuccess = statusLower = "success"
            Dim isFailed = statusLower = "failed"
            Dim icon = If(isSuccess, "✅", If(isFailed, "❌", "⏳"))
            Dim title = If(isSuccess, "Payment Successful!", If(isFailed, "Payment Failed", "Payment Processing..."))
            Dim message = If(isSuccess, "Your payment has been processed successfully. Your contractor registration is now complete!", If(isFailed, "Your payment could not be processed. Please try again.", "Please wait while we verify your payment..."))
            Dim bgColor = If(isSuccess, "#22c55e", If(isFailed, "#ef4444", "#3b82f6"))

            Dim html = $"
<!DOCTYPE html>
<html>
<head>
    <title>Payment Callback - CTP Gateway</title>
    <style>
        body {{ 
            font-family: Arial, sans-serif; 
            max-width: 600px; 
            margin: 50px auto; 
            padding: 20px;
            background: #f5f5f5;
        }}
        .card {{
            background: white;
            border-radius: 12px;
            padding: 40px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            text-align: center;
        }}
        .icon {{
            font-size: 64px;
            margin-bottom: 20px;
        }}
        h1 {{
            color: {bgColor};
            margin-bottom: 16px;
        }}
        .message {{
            color: #666;
            font-size: 16px;
            margin-bottom: 30px;
            line-height: 1.6;
        }}
        .order-info {{
            background: #f9fafb;
            padding: 16px;
            border-radius: 8px;
            margin: 20px 0;
            text-align: left;
        }}
        .order-info p {{
            margin: 8px 0;
            color: #333;
        }}
        .btn {{
            display: inline-block;
            padding: 12px 24px;
            background: {bgColor};
            color: white;
            text-decoration: none;
            border-radius: 6px;
            margin-top: 20px;
            font-weight: bold;
        }}
        .btn:hover {{
            opacity: 0.9;
        }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""icon"">{icon}</div>
        <h1>{title}</h1>
        <p class=""message"">{message}</p>
        
        <div class=""order-info"">
            <p><strong>Order ID:</strong> {orderId}</p>
            <p><strong>Status:</strong> {status.ToUpperInvariant()}</p>
        </div>
        
        <a href=""http://localhost:3000"" class=""btn"">Return to Home</a>
    </div>
    
    <script>
        // Auto-verify payment status after 2 seconds
        setTimeout(async function() {{
            try {{
                const response = await fetch(`http://localhost:4000/api/payments/verify/{orderId}`);
                const data = await response.json();
                console.log('Payment verification:', data);
            }} catch (error) {{
                console.error('Verification error:', error);
            }}
        }}, 2000);
    </script>
</body>
</html>"

            Return Content(html, "text/html")
        End Function
    End Class

End Namespace

