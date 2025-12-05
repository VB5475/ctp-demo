const express = require("express");
const cors = require("cors");

const app = express();
const PORT = process.env.PORT || 4000;

app.use(cors());
app.use(express.json());

// In-memory store for demo purposes only
const payments = {};

// POST /api/payments/initiate
// Simulates creating a payment with CTP and returning a redirect URL.
app.post("/api/payments/initiate", (req, res) => {
  const { contractorName, email, amount, workType } = req.body || {};

  if (!contractorName || !email || !amount) {
    return res.status(400).json({
      success: false,
      message: "contractorName, email and amount are required"
    });
  }

  const transactionId = "CTP-TXN-" + Date.now();

  payments[transactionId] = {
    transactionId,
    contractorName,
    email,
    amount,
    workType: workType || "",
    status: "PENDING",
    createdAt: new Date().toISOString()
  };

  // In a real integration, this would come from CTP's API response
  const redirectUrl = `https://ctp-gateway.example.com/pay?txnId=${encodeURIComponent(
    transactionId
  )}`;

  res.json({
    success: true,
    transactionId,
    redirectUrl
  });
});

// GET /api/payments/status/:transactionId
// Simulates querying CTP for current payment status.
app.get("/api/payments/status/:transactionId", (req, res) => {
  const { transactionId } = req.params;
  const payment = payments[transactionId];

  if (!payment) {
    return res.status(404).json({
      success: false,
      message: "Transaction not found"
    });
  }

  // For demo: flip PENDING to SUCCESS after some time
  const createdTime = new Date(payment.createdAt).getTime();
  const now = Date.now();
  if (payment.status === "PENDING" && now - createdTime > 10000) {
    payment.status = "SUCCESS";
  }

  res.json({
    success: true,
    transactionId,
    status: payment.status,
    details: payment
  });
});

// POST /api/payments/webhook
// Simulated callback endpoint CTP would call after payment.
app.post("/api/payments/webhook", (req, res) => {
  const { transactionId, status } = req.body || {};

  if (!transactionId || !status) {
    return res.status(400).json({
      success: false,
      message: "transactionId and status are required"
    });
  }

  if (!payments[transactionId]) {
    payments[transactionId] = {
      transactionId,
      status,
      createdAt: new Date().toISOString()
    };
  } else {
    payments[transactionId].status = status;
  }

  // Always respond 200 so CTP considers callback delivered
  res.json({ success: true });
});

app.listen(PORT, () => {
  console.log(`CTP demo backend listening on http://localhost:${PORT}`);
});


