// Firebase SDK imports
import { initializeApp } from "firebase/app";
import { getAuth } from "firebase/auth";

// Your web app's Firebase configuration
const firebaseConfig = {
  apiKey: "AIzaSyBfgK6PRKXR21Vx-iQbQPqlI-zWcz_6xhk",
  authDomain: "ecommercedashboard-d890f.firebaseapp.com",
  projectId: "ecommercedashboard-d890f",
  storageBucket: "ecommercedashboard-d890f.firebasestorage.app",
  messagingSenderId: "277694336887",
  appId: "1:277694336887:web:ac448c3e1e5fa1b64b5b72",
  measurementId: "G-07FEK5NF80"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);

// Initialize Firebase Authentication
export const auth = getAuth(app);

export default app;
