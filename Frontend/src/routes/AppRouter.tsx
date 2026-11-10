import { BrowserRouter } from "react-router-dom";
import { AppGate, SessionExpiredHandler } from "./AppGate";

export default function AppRouter() {
  return (
    <BrowserRouter>
      <SessionExpiredHandler />
      <AppGate />
    </BrowserRouter>
  );
}
