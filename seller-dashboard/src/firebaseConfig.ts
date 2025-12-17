// Firebase SDK imports
import { initializeApp } from "firebase/app";
import { getAuth } from "firebase/auth";

// Your web app's Firebase configuration from environment variables
const firebaseConfig = {
  apiKey: import.meta.env.VITE_FIREBASE_API_KEY || "AIzaSyBfgK6PRKXR21Vx-iQbQPqlI-zWcz_6xhk",
  authDomain: import.meta.env.VITE_FIREBASE_AUTH_DOMAIN || "ecommercedashboard-d890f.firebaseapp.com",
  projectId: import.meta.env.VITE_FIREBASE_PROJECT_ID || "ecommercedashboard-d890f",
  storageBucket: import.meta.env.VITE_FIREBASE_STORAGE_BUCKET || "ecommercedashboard-d890f.firebasestorage.app",
  messagingSenderId: import.meta.env.VITE_FIREBASE_MESSAGING_SENDER_ID || "277694336887",
  appId: import.meta.env.VITE_FIREBASE_APP_ID || "1:277694336887:web:ac448c3e1e5fa1b64b5b72",
  measurementId: import.meta.env.VITE_FIREBASE_MEASUREMENT_ID || "G-07FEK5NF80"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);

// Initialize Firebase Authentication
export const auth = getAuth(app);

export default app;
