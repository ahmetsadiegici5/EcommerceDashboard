import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import type { ReactNode } from 'react';

interface AuthGuardProps {
    children: ReactNode;
}

export function AuthGuard({ children }: AuthGuardProps) {
    const { user, loading } = useAuth();

    if (loading) {
        return null;
    }

    if (!user) {
        return <Navigate to="/login" />;
    }

    return <>{children}</>;
}