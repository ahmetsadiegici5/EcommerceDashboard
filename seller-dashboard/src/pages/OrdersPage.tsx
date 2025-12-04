import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Typography,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  MenuItem,
  Select,
  FormControl,
  InputLabel,
  Card,
  CardContent,
} from '@mui/material';
import { DataGrid, GridActionsCellItem } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import {
  Visibility as ViewIcon,
  Edit as EditIcon,
} from '@mui/icons-material';
import { orderService } from '../services/api';
import type { Order } from '../types';

const STATUS_LABELS: Record<string, string> = {
  Pending: 'Beklemede',
  Processing: 'İşleniyor',
  Shipped: 'Kargoda',
  Delivered: 'Teslim Edildi',
  Cancelled: 'İptal Edildi',
};

const STATUS_COLORS: Record<string, 'default' | 'warning' | 'info' | 'success' | 'error'> = {
  Pending: 'warning',
  Processing: 'info',
  Shipped: 'info',
  Delivered: 'success',
  Cancelled: 'error',
};

export default function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [statusDialogOpen, setStatusDialogOpen] = useState(false);
  const [newStatus, setNewStatus] = useState('');

  useEffect(() => {
    loadOrders();
  }, []);

  const loadOrders = async () => {
    try {
      setLoading(true);
      const data = await orderService.getAll();
      setOrders(data);
    } catch (error) {
      console.error('Siparişler yüklenirken hata:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleViewOrder = (order: Order) => {
    setSelectedOrder(order);
    setOpenDialog(true);
  };

  const handleStatusChange = (order: Order) => {
    setSelectedOrder(order);
    setNewStatus(order.status);
    setStatusDialogOpen(true);
  };

  const handleUpdateStatus = async () => {
    if (!selectedOrder?.id) return;

    try {
      await orderService.updateStatus(selectedOrder.id, newStatus);
      setStatusDialogOpen(false);
      loadOrders();
    } catch (error) {
      console.error('Durum güncellenirken hata:', error);
    }
  };

  const columns: GridColDef[] = [
    { field: 'orderNumber', headerName: 'Sipariş No', width: 150 },
    { field: 'customerName', headerName: 'Müşteri', width: 180 },
    { field: 'customerEmail', headerName: 'E-posta', width: 200 },
    {
      field: 'totalAmount',
      headerName: 'Toplam',
      width: 120,
      valueFormatter: (value) => `₺${Number(value).toLocaleString('tr-TR', { minimumFractionDigits: 2 })}`,
    },
    {
      field: 'status',
      headerName: 'Durum',
      width: 130,
      renderCell: (params) => (
        <Chip
          label={STATUS_LABELS[params.value] || params.value}
          color={STATUS_COLORS[params.value] || 'default'}
          size="small"
        />
      ),
    },
    {
      field: 'orderDate',
      headerName: 'Tarih',
      width: 120,
      valueFormatter: (value) => new Date(value).toLocaleDateString('tr-TR'),
    },
    {
      field: 'trackingNumber',
      headerName: 'Takip No',
      width: 130,
      renderCell: (params) => params.value || '-',
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: 'İşlemler',
      width: 120,
      getActions: (params) => [
        <GridActionsCellItem
          icon={<ViewIcon />}
          label="Görüntüle"
          onClick={() => handleViewOrder(params.row as Order)}
        />,
        <GridActionsCellItem
          icon={<EditIcon />}
          label="Durum Değiştir"
          onClick={() => handleStatusChange(params.row as Order)}
        />,
      ],
    },
  ];

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Siparişler</Typography>
      </Box>

      <DataGrid
        rows={orders}
        columns={columns}
        loading={loading}
        autoHeight
        pageSizeOptions={[10, 25, 50]}
        initialState={{
          pagination: { paginationModel: { pageSize: 10 } },
        }}
      />

      {/* Sipariş Detay Dialog */}
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="md" fullWidth>
        <DialogTitle>Sipariş Detayları - {selectedOrder?.orderNumber}</DialogTitle>
        <DialogContent>
          {selectedOrder && (
            <Box sx={{ mt: 2 }}>
              <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
                <Card sx={{ flex: 1 }}>
                  <CardContent>
                    <Typography variant="subtitle2" color="text.secondary">
                      Müşteri Bilgileri
                    </Typography>
                    <Typography variant="body1" sx={{ mt: 1 }}>
                      <strong>Ad:</strong> {selectedOrder.customerName}
                    </Typography>
                    <Typography variant="body2">
                      <strong>E-posta:</strong> {selectedOrder.customerEmail}
                    </Typography>
                    <Typography variant="body2">
                      <strong>Telefon:</strong> {selectedOrder.customerPhone}
                    </Typography>
                    <Typography variant="body2" sx={{ mt: 1 }}>
                      <strong>Adres:</strong> {selectedOrder.shippingAddress}
                    </Typography>
                  </CardContent>
                </Card>

                <Card sx={{ flex: 1 }}>
                  <CardContent>
                    <Typography variant="subtitle2" color="text.secondary">
                      Sipariş Bilgileri
                    </Typography>
                    <Typography variant="body2" sx={{ mt: 1 }}>
                      <strong>Durum:</strong>{' '}
                      <Chip
                        label={STATUS_LABELS[selectedOrder.status]}
                        color={STATUS_COLORS[selectedOrder.status]}
                        size="small"
                      />
                    </Typography>
                    <Typography variant="body2" sx={{ mt: 1 }}>
                      <strong>Sipariş Tarihi:</strong>{' '}
                      {new Date(selectedOrder.orderDate).toLocaleString('tr-TR')}
                    </Typography>
                    {selectedOrder.trackingNumber && (
                      <Typography variant="body2">
                        <strong>Takip No:</strong> {selectedOrder.trackingNumber}
                      </Typography>
                    )}
                    <Typography variant="h6" sx={{ mt: 2 }}>
                      <strong>Toplam:</strong> ₺
                      {selectedOrder.totalAmount.toLocaleString('tr-TR', {
                        minimumFractionDigits: 2,
                      })}
                    </Typography>
                  </CardContent>
                </Card>
              </Box>

              <Typography variant="h6" gutterBottom>
                Sipariş Ürünleri
              </Typography>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Ürün</TableCell>
                    <TableCell align="right">Adet</TableCell>
                    <TableCell align="right">Birim Fiyat</TableCell>
                    <TableCell align="right">Toplam</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {selectedOrder.items.map((item, index) => (
                    <TableRow key={index}>
                      <TableCell>{item.productName}</TableCell>
                      <TableCell align="right">{item.quantity}</TableCell>
                      <TableCell align="right">
                        ₺{item.unitPrice.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}
                      </TableCell>
                      <TableCell align="right">
                        ₺{item.totalPrice.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>Kapat</Button>
        </DialogActions>
      </Dialog>

      {/* Durum Değiştirme Dialog */}
      <Dialog open={statusDialogOpen} onClose={() => setStatusDialogOpen(false)}>
        <DialogTitle>Sipariş Durumunu Değiştir</DialogTitle>
        <DialogContent>
          <FormControl fullWidth sx={{ mt: 2 }}>
            <InputLabel>Durum</InputLabel>
            <Select
              value={newStatus}
              label="Durum"
              onChange={(e) => setNewStatus(e.target.value)}
            >
              <MenuItem value="Pending">Beklemede</MenuItem>
              <MenuItem value="Processing">İşleniyor</MenuItem>
              <MenuItem value="Shipped">Kargoda</MenuItem>
              <MenuItem value="Delivered">Teslim Edildi</MenuItem>
              <MenuItem value="Cancelled">İptal Edildi</MenuItem>
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setStatusDialogOpen(false)}>İptal</Button>
          <Button onClick={handleUpdateStatus} variant="contained">
            Güncelle
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
