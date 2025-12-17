import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Box, TextField, Button, Typography, Container, Alert, Tabs, Tab, Paper } from '@mui/material';
import { Lock } from '@mui/icons-material';
import { signInWithEmailAndPassword, createUserWithEmailAndPassword } from 'firebase/auth';
import { auth } from '../firebaseConfig';
import { api } from '../services/apiConfig';
import { useToast } from '../contexts/ToastContext';

export default function LoginPage() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const [tabValue, setTabValue] = useState(0);
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
            setError(err.message || 'Giriş başarısız. Lütfen bilgilerinizi kontrol edin.');
            showToast('Giriş başarısız', 'error');
        } finally {
            setLoading(false);
        }
    };

    const handleRegister = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            const userCredential = await createUserWithEmailAndPassword(auth, email, password);
            const token = await userCredential.user.getIdToken();
            await api.post('/Auth/session', { idToken: token });
            
            showToast('Kayıt başarılı', 'success');
            navigate('/');
        } catch (err: any) {
            console.error('Register error:', err);
            setError(err.message || 'Kayıt başarısız. Lütfen bilgilerinizi kontrol edin.');
            showToast('Kayıt başarısız', 'error');
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
                            Satıcı Paneli
                        </Typography>

                        <Tabs value={tabValue} onChange={(_, newValue) => setTabValue(newValue)} sx={{ mb: 3, width: '100%' }}>
                            <Tab label="Giriş Yap" sx={{ flex: 1 }} />
                            <Tab label="Kayıt Ol" sx={{ flex: 1 }} />
                        </Tabs>

                        {error && (
                            <Alert severity="error" sx={{ width: '100%', mb: 2 }}>
                                {error}
                            </Alert>
                        )}

                        <Box component="form" onSubmit={tabValue === 0 ? handleLogin : handleRegister} sx={{ width: '100%' }}>
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
                                {loading ? 'İşlem yapılıyor...' : tabValue === 0 ? 'Giriş Yap' : 'Kayıt Ol'}
                            </Button>
                        </Box>
                    </Box>
                </Paper>
            </Box>
        </Container>
        </Box>
    );
}