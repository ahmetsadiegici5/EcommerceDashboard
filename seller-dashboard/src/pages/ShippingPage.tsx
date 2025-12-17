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
  TextField,
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
  Refresh as RefreshIcon,
  Add as AddIcon,
  LocalShipping as ShippingIcon,
} from '@mui/icons-material';
import { shippingService, orderService } from '../services/api';
import type { Shipping, Order } from '../types';
import { useToast } from '../contexts/ToastContext';

const STATUS_LABELS: Record<string, string> = {
  Preparing: 'Hazırlanıyor',
  Shipped: 'Kargoya Verildi',
  InTransit: 'Yolda',
  OutForDelivery: 'Dağıtımda',
  Delivered: 'Teslim Edildi',
  Failed: 'Başarısız',
};

const STATUS_COLORS: Record<string, 'default' | 'warning' | 'info' | 'success' | 'error'> = {
  Preparing: 'warning',
  Shipped: 'info',
  InTransit: 'info',
  OutForDelivery: 'info',
  Delivered: 'success',
  Failed: 'error',
};

export default function ShippingPage() {
  const [shippingList, setShippingList] = useState<Shipping[]>([]);
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedShipping, setSelectedShipping] = useState<Shipping | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [statusDialogOpen, setStatusDialogOpen] = useState(false);
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [newStatus, setNewStatus] = useState('');
  const [newLocation, setNewLocation] = useState('');
  const [newDescription, setNewDescription] = useState('');
  const [newShipping, setNewShipping] = useState({
    orderId: '',
    carrier: '',
    trackingNumber: '',
  });
  const { showToast } = useToast();

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async (manual = false) => {
    try {
      setLoading(true);
      const [shippingData, ordersData] = await Promise.all([
        shippingService.getAll(),
        orderService.getAll(),
      ]);
      setShippingList(shippingData);
      setOrders(ordersData);
      if (manual) showToast('Kargo bilgileri güncellendi', 'success');
    } catch (error) {
      console.error('Veriler yüklenirken hata:', error);
      showToast('Veriler yüklenirken hata oluştu', 'error');
    } finally {
      setLoading(false);
    }
  };

  const handleViewShipping = (shipping: Shipping) => {
    setSelectedShipping(shipping);
    setOpenDialog(true);
  };

  const handleStatusChange = (shipping: Shipping) => {
    setSelectedShipping(shipping);
    setNewStatus(shipping.status);
    setNewLocation('');
    setNewDescription('');
    setStatusDialogOpen(true);
  };

  const handleUpdateStatus = async () => {
    if (!selectedShipping?.id) return;

    try {
      await shippingService.updateStatus(
        selectedShipping.id,
        newStatus,
        newLocation,
        newDescription
      );
      setStatusDialogOpen(false);
      showToast('Kargo durumu güncellendi', 'success');
      loadData();
    } catch (error) {
      console.error('Durum güncellenirken hata:', error);
      showToast('Durum güncellenirken hata oluştu', 'error');
    }
  };

  const handleCreateShipping = async () => {
    if (!newShipping.orderId || !newShipping.carrier) {
      showToast('Sipariş ve kargo firması seçimi zorunludur', 'warning');
      return;
    }

    try {
      await shippingService.create(newShipping as Omit<Shipping, 'id'>);
      setCreateDialogOpen(false);
      setNewShipping({ orderId: '', carrier: '', trackingNumber: '' });
      showToast('Kargo kaydı oluşturuldu', 'success');
      loadData();
    } catch (error) {
      console.error('Kargo oluşturulurken hata:', error);
      showToast('Kargo oluşturulurken hata oluştu', 'error');
    }
  };

  const columns: GridColDef[] = [
    { field: 'trackingNumber', headerName: 'Takip No', width: 150 },
    { field: 'carrier', headerName: 'Kargo Firması', width: 130 },
    { field: 'orderId', headerName: 'Sipariş ID', width: 150 },
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
      field: 'estimatedDelivery',
      headerName: 'Tahmini Teslimat',
      width: 140,
      valueFormatter: (value) =>
        value ? new Date(value).toLocaleDateString('tr-TR') : '-',
    },
    {
      field: 'actualDelivery',
      headerName: 'Teslim Tarihi',
      width: 130,
      valueFormatter: (value) =>
        value ? new Date(value).toLocaleDateString('tr-TR') : '-',
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: 'İşlemler',
      width: 120,
      getActions: (params) => [
        <GridActionsCellItem
          key="view"
          icon={<ViewIcon />}
          label="Görüntüle"
          onClick={() => handleViewShipping(params.row as Shipping)}
        />,
        <GridActionsCellItem
          key="edit"
          icon={<EditIcon />}
          label="Durum Değiştir"
          onClick={() => handleStatusChange(params.row as Shipping)}
        />,
      ],
    },
  ];

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Kargo Takip</Typography>
        <Box display="flex" gap={2}>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => setCreateDialogOpen(true)}
          >
            Yeni Kargo
          </Button>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={() => loadData(true)}
            disabled={loading}
          >
            Yenile
          </Button>
        </Box>
      </Box>

      <DataGrid
        rows={shippingList}
        columns={columns}
        loading={loading}
        autoHeight
        pageSizeOptions={[10, 25, 50]}
        initialState={{
          pagination: { paginationModel: { pageSize: 10 } },
        }}
      />

      {/* Kargo Detay Dialog */}
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          <Box display="flex" alignItems="center" gap={1}>
            <ShippingIcon color="primary" />
            Kargo Detayları - {selectedShipping?.trackingNumber}
          </Box>
        </DialogTitle>
        <DialogContent>
          {selectedShipping && (
            <Box sx={{ mt: 2 }}>
              <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
                <Card sx={{ flex: 1 }}>
                  <CardContent>
                    <Typography variant="subtitle2" color="text.secondary">
                      Kargo Bilgileri
                    </Typography>
                    <Typography variant="body1" sx={{ mt: 1 }}>
                      <strong>Firma:</strong> {selectedShipping.carrier}
                    </Typography>
                    <Typography variant="body2">
                      <strong>Takip No:</strong> {selectedShipping.trackingNumber}
                    </Typography>
                    <Typography variant="body2">
                      <strong>Durum:</strong>{' '}
                      <Chip
                        label={STATUS_LABELS[selectedShipping.status] || selectedShipping.status}
                        color={STATUS_COLORS[selectedShipping.status] || 'default'}
                        size="small"
                      />
                    </Typography>
                  </CardContent>
                </Card>
                <Card sx={{ flex: 1 }}>
                  <CardContent>
                    <Typography variant="subtitle2" color="text.secondary">
                      Teslimat Bilgileri
                    </Typography>
                    <Typography variant="body1" sx={{ mt: 1 }}>
                      <strong>Alıcı:</strong> {selectedShipping.recipientName}
                    </Typography>
                    <Typography variant="body2">
                      <strong>Telefon:</strong> {selectedShipping.recipientPhone}
                    </Typography>
                    <Typography variant="body2">
                      <strong>Adres:</strong> {selectedShipping.shippingAddress}
                    </Typography>
                  </CardContent>
                </Card>
              </Box>

              {/* Kargo Geçmişi */}
              <Typography variant="h6" gutterBottom>
                Kargo Geçmişi
              </Typography>
              {selectedShipping.events && selectedShipping.events.length > 0 ? (
                <Box sx={{ pl: 2 }}>
                  {selectedShipping.events.map((event, index) => (
                    <Box
                      key={index}
                      sx={{
                        display: 'flex',
                        mb: 2,
                        borderLeft: '2px solid',
                        borderColor: 'primary.main',
                        pl: 2,
                      }}
                    >
                      <Box>
                        <Typography variant="body2" color="text.secondary">
                          {new Date(event.timestamp).toLocaleString('tr-TR')}
                        </Typography>
                        <Typography variant="body1">
                          <strong>{STATUS_LABELS[event.status] || event.status}</strong>
                        </Typography>
                        <Typography variant="body2">
                          {event.location} - {event.description}
                        </Typography>
                      </Box>
                    </Box>
                  ))}
                </Box>
              ) : (
                <Typography color="text.secondary">Henüz kargo hareketi bulunmuyor.</Typography>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>Kapat</Button>
        </DialogActions>
      </Dialog>

      {/* Durum Güncelleme Dialog */}
      <Dialog open={statusDialogOpen} onClose={() => setStatusDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Kargo Durumu Güncelle</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
            <FormControl fullWidth>
              <InputLabel>Durum</InputLabel>
              <Select
                value={newStatus}
                onChange={(e) => setNewStatus(e.target.value)}
                label="Durum"
              >
                <MenuItem value="Preparing">Hazırlanıyor</MenuItem>
                <MenuItem value="Shipped">Kargoya Verildi</MenuItem>
                <MenuItem value="InTransit">Yolda</MenuItem>
                <MenuItem value="OutForDelivery">Dağıtımda</MenuItem>
                <MenuItem value="Delivered">Teslim Edildi</MenuItem>
                <MenuItem value="Failed">Başarısız</MenuItem>
              </Select>
            </FormControl>
            <TextField
              label="Konum"
              value={newLocation}
              onChange={(e) => setNewLocation(e.target.value)}
              fullWidth
              placeholder="Örn: İstanbul Dağıtım Merkezi"
            />
            <TextField
              label="Açıklama"
              value={newDescription}
              onChange={(e) => setNewDescription(e.target.value)}
              fullWidth
              multiline
              rows={2}
              placeholder="Örn: Paket teslim alındı"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setStatusDialogOpen(false)}>İptal</Button>
          <Button onClick={handleUpdateStatus} variant="contained">
            Güncelle
          </Button>
        </DialogActions>
      </Dialog>

      {/* Yeni Kargo Dialog */}
      <Dialog open={createDialogOpen} onClose={() => setCreateDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Yeni Kargo Oluştur</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
            <FormControl fullWidth>
              <InputLabel>Sipariş</InputLabel>
              <Select
                value={newShipping.orderId}
                onChange={(e) => setNewShipping({ ...newShipping, orderId: e.target.value })}
                label="Sipariş"
              >
                {orders.map((order) => (
                  <MenuItem key={order.id} value={order.id}>
                    {order.orderNumber} - {order.customerName}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <FormControl fullWidth>
              <InputLabel>Kargo Firması</InputLabel>
              <Select
                value={newShipping.carrier}
                onChange={(e) => setNewShipping({ ...newShipping, carrier: e.target.value })}
                label="Kargo Firması"
              >
                <MenuItem value="Yurtiçi Kargo">Yurtiçi Kargo</MenuItem>
                <MenuItem value="Aras Kargo">Aras Kargo</MenuItem>
                <MenuItem value="MNG Kargo">MNG Kargo</MenuItem>
                <MenuItem value="PTT Kargo">PTT Kargo</MenuItem>
                <MenuItem value="Sürat Kargo">Sürat Kargo</MenuItem>
                <MenuItem value="UPS">UPS</MenuItem>
                <MenuItem value="DHL">DHL</MenuItem>
              </Select>
            </FormControl>
            <TextField
              label="Takip Numarası (Opsiyonel)"
              value={newShipping.trackingNumber}
              onChange={(e) => setNewShipping({ ...newShipping, trackingNumber: e.target.value })}
              fullWidth
              placeholder="Boş bırakılırsa otomatik oluşturulur"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreateDialogOpen(false)}>İptal</Button>
          <Button onClick={handleCreateShipping} variant="contained">
            Oluştur
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
