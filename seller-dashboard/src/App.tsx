import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { CssBaseline, ThemeProvider, Box, CircularProgress } from '@mui/material';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import ProductsPage from './pages/ProductsPage';
import ExcelPage from './pages/ExcelPage';
import OrdersPage from './pages/OrdersPage';
import ShippingPage from './pages/ShippingPage';
import LoginPage from './pages/LoginPage';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { ToastProvider } from './contexts/ToastContext';
import { theme } from './theme';

// Auth kontrolü için özel Route componenti
const PrivateRoute = ({ children }: { children: React.ReactNode }) => {
  const { user, loading } = useAuth();
  
  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }
  
  return user ? <>{children}</> : <Navigate to="/login" />;
};

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <ToastProvider>
        <AuthProvider>
          <Router>
            <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route
              path="/"
              element={
                <PrivateRoute>
                  <Layout>
                    <Dashboard />
                  </Layout>
                </PrivateRoute>
              }
            />
            <Route
              path="/products"
              element={
                <PrivateRoute>
                  <Layout>
                    <ProductsPage />
                  </Layout>
                </PrivateRoute>
              }
            />
            <Route
              path="/excel"
              element={
                <PrivateRoute>
                  <Layout>
                    <ExcelPage />
                  </Layout>
                </PrivateRoute>
              }
            />
            <Route
              path="/orders"
              element={
                <PrivateRoute>
                  <Layout>
                    <OrdersPage />
                  </Layout>
                </PrivateRoute>
              }
            />
            <Route
              path="/shipping"
              element={
                <PrivateRoute>
                  <Layout>
                    <ShippingPage />
                  </Layout>
                </PrivateRoute>
              }
            />
          </Routes>
        </Router>
        </AuthProvider>
      </ToastProvider>
    </ThemeProvider>
  );
}

export default App;
