const API_BASE = import.meta.env.VITE_API_BASE_URL || "http://localhost:4000/api";

export const api = {
  // Contractor endpoints
  registerContractor: async (data) => {
    const res = await fetch(`${API_BASE}/contractors/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
    return res.json();
  },

  getContractor: async (contractorId) => {
    const res = await fetch(`${API_BASE}/contractors/${contractorId}`);
    return res.json();
  },

  // Payment endpoints
  initiatePayment: async (contractorId, amount) => {
    const res = await fetch(`${API_BASE}/payments/initiate`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ contractorId, amount }),
    });
    return res.json();
  },

  verifyPayment: async (orderId) => {
    const res = await fetch(`${API_BASE}/payments/verify/${orderId}`);
    return res.json();
  },

  retryPayment: async (orderId) => {
    const res = await fetch(`${API_BASE}/payments/retry/${orderId}`, {
      method: "POST",
    });
    return res.json();
  },
};


