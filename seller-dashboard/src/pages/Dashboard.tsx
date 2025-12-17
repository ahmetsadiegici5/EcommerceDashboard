import { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  CircularProgress,
  Button,
} from '@mui/material';
import {
  Inventory as InventoryIcon,
  ShoppingCart as ShoppingCartIcon,
  AttachMoney as MoneyIcon,
  Warning as WarningIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { dashboardService } from '../services/api';
import type { DashboardStats } from '../types';
import { useToast } from '../contexts/ToastContext';

import { useTheme, alpha } from '@mui/material/styles';

interface StatCardProps {
  title: string;
  value: number;
  icon: React.ReactNode;
  color: 'primary' | 'success' | 'warning' | 'error';
  format?: 'number' | 'currency';
}

function StatCard({ title, value, icon, color, format = 'number' }: StatCardProps) {
  const theme = useTheme();
  const formattedValue =
    format === 'currency'
      ? `₺${value.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}`
      : value.toLocaleString('tr-TR');

  return (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Box display="flex" justifyContent="space-between" alignItems="flex-start">
          <Box>
            <Typography variant="subtitle2" color="textSecondary" gutterBottom sx={{ fontWeight: 600 }}>
              {title}
            </Typography>
            <Typography variant="h4" component="div" sx={{ fontWeight: 700, color: 'text.primary' }}>
              {formattedValue}
            </Typography>
          </Box>
          <Box
            sx={{
              backgroundColor: alpha(theme.palette[color].main, 0.1),
              color: theme.palette[color].main,
              borderRadius: 3,
              p: 1.5,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            {icon}
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
}

export default function Dashboard() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const { showToast } = useToast();

  useEffect(() => {
    loadStats();
  }, []);

  const loadStats = async (manual = false) => {
    try {
      if (manual) setLoading(true);
      const data = await dashboardService.getStats();
      setStats(data);
      if (manual) showToast('İstatistikler güncellendi', 'success');
    } catch (error) {
      console.error('İstatistikler yüklenirken hata:', error);
      showToast('İstatistikler yüklenirken hata oluştu', 'error');
    } finally {
      setLoading(false);
    }
  };

  // Örnek grafik verisi - gerçek uygulamada backend'den gelecek
  const chartData = [
    { name: 'Ocak', siparis: 40, gelir: 24000 },
    { name: 'Şubat', siparis: 30, gelir: 18000 },
    { name: 'Mart', siparis: 50, gelir: 30000 },
    { name: 'Nisan', siparis: 45, gelir: 27000 },
    { name: 'Mayıs', siparis: 60, gelir: 36000 },
    { name: 'Haziran', siparis: 55, gelir: 33000 },
  ];

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" gutterBottom sx={{ mb: 0 }}>
          Dashboard
        </Typography>
        <Button 
          variant="outlined" 
          startIcon={<RefreshIcon />} 
          onClick={() => loadStats(true)}
          disabled={loading}
        >
          Yenile
        </Button>
      </Box>

      <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', mb: 3 }}>
        <Box sx={{ flex: '1 1 250px', minWidth: 250 }}>
          <StatCard
            title="Toplam Ürün"
            value={stats?.totalProducts || 0}
            icon={<InventoryIcon fontSize="large" />}
            color="primary"
          />
        </Box>
        <Box sx={{ flex: '1 1 250px', minWidth: 250 }}>
          <StatCard
            title="Toplam Sipariş"
            value={stats?.totalOrders || 0}
            icon={<ShoppingCartIcon fontSize="large" />}
            color="success"
          />
        </Box>
        <Box sx={{ flex: '1 1 250px', minWidth: 250 }}>
          <StatCard
            title="Toplam Gelir"
            value={stats?.totalRevenue || 0}
            icon={<MoneyIcon fontSize="large" />}
            color="warning"
            format="currency"
          />
        </Box>
        <Box sx={{ flex: '1 1 250px', minWidth: 250 }}>
          <StatCard
            title="Düşük Stok"
            value={stats?.lowStockProducts || 0}
            icon={<WarningIcon fontSize="large" />}
            color="error"
          />
        </Box>
      </Box>

      <Card sx={{ p: 2 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom sx={{ mb: 4 }}>
            Aylık Satış Performansı
          </Typography>
          <ResponsiveContainer width="100%" height={350}>
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e5e7eb" />
              <XAxis 
                dataKey="name" 
                axisLine={false} 
                tickLine={false} 
                tick={{ fill: '#6b7280', fontSize: 12 }}
                dy={10}
              />
              <YAxis 
                yAxisId="left" 
                axisLine={false} 
                tickLine={false} 
                tick={{ fill: '#6b7280', fontSize: 12 }}
              />
              <YAxis 
                yAxisId="right" 
                orientation="right" 
                axisLine={false} 
                tickLine={false} 
                tick={{ fill: '#6b7280', fontSize: 12 }}
              />
              <Tooltip 
                contentStyle={{ 
                  backgroundColor: '#fff', 
                  borderRadius: '8px', 
                  boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)',
                  border: 'none'
                }}
              />
              <Legend wrapperStyle={{ paddingTop: '20px' }} />
              <Line
                yAxisId="left"
                type="monotone"
                dataKey="siparis"
                stroke="#6366f1"
                strokeWidth={3}
                dot={{ fill: '#6366f1', strokeWidth: 2 }}
                activeDot={{ r: 8 }}
                name="Sipariş Sayısı"
              />
              <Line
                yAxisId="right"
                type="monotone"
                dataKey="gelir"
                stroke="#10b981"
                strokeWidth={3}
                dot={{ fill: '#10b981', strokeWidth: 2 }}
                activeDot={{ r: 8 }}
                name="Gelir (₺)"
              />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>
    </Box>
  );
}
