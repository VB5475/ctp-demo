import { useState, useEffect } from "react";
import { api } from "../services/api";

export default function PaymentCallback({ orderId, initialStatus }) {
  const [status, setStatus] = useState("checking");
  const [message, setMessage] = useState("");
  const [paymentData, setPaymentData] = useState(null);
  const [contractor, setContractor] = useState(null);

  useEffect(() => {
    verifyPayment();
  }, [orderId]);

  const verifyPayment = async () => {
    try {
      const data = await api.verifyPayment(orderId);

      if (data.status === "SUCCESS") {
        setStatus("success");
        setMessage(
          "Payment successful! Your contractor registration is now complete."
        );
        setPaymentData(data.payment);
        setContractor(data.contractor);

        // Redirect to dashboard after 5 seconds
        setTimeout(() => {
          alert("In a real app, you'd be redirected to your dashboard now!");
          window.location.href = "/";
        }, 5000);
      } else if (data.status === "FAILED") {
        setStatus("failed");
        setMessage(`Payment failed: ${data.reason || "Unknown error"}`);
        setPaymentData(data.payment);
      } else if (data.status === "PENDING") {
        setStatus("pending");
        setMessage("Payment is being processed. Please wait...");

        // Retry verification after 3 seconds
        setTimeout(verifyPayment, 3000);
      }
    } catch (error) {
      setStatus("error");
      setMessage("Unable to verify payment status. Please contact support.");
      console.error(error);
    }
  };

  const retryPayment = async () => {
    try {
      const data = await api.retryPayment(orderId);

      if (data.success) {
        window.location.href = data.redirectUrl;
      } else {
        alert(data.message || "Failed to retry payment");
      }
    } catch (error) {
      alert("Error retrying payment. Please try again.");
    }
  };

  return (
    <div className="callback-container">
      <div className="card callback-card">
        {status === "checking" && (
          <div>
            <div className="spinner"></div>
            <h2 className="callback-title">Verifying Payment...</h2>
            <p className="callback-message">
              Please wait while we confirm your payment status.
            </p>
          </div>
        )}

        {status === "success" && (
          <div>
            <div className="callback-icon">✅</div>
            <h2 className="callback-title">Payment Successful!</h2>
            <p className="callback-message">{message}</p>

            {contractor && (
              <div
                className="success-box"
                style={{ textAlign: "left", marginTop: "20px" }}
              >
                <strong>Registration Complete:</strong>
                <br />
                Contractor: {contractor.contractorName}
                <br />
                Email: {contractor.email}
                <br />
                Status: <span style={{ color: "#22c55e" }}>ACTIVE</span>
                <br />
                <br />
                <small>Redirecting to dashboard in 5 seconds...</small>
              </div>
            )}

            <div
              className="btn-row"
              style={{ justifyContent: "center", marginTop: "24px" }}
            >
              <button
                className="btn btn-primary"
                onClick={() => (window.location.href = "/")}
              >
                Go to Dashboard
              </button>
            </div>
          </div>
        )}

        {status === "failed" && (
          <div>
            <div className="callback-icon">❌</div>
            <h2 className="callback-title">Payment Failed</h2>
            <p className="callback-message">{message}</p>

            <div
              className="error-box"
              style={{ textAlign: "left", marginTop: "20px" }}
            >
              <strong>What happened?</strong>
              <br />
              Your payment could not be processed. This might be due to insufficient
              funds, incorrect details, or a network issue.
            </div>

            <div
              className="btn-row"
              style={{ justifyContent: "center", marginTop: "24px" }}
            >
              <button className="btn btn-primary" onClick={retryPayment}>
                🔄 Retry Payment
              </button>
              <button
                className="btn btn-secondary"
                onClick={() => (window.location.href = "/")}
              >
                Back to Home
              </button>
            </div>

            <p
              style={{ fontSize: "0.85rem", color: "#6b7280", marginTop: "16px" }}
            >
              Your registration is saved. You can retry payment anytime.
              <br />
              Order ID: <code>{orderId}</code>
            </p>
          </div>
        )}

        {status === "pending" && (
          <div>
            <div className="spinner"></div>
            <h2 className="callback-title">Payment Processing...</h2>
            <p className="callback-message">{message}</p>
            <p style={{ fontSize: "0.85rem", color: "#6b7280" }}>
              This usually takes a few seconds. Please don't close this page.
            </p>
          </div>
        )}

        {status === "error" && (
          <div>
            <div className="callback-icon">⚠️</div>
            <h2 className="callback-title">Unable to Verify</h2>
            <p className="callback-message">{message}</p>

            <div
              className="btn-row"
              style={{ justifyContent: "center", marginTop: "24px" }}
            >
              <button className="btn btn-primary" onClick={verifyPayment}>
                Try Again
              </button>
              <button
                className="btn btn-secondary"
                onClick={() => (window.location.href = "/")}
              >
                Contact Support
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}


