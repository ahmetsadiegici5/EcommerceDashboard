import { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  CircularProgress,
} from '@mui/material';
import {
  Inventory as InventoryIcon,
  ShoppingCart as ShoppingCartIcon,
  AttachMoney as MoneyIcon,
  Warning as WarningIcon,
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

interface StatCardProps {
  title: string;
  value: number;
  icon: React.ReactNode;
  color: string;
  format?: 'number' | 'currency';
}

function StatCard({ title, value, icon, color, format = 'number' }: StatCardProps) {
  const formattedValue =
    format === 'currency'
      ? `₺${value.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}`
      : value.toLocaleString('tr-TR');

  return (
    <Card>
      <CardContent>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Box>
            <Typography color="textSecondary" gutterBottom>
              {title}
            </Typography>
            <Typography variant="h4" component="div">
              {formattedValue}
            </Typography>
          </Box>
          <Box
            sx={{
              backgroundColor: color,
              borderRadius: 2,
              p: 2,
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

  useEffect(() => {
    loadStats();
  }, []);

  const loadStats = async () => {
    try {
      const data = await dashboardService.getStats();
      setStats(data);
    } catch (error) {
      console.error('İstatistikler yüklenirken hata:', error);
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
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>

      <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', mb: 3 }}>
        <Box sx={{ flex: '1 1 250px', minWidth: 250 }}>
          <StatCard
            title="Toplam Ürün"
            value={stats?.totalProducts || 0}
            icon={<InventoryIcon sx={{ color: 'white', fontSize: 40 }} />}
            color="#1976d2"
          />
        </Box>
        <Box sx={{ flex: '1 1 250px', minWidth: 250 }}>
          <StatCard
            title="Toplam Sipariş"
            value={stats?.totalOrders || 0}
            icon={<ShoppingCartIcon sx={{ color: 'white', fontSize: 40 }} />}
            color="#2e7d32"
          />
        </Box>
        <Box sx={{ flex: '1 1 250px', minWidth: 250 }}>
          <StatCard
            title="Toplam Gelir"
            value={stats?.totalRevenue || 0}
            icon={<MoneyIcon sx={{ color: 'white', fontSize: 40 }} />}
            color="#ed6c02"
            format="currency"
          />
        </Box>
        <Box sx={{ flex: '1 1 250px', minWidth: 250 }}>
          <StatCard
            title="Düşük Stok"
            value={stats?.lowStockProducts || 0}
            icon={<WarningIcon sx={{ color: 'white', fontSize: 40 }} />}
            color="#d32f2f"
          />
        </Box>
      </Box>

      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Aylık Satış Performansı
          </Typography>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" />
              <YAxis yAxisId="left" />
              <YAxis yAxisId="right" orientation="right" />
              <Tooltip />
              <Legend />
              <Line
                yAxisId="left"
                type="monotone"
                dataKey="siparis"
                stroke="#1976d2"
                name="Sipariş Sayısı"
              />
              <Line
                yAxisId="right"
                type="monotone"
                dataKey="gelir"
                stroke="#2e7d32"
                name="Gelir (₺)"
              />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>
    </Box>
  );
}
