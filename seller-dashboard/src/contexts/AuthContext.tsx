import { createContext, useContext, useEffect, useRef, useState, useCallback } from 'react';
import { auth } from '../firebaseConfig';
import { onIdTokenChanged, signOut, type User } from 'firebase/auth';
import { api } from '../services/apiConfig';

interface AuthContextType {
  user: User | null;
  loading: boolean;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType>({ user: null, loading: true, logout: async () => {} });

export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const previousUserRef = useRef<User | null>(null);

  const logout = useCallback(async () => {
    try {
      await api.delete('/Auth/session').catch(() => undefined);
      await signOut(auth);
      setUser(null);
    } catch (error) {
      console.error('Çıkış yapılırken hata:', error);
      throw error;
    }
  }, []);

  useEffect(() => {
    let isMounted = true;

    const unsubscribe = onIdTokenChanged(auth, async (currentUser) => {
      if (!isMounted) {
        return;
      }

      setLoading(true);

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
      } catch (error) {
        console.error('Oturum oluşturulamadı:', error);
        if (isMounted) {
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

  return (
    <AuthContext.Provider value={{ user, loading, logout }}>
      {!loading && children}
    </AuthContext.Provider>
  );
};
