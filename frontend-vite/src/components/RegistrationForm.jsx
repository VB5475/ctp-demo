import { useState } from "react";
import { api } from "../services/api";

export default function RegistrationForm() {
  const [step, setStep] = useState(1); // 1: Register, 2: Payment
  const [form, setForm] = useState({
    contractorName: "",
    email: "",
    phone: "",
    workType: "Road Cutting",
    address: "",
  });
  const [contractorId, setContractorId] = useState("");
  const [registrationFee, setRegistrationFee] = useState(1500);
  const [loading, setLoading] = useState(false);
  const [log, setLog] = useState("// System logs will appear here\n");

  const appendLog = (line) => {
    setLog(
      (prev) => prev + new Date().toLocaleTimeString() + " - " + line + "\n"
    );
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      appendLog("Registering contractor...");

      const data = await api.registerContractor(form);
      appendLog(`Response: ${JSON.stringify(data)}`);

      if (!data.success) {
        alert(data.message || "Registration failed");
        return;
      }

      setContractorId(data.contractorId);
      setRegistrationFee(data.registrationFee);
      appendLog(`✅ Contractor registered: ${data.contractorId}`);
      setStep(2);
    } catch (err) {
      console.error(err);
      appendLog(`❌ Error: ${err.message}`);
      alert("Error during registration. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const initiatePayment = async () => {
    setLoading(true);

    try {
      appendLog("Initiating CTP payment...");

      const data = await api.initiatePayment(contractorId, registrationFee);
      appendLog(`Payment response: ${JSON.stringify(data)}`);

      if (!data.success) {
        alert(data.message || "Failed to initiate payment");
        return;
      }

      appendLog(`✅ Redirecting to CTP payment gateway...`);

      // Open CTP payment page in a new tab to preserve current page state
      window.open(data.redirectUrl, "_blank", "noopener,noreferrer");
    } catch (err) {
      console.error(err);
      appendLog(`❌ Error: ${err.message}`);
      alert("Error initiating payment. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app-container">
      <div className="card">
        <h1>🏗️ Contractor Registration System</h1>
        <div className="subtitle">
          Complete your contractor registration with secure CTP payment gateway
          integration.
        </div>

        {step === 1 && (
          <div>
            <h2>Step 1: Registration Details</h2>
            <form onSubmit={handleRegister}>
              <div className="grid">
                <div className="form-group">
                  <label>Contractor/Company Name *</label>
                  <input
                    name="contractorName"
                    value={form.contractorName}
                    onChange={handleChange}
                    placeholder="ABC Infra Pvt Ltd"
                    required
                  />
                </div>

                <div className="form-group">
                  <label>Contact Email *</label>
                  <input
                    type="email"
                    name="email"
                    value={form.email}
                    onChange={handleChange}
                    placeholder="contact@contractor.com"
                    required
                  />
                </div>

                <div className="form-group">
                  <label>Phone Number *</label>
                  <input
                    type="tel"
                    name="phone"
                    value={form.phone}
                    onChange={handleChange}
                    placeholder="9876543210"
                    pattern="[0-9]{10}"
                    required
                  />
                </div>

                <div className="form-group">
                  <label>Work Type *</label>
                  <select
                    name="workType"
                    value={form.workType}
                    onChange={handleChange}
                  >
                    <option>Road Cutting</option>
                    <option>ROW Permit</option>
                    <option>Cable Laying</option>
                    <option>Other Works</option>
                  </select>
                </div>
              </div>

              <div className="form-group">
                <label>Business Address</label>
                <textarea
                  name="address"
                  value={form.address}
                  onChange={handleChange}
                  placeholder="Enter your business address"
                />
              </div>

              <div className="info-box">
                <strong>ℹ️ Registration Fee:</strong> Rs. 1,500 (One-time payment)
              </div>

              <div className="btn-row">
                <button className="btn btn-primary" type="submit" disabled={loading}>
                  {loading ? "Processing..." : "Continue to Payment →"}
                </button>
              </div>
            </form>
          </div>
        )}

        {step === 2 && (
          <div>
            <h2>Step 2: Payment</h2>

            <div className="success-box">
              <strong>✅ Registration Details Saved</strong>
              <br />
              Contractor ID: <code>{contractorId}</code>
            </div>

            <div className="info-box">
              <strong>Payment Details:</strong>
              <br />
              Amount: <strong>Rs. {registrationFee}</strong>
              <br />
              Contractor: <strong>{form.contractorName}</strong>
              <br />
              Email: <strong>{form.email}</strong>
            </div>

            <p style={{ color: "#9ca3af", fontSize: "0.9rem", marginTop: "16px" }}>
              You will be redirected to the CTP payment gateway to complete your
              registration payment securely.
            </p>

            <div className="btn-row">
              <button
                className="btn btn-primary"
                onClick={initiatePayment}
                disabled={loading}
              >
                {loading ? "Redirecting..." : "Proceed to CTP Payment 🔒"}
              </button>

              <button
                className="btn btn-secondary"
                onClick={() => setStep(1)}
                disabled={loading}
              >
                ← Back to Edit Details
              </button>
            </div>
          </div>
        )}

        <h2>Debug Log</h2>
        <div className="log-container">{log}</div>
      </div>
    </div>
  );
}


