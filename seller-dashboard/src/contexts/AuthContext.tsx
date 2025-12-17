import { createContext, useContext, useEffect, useRef, useState, useCallback } from 'react';
import { auth } from '../firebaseConfig';
import { onIdTokenChanged, signOut, type User } from 'firebase/auth';
import { api } from '../services/apiConfig';
import { Box, CircularProgress, Typography } from '@mui/material';

interface AuthContextType {
  user: User | null;
  loading: boolean;
  logout: () => Promise<void>;
  error: string | null;
}

const AuthContext = createContext<AuthContextType>({ 
  user: null, 
  loading: true, 
  logout: async () => {},
  error: null
});

export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const previousUserRef = useRef<User | null>(null);

  const logout = useCallback(async () => {
    try {
      await api.delete('/Auth/session').catch(() => undefined);
      await signOut(auth);
      setUser(null);
    } catch (errorVal) {
      console.error('Çıkış yapılırken hata:', errorVal);
      throw errorVal;
    }
  }, []);

  useEffect(() => {
    let isMounted = true;

    const unsubscribe = onIdTokenChanged(auth, async (currentUser) => {
      if (!isMounted) {
        return;
      }

      setLoading(true);
      setError(null);

      try {
        if (currentUser) {
          const token = await currentUser.getIdToken();
          await api.post('/Auth/session', { idToken: token });
          if (isMounted) {
            setUser(currentUser);
          }
        } else if (previousUserRef.current) {
          await api.delete('/Auth/session').catch(() => undefined);
          if (isMounted) {
            setUser(null);
          }
        } else {
          if (isMounted) {
            setUser(null);
          }
        }

        previousUserRef.current = currentUser;
      } catch (errorVal) {
        console.error('Oturum oluşturulamadı:', errorVal);
        const errorMessage = errorVal instanceof Error ? errorVal.message : 'Oturum oluşturulamadı';
        if (isMounted) {
          setError(errorMessage);
          setUser(null);
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    });

    return () => {
      isMounted = false;
      unsubscribe();
    };
  }, []);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh" bgcolor="#f5f5f5">
        <Box textAlign="center">
          <CircularProgress />
          <Typography mt={2}>Yükleniyor...</Typography>
        </Box>
      </Box>
    );
  }

  if (error) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh" bgcolor="#f5f5f5">
        <Box textAlign="center">
          <Typography color="error" variant="h6">Hata oluştu</Typography>
          <Typography color="error">{error}</Typography>
        </Box>
      </Box>
    );
  }

  return (
    <AuthContext.Provider value={{ user, loading, logout, error }}>
      {children}
    </AuthContext.Provider>
  );
};
