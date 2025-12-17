import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Box, TextField, Button, Typography, Container, Alert, Paper } from '@mui/material';
import { Lock } from '@mui/icons-material';
import { signInWithEmailAndPassword } from 'firebase/auth';
import { auth } from '../firebaseConfig';
import { api } from '../services/apiConfig';
import { useToast } from '../contexts/ToastContext';

export default function LoginPage() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    // const [tabValue, setTabValue] = useState(0);
    const navigate = useNavigate();
    const { showToast } = useToast();

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            const userCredential = await signInWithEmailAndPassword(auth, email, password);
            const token = await userCredential.user.getIdToken();
            await api.post('/Auth/session', { idToken: token });
            showToast('Giriş başarılı', 'success');
            navigate('/');
        } catch (err: any) {
            console.error('Login error:', err);
            let message = 'Giriş başarısız. Lütfen bilgilerinizi kontrol edin.';
            if (err.code === 'auth/user-not-found' || err.code === 'auth/invalid-credential') {
                message = 'Bu e-posta ile kayıtlı bir kullanıcı bulunamadı veya şifre yanlış.';
            } else if (err.code === 'auth/wrong-password') {
                message = 'Şifre yanlış. Lütfen tekrar deneyin.';
            } else if (err.code === 'auth/invalid-email') {
                message = 'Geçersiz e-posta adresi.';
            } else if (err.code === 'auth/too-many-requests') {
                message = 'Çok fazla deneme yapıldı. Lütfen daha sonra tekrar deneyin.';
            } else if (err.message) {
                message = err.message;
            }
            setError(message);
            showToast(message, 'error');
        } finally {
            setLoading(false);
        }
    };



    return (
        <Box 
            sx={{ 
                minHeight: '100vh', 
                display: 'flex', 
                alignItems: 'center', 
                justifyContent: 'center',
                bgcolor: 'background.default'
            }}
        >
        <Container component="main" maxWidth="xs">
            <Box
                sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                }}
            >
                <Paper elevation={3} sx={{ p: 4, width: '100%', borderRadius: 4 }}>
                    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                        <Box sx={{ p: 2, bgcolor: 'primary.light', borderRadius: '50%', mb: 2, color: 'white' }}>
                            <Lock sx={{ fontSize: 32 }} />
                        </Box>
                        <Typography component="h1" variant="h5" gutterBottom fontWeight="bold">
                            Dashboard
                        </Typography>



                        {error && (
                            <Alert severity="error" sx={{ width: '100%', mb: 2 }}>
                                {error}
                            </Alert>
                        )}

                        <Box component="form" onSubmit={handleLogin} sx={{ width: '100%' }}>
                            <TextField
                                margin="normal"
                                required
                                fullWidth
                                id="email"
                                label="E-posta Adresi"
                                name="email"
                                autoComplete="email"
                                autoFocus
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                disabled={loading}
                            />
                            <TextField
                                margin="normal"
                                required
                                fullWidth
                                name="password"
                                label="Şifre"
                                type="password"
                                id="password"
                                autoComplete="current-password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                disabled={loading}
                            />
                            <Button
                                type="submit"
                                fullWidth
                                variant="contained"
                                sx={{ mt: 3, mb: 2 }}
                                disabled={loading}
                            >
                                {loading ? 'İşlem yapılıyor...' : 'Giriş Yap'}
                            </Button>
                        </Box>
                    </Box>
                </Paper>
            </Box>
        </Container>
        </Box>
    );
}