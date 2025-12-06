import { useState, useEffect } from "react";
import RegistrationForm from "./components/RegistrationForm";
import PaymentCallback from "./components/PaymentCallback";
import "./styles/index.css";

function App() {
  const [currentView, setCurrentView] = useState("register"); // register | callback
  const [callbackParams, setCallbackParams] = useState({});

  useEffect(() => {
    // Check if we're on callback page
    const params = new URLSearchParams(window.location.search);
    const orderId = params.get("orderId");
    const status = params.get("status");

    if (orderId) {
      setCallbackParams({ orderId, status });
      setCurrentView("callback");
    }
  }, []);

  if (currentView === "callback") {
    return (
      <PaymentCallback
        orderId={callbackParams.orderId}
        initialStatus={callbackParams.status}
      />
    );
  }

  return <RegistrationForm />;
}

export default App;


