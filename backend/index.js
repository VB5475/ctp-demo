const express = require("express");
const cors = require("cors");
const crypto = require("crypto");
const axios = require("axios");
require("dotenv").config();

const app = express();
const PORT = process.env.PORT || 4000;

app.use(cors());
app.use(express.json());

// ============================================
// IN-MEMORY DATABASE (Replace with MongoDB/PostgreSQL)
// ============================================
const contractors = {}; // contractorId -> contractor data
const payments = {}; // orderId -> payment data

// ============================================
// UTILITY FUNCTIONS
// ============================================

// Generate signature for CTP API requests
function generateCtpSignature(payload) {
  const secret = process.env.CTP_API_SECRET;

  // CTP will specify exact format - example:
  const dataString = `${payload.merchantId}|${payload.orderId}|${payload.amount}|${payload.timestamp}`;

  const signature = crypto
    .createHmac("sha256", secret)
    .update(dataString)
    .digest("hex");

  return signature;
}

// Verify webhook signature from CTP
function verifyWebhookSignature(payload, receivedSignature) {
  const secret = process.env.CTP_WEBHOOK_SECRET;

  // CTP will specify exact format - example:
  const dataString = `${payload.transactionId}|${payload.orderId}|${payload.status}|${payload.amount}`;

  const calculatedSignature = crypto
    .createHmac("sha256", secret)
    .update(dataString)
    .digest("hex");

  return calculatedSignature === receivedSignature;
}

// Send email (placeholder - integrate with SendGrid/Nodemailer)
async function sendEmail(to, subject, body) {
  console.log(`📧 EMAIL SENT TO: ${to}`);
  console.log(`Subject: ${subject}`);
  console.log(`Body: ${body}`);
  // TODO: Implement actual email sending
}

// ============================================
// API ENDPOINTS
// ============================================

// 1. CONTRACTOR REGISTRATION (Step 1)
app.post("/api/contractors/register", async (req, res) => {
  try {
    const { contractorName, email, phone, workType, address } = req.body;

    if (!contractorName || !email || !phone) {
      return res.status(400).json({
        success: false,
        message: "contractorName, email, and phone are required"
      });
    }

    // Generate unique contractor ID
    const contractorId = `CONT-${Date.now()}`;

    // Save contractor data (status: PENDING_PAYMENT)
    contractors[contractorId] = {
      contractorId,
      contractorName,
      email,
      phone,
      workType: workType || "Road Cutting",
      address: address || "",
      status: "PENDING_PAYMENT",
      paymentStatus: "UNPAID",
      registrationFee: 1500, // Fixed or dynamic
      createdAt: new Date().toISOString()
    };

    console.log(`✅ Contractor registered: ${contractorId}`);

    res.json({
      success: true,
      contractorId,
      registrationFee: 1500,
      message: "Registration saved. Please complete payment."
    });

  } catch (error) {
    console.error("❌ Registration error:", error);
    res.status(500).json({
      success: false,
      message: "Internal server error"
    });
  }
});

// 2. INITIATE PAYMENT (Step 2)
app.post("/api/payments/initiate", async (req, res) => {
  try {
    const { contractorId, amount } = req.body;

    // Validate contractor exists
    const contractor = contractors[contractorId];
    if (!contractor) {
      return res.status(404).json({
        success: false,
        message: "Contractor not found"
      });
    }

    // Check if already paid
    if (contractor.paymentStatus === "PAID") {
      return res.status(400).json({
        success: false,
        message: "Payment already completed"
      });
    }

    // Generate unique order ID
    const orderId = `ORD-${Date.now()}-${contractorId}`;
    const timestamp = Date.now();

    // Prepare payload for CTP
    const ctpPayload = {
      merchantId: process.env.CTP_MERCHANT_ID,
      orderId: orderId,
      amount: amount,
      currency: "INR",
      customerName: contractor.contractorName,
      customerEmail: contractor.email,
      customerPhone: contractor.phone,
      returnUrl: `${process.env.APP_URL}/payment/callback`,
      webhookUrl: `${process.env.APP_URL}/api/payments/webhook`,
      timestamp: timestamp,
      metadata: {
        contractorId: contractorId,
        workType: contractor.workType
      }
    };

    // Generate signature
    const signature = generateCtpSignature(ctpPayload);

    console.log(`🔄 Initiating CTP payment for ${contractorId}...`);

    // TODO: REPLACE WITH REAL CTP API CALL
    // For demo, we simulate CTP response
    let ctpResponse;

    if (process.env.USE_REAL_CTP === "true") {
      // REAL CTP API CALL
      ctpResponse = await axios.post(
        `${process.env.CTP_API_BASE_URL}/payments/create`,
        ctpPayload,
        {
          headers: {
            "Authorization": `Bearer ${process.env.CTP_API_KEY}`,
            "Content-Type": "application/json",
            "X-Signature": signature
          }
        }
      );
    } else {
      // MOCK RESPONSE for testing
      ctpResponse = {
        data: {
          success: true,
          transactionId: `CTP-TXN-${Date.now()}`,
          redirectUrl: `http://localhost:4000/mock-ctp-payment?orderId=${orderId}&txnId=CTP-TXN-${Date.now()}`
        }
      };
    }

    // Save payment record
    payments[orderId] = {
      orderId,
      contractorId,
      transactionId: ctpResponse.data.transactionId,
      amount,
      currency: "INR",
      status: "PENDING",
      ctpRedirectUrl: ctpResponse.data.redirectUrl,
      createdAt: new Date().toISOString(),
      retryCount: 0
    };

    console.log(`✅ Payment initiated: ${orderId}`);

    res.json({
      success: true,
      orderId,
      transactionId: ctpResponse.data.transactionId,
      redirectUrl: ctpResponse.data.redirectUrl
    });

  } catch (error) {
    console.error("❌ Payment initiation error:", error.message);
    res.status(500).json({
      success: false,
      message: "Failed to initiate payment. Please try again."
    });
  }
});

// 3. WEBHOOK HANDLER (CTP calls this)
app.post("/api/payments/webhook", async (req, res) => {
  try {
    const { transactionId, orderId, status, amount, signature, failureReason } = req.body;

    console.log(`🔔 Webhook received for ${orderId}: ${status}`);

    // CRITICAL: Verify signature
    const isValid = verifyWebhookSignature(req.body, signature);

    if (!isValid) {
      console.error("⚠️ INVALID WEBHOOK SIGNATURE!");
      return res.status(401).json({
        success: false,
        message: "Invalid signature"
      });
    }

    // Find payment
    const payment = payments[orderId];
    if (!payment) {
      console.error(`❌ Payment not found: ${orderId}`);
      return res.status(404).json({
        success: false,
        message: "Payment not found"
      });
    }

    // Prevent duplicate processing
    if (payment.status === status) {
      console.log(`⚠️ Duplicate webhook ignored: ${orderId}`);
      return res.json({ success: true, message: "Already processed" });
    }

    // Update payment status
    payment.status = status;
    payment.updatedAt = new Date().toISOString();
    payment.webhookReceivedAt = new Date().toISOString();

    if (status === "FAILED") {
      payment.failureReason = failureReason || "Unknown error";
    }

    // Get contractor
    const contractor = contractors[payment.contractorId];

    if (!contractor) {
      console.error(`❌ Contractor not found: ${payment.contractorId}`);
      return res.status(404).json({
        success: false,
        message: "Contractor not found"
      });
    }

    // Handle SUCCESS
    if (status === "SUCCESS") {
      contractor.status = "ACTIVE";
      contractor.paymentStatus = "PAID";
      contractor.registrationCompletedAt = new Date().toISOString();

      console.log(`✅ Contractor activated: ${contractor.contractorId}`);

      // Send success email
      await sendEmail(
        contractor.email,
        "Registration Successful!",
        `Hello ${contractor.contractorName},\n\nYour contractor registration is now complete!\n\nPayment Amount: Rs. ${amount}\nTransaction ID: ${transactionId}\n\nYou can now access your dashboard.\n\nThank you!`
      );
    }

    // Handle FAILED
    if (status === "FAILED") {
      contractor.status = "PAYMENT_FAILED";
      contractor.paymentStatus = "FAILED";

      console.log(`❌ Payment failed for: ${contractor.contractorId}`);

      // Send failure email with retry link
      await sendEmail(
        contractor.email,
        "Payment Failed - Please Retry",
        `Hello ${contractor.contractorName},\n\nYour payment of Rs. ${amount} failed.\n\nReason: ${failureReason || "Unknown error"}\n\nPlease retry payment to complete registration:\n${process.env.APP_URL}/contractor/retry-payment/${orderId}\n\nIf issue persists, contact support.`
      );
    }

    // ALWAYS respond 200 OK (so CTP knows we received it)
    res.json({ success: true });

  } catch (error) {
    console.error("❌ Webhook error:", error);
    // Still return 200 to prevent retries
    res.json({ success: true, error: error.message });
  }
});

// 4. VERIFY PAYMENT STATUS (User returns from CTP)
app.get("/api/payments/verify/:orderId", async (req, res) => {
  try {
    const { orderId } = req.params;

    const payment = payments[orderId];

    if (!payment) {
      return res.status(404).json({
        success: false,
        status: "NOT_FOUND",
        message: "Payment not found"
      });
    }

    // If webhook already updated, return that
    if (payment.status === "SUCCESS") {
      const contractor = contractors[payment.contractorId];
      return res.json({
        success: true,
        status: "SUCCESS",
        payment,
        contractor
      });
    }

    if (payment.status === "FAILED") {
      return res.json({
        success: false,
        status: "FAILED",
        reason: payment.failureReason,
        payment
      });
    }

    // If still PENDING, check with CTP API (backup)
    console.log(`🔄 Checking CTP status for ${orderId}...`);

    try {
      // TODO: REPLACE WITH REAL CTP STATUS API
      let ctpStatus;

      if (process.env.USE_REAL_CTP === "true") {
        ctpStatus = await axios.get(
          `${process.env.CTP_API_BASE_URL}/payments/status/${payment.transactionId}`,
          {
            headers: {
              "Authorization": `Bearer ${process.env.CTP_API_KEY}`
            }
          }
        );
      } else {
        // Mock: Auto-success after 10 seconds for testing
        const createdTime = new Date(payment.createdAt).getTime();
        const now = Date.now();
        const mockStatus = (now - createdTime > 10000) ? "SUCCESS" : "PENDING";

        ctpStatus = {
          data: {
            status: mockStatus,
            transactionId: payment.transactionId
          }
        };
      }

      // Update local status
      payment.status = ctpStatus.data.status;
      payment.updatedAt = new Date().toISOString();

      if (payment.status === "SUCCESS") {
        const contractor = contractors[payment.contractorId];
        contractor.status = "ACTIVE";
        contractor.paymentStatus = "PAID";
        contractor.registrationCompletedAt = new Date().toISOString();
      }

      return res.json({
        success: payment.status === "SUCCESS",
        status: payment.status,
        payment
      });

    } catch (error) {
      console.error("❌ CTP status check failed:", error.message);
      return res.json({
        success: false,
        status: "PENDING",
        message: "Payment is being processed"
      });
    }

  } catch (error) {
    console.error("❌ Verify error:", error);
    res.status(500).json({
      success: false,
      message: "Error verifying payment"
    });
  }
});

// 5. RETRY PAYMENT
app.post("/api/payments/retry/:orderId", async (req, res) => {
  try {
    const { orderId } = req.params;

    const payment = payments[orderId];

    if (!payment) {
      return res.status(404).json({
        success: false,
        message: "Payment not found"
      });
    }

    if (payment.status === "SUCCESS") {
      return res.status(400).json({
        success: false,
        message: "Payment already completed"
      });
    }

    const contractor = contractors[payment.contractorId];
    const timestamp = Date.now();

    // Create new payment request with CTP
    const ctpPayload = {
      merchantId: process.env.CTP_MERCHANT_ID,
      orderId: orderId, // Same order, new transaction
      amount: payment.amount,
      currency: "INR",
      customerName: contractor.contractorName,
      customerEmail: contractor.email,
      customerPhone: contractor.phone,
      returnUrl: `${process.env.APP_URL}/payment/callback`,
      webhookUrl: `${process.env.APP_URL}/api/payments/webhook`,
      timestamp: timestamp
    };

    const signature = generateCtpSignature(ctpPayload);

    let ctpResponse;

    if (process.env.USE_REAL_CTP === "true") {
      ctpResponse = await axios.post(
        `${process.env.CTP_API_BASE_URL}/payments/create`,
        ctpPayload,
        {
          headers: {
            "Authorization": `Bearer ${process.env.CTP_API_KEY}`,
            "Content-Type": "application/json",
            "X-Signature": signature
          }
        }
      );
    } else {
      ctpResponse = {
        data: {
          transactionId: `CTP-TXN-${Date.now()}`,
          redirectUrl: `http://localhost:4000/mock-ctp-payment?orderId=${orderId}&txnId=CTP-TXN-${Date.now()}`
        }
      };
    }

    // Update payment with new transaction ID
    payment.transactionId = ctpResponse.data.transactionId;
    payment.status = "PENDING";
    payment.retryCount = (payment.retryCount || 0) + 1;
    payment.updatedAt = new Date().toISOString();

    console.log(`🔄 Payment retry initiated: ${orderId}`);

    res.json({
      success: true,
      redirectUrl: ctpResponse.data.redirectUrl
    });

  } catch (error) {
    console.error("❌ Retry error:", error);
    res.status(500).json({
      success: false,
      message: "Failed to retry payment"
    });
  }
});

// 6. GET CONTRACTOR DETAILS
app.get("/api/contractors/:contractorId", (req, res) => {
  const { contractorId } = req.params;
  const contractor = contractors[contractorId];

  if (!contractor) {
    return res.status(404).json({
      success: false,
      message: "Contractor not found"
    });
  }

  res.json({
    success: true,
    contractor
  });
});

// ============================================
// MOCK CTP PAYMENT PAGE (for testing)
// ============================================
app.get("/mock-ctp-payment", (req, res) => {
  const { orderId, txnId } = req.query;

  res.send(`
    <!DOCTYPE html>
    <html>
    <head>
      <title>Mock CTP Payment Gateway</title>
      <style>
        body { font-family: Arial; max-width: 500px; margin: 50px auto; padding: 20px; }
        .btn { padding: 10px 20px; margin: 10px; cursor: pointer; border-radius: 5px; border: none; font-size: 16px; }
        .success { background: #22c55e; color: white; }
        .failed { background: #ef4444; color: white; }
      </style>
    </head>
    <body>
      <h2>🏦 Mock CTP Payment Gateway</h2>
      <p>Order ID: ${orderId}</p>
      <p>Transaction ID: ${txnId}</p>
      <p>Amount: Rs. 1500</p>
      <hr>
      <p>Simulate payment result:</p>
      <button class="btn success" onclick="simulatePayment('SUCCESS')">✅ Pay Successfully</button>
      <button class="btn failed" onclick="simulatePayment('FAILED')">❌ Fail Payment</button>
      
      <script>
        async function simulatePayment(status) {
          // Call webhook
          await fetch('http://localhost:4000/api/payments/webhook', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
              transactionId: '${txnId}',
              orderId: '${orderId}',
              status: status,
              amount: 1500,
              signature: 'mock-signature-for-testing',
              failureReason: status === 'FAILED' ? 'Insufficient funds' : null
            })
          });
          
          // Redirect back to app
          window.location.href = 'http://localhost:3000/payment/callback?orderId=${orderId}&status=' + status.toLowerCase();
        }
      </script>
    </body>
    </html>
  `);
});

// ============================================
// START SERVER
// ============================================
app.listen(PORT, () => {
  console.log(`🚀 CTP Backend running on http://localhost:${PORT}`);
  console.log(`📝 Environment: ${process.env.USE_REAL_CTP === 'true' ? 'PRODUCTION (Real CTP)' : 'DEVELOPMENT (Mock CTP)'}`);
});