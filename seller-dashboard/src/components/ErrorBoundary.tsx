import { Component } from 'react';
import type { ErrorInfo, ReactNode } from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import { ErrorOutline as ErrorIcon } from '@mui/icons-material';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
  errorInfo: ErrorInfo | null;
}

export class ErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false,
    error: null,
    errorInfo: null,
  };

  public static getDerivedStateFromError(error: Error): Partial<State> {
    return { hasError: true, error };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('ErrorBoundary caught an error:', error, errorInfo);
    this.setState({ errorInfo });
    
    // Burada hata raporlama servisi çağrılabilir (örn: Sentry, LogRocket)
    // reportError(error, errorInfo);
  }

  private handleRetry = () => {
    this.setState({ hasError: false, error: null, errorInfo: null });
  };

  private handleGoHome = () => {
    window.location.href = '/';
  };

  public render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            minHeight: '100vh',
            p: 3,
            bgcolor: 'background.default',
          }}
        >
          <Paper
            elevation={3}
            sx={{
              p: 4,
              maxWidth: 500,
              textAlign: 'center',
              borderRadius: 3,
            }}
          >
            <ErrorIcon sx={{ fontSize: 80, color: 'error.main', mb: 2 }} />
            <Typography variant="h5" gutterBottom fontWeight="bold">
              Bir Hata Oluştu
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
              Üzgünüz, beklenmeyen bir hata meydana geldi. Lütfen sayfayı yenilemeyi
              deneyin veya ana sayfaya dönün.
            </Typography>
            
            {import.meta.env.MODE === 'development' && this.state.error && (
              <Paper
                sx={{
                  p: 2,
                  mb: 3,
                  bgcolor: 'grey.100',
                  textAlign: 'left',
                  maxHeight: 200,
                  overflow: 'auto',
                }}
              >
                <Typography variant="caption" component="pre" sx={{ fontFamily: 'monospace' }}>
                  {this.state.error.toString()}
                  {this.state.errorInfo?.componentStack}
                </Typography>
              </Paper>
            )}

            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center' }}>
              <Button variant="outlined" onClick={this.handleRetry}>
                Tekrar Dene
              </Button>
              <Button variant="contained" onClick={this.handleGoHome}>
                Ana Sayfaya Dön
              </Button>
            </Box>
          </Paper>
        </Box>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
