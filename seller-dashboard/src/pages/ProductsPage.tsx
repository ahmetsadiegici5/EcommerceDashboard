import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Typography,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControlLabel,
  Switch,
  Chip,
} from '@mui/material';
import { DataGrid, GridActionsCellItem } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import { Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, Refresh as RefreshIcon } from '@mui/icons-material';
import { productService } from '../services/api';
import type { Product } from '../types';
import { useToast } from '../contexts/ToastContext';

type ProductFormData = Omit<Product, 'id' | 'sellerId' | 'createdAt' | 'updatedAt'>;

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const createEmptyForm = (): ProductFormData => ({
    name: '',
    description: '',
    price: 0,
    stock: 0,
    category: '',
    sku: '',
    imageUrl: '',
    isActive: true,
  });

  const [formData, setFormData] = useState<ProductFormData>(createEmptyForm());
  const { showToast } = useToast();

  useEffect(() => {
    loadProducts();
  }, []);

  const loadProducts = async (manual = false) => {
    try {
      setLoading(true);
      const data = await productService.getAll();
      setProducts(data);
      if (manual) showToast('Ürünler güncellendi', 'success');
    } catch (error) {
      console.error('Ürünler yüklenirken hata:', error);
      showToast('Ürünler yüklenirken hata oluştu', 'error');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (product?: Product) => {
    if (product) {
      setEditingProduct(product);
      const { sellerId: _sellerId, createdAt: _createdAt, updatedAt: _updatedAt, id: _id, ...rest } = product;
      setFormData(rest);
    } else {
      setEditingProduct(null);
      setFormData(createEmptyForm());
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingProduct(null);
  };

  const handleSave = async () => {
    try {
      if (editingProduct?.id) {
        await productService.update(editingProduct.id, formData);
        showToast('Ürün güncellendi', 'success');
      } else {
        await productService.create(formData);
        showToast('Ürün oluşturuldu', 'success');
      }
      handleCloseDialog();
      loadProducts();
    } catch (error) {
      console.error('Ürün kaydedilirken hata:', error);
      showToast('Ürün kaydedilirken hata oluştu', 'error');
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Bu ürünü silmek istediğinizden emin misiniz?')) {
      try {
        await productService.delete(id);
        showToast('Ürün silindi', 'success');
        loadProducts();
      } catch (error) {
        console.error('Ürün silinirken hata:', error);
        showToast('Ürün silinirken hata oluştu', 'error');
      }
    }
  };

  const columns: GridColDef[] = [
    { field: 'name', headerName: 'Ürün Adı', width: 200 },
    { field: 'sku', headerName: 'SKU', width: 150 },
    { field: 'category', headerName: 'Kategori', width: 130 },
    {
      field: 'price',
      headerName: 'Fiyat',
      width: 120,
      valueFormatter: (value) => `₺${Number(value).toLocaleString('tr-TR', { minimumFractionDigits: 2 })}`,
    },
    {
      field: 'stock',
      headerName: 'Stok',
      width: 100,
      renderCell: (params) => (
        <Chip
          label={params.value}
          color={params.value < 10 ? 'error' : 'success'}
          size="small"
        />
      ),
    },
    {
      field: 'isActive',
      headerName: 'Durum',
      width: 100,
      renderCell: (params) => (
        <Chip
          label={params.value ? 'Aktif' : 'Pasif'}
          color={params.value ? 'success' : 'default'}
          size="small"
        />
      ),
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: 'İşlemler',
      width: 120,
      getActions: (params) => [
        <GridActionsCellItem
          icon={<EditIcon />}
          label="Düzenle"
          onClick={() => handleOpenDialog(params.row as Product)}
        />,
        <GridActionsCellItem
          icon={<DeleteIcon />}
          label="Sil"
          onClick={() => handleDelete(params.row.id)}
        />,
      ],
    },
  ];

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Ürünler</Typography>
        <Box>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={() => loadProducts(true)}
            disabled={loading}
            sx={{ mr: 2 }}
          >
            Yenile
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
          >
            Yeni Ürün
          </Button>
        </Box>
      </Box>

      <DataGrid
        rows={products}
        columns={columns}
        loading={loading}
        autoHeight
        pageSizeOptions={[10, 25, 50]}
        initialState={{
          pagination: { paginationModel: { pageSize: 10 } },
        }}
      />

      {/* Ürün Ekleme/Düzenleme Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingProduct ? 'Ürün Düzenle' : 'Yeni Ürün Ekle'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              label="Ürün Adı"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              fullWidth
              required
            />
            <TextField
              label="Açıklama"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              fullWidth
              multiline
              rows={3}
            />
            <TextField
              label="Fiyat"
              type="number"
              value={formData.price}
              onChange={(e) => setFormData({ ...formData, price: parseFloat(e.target.value) })}
              fullWidth
              required
            />
            <TextField
              label="Stok"
              type="number"
              value={formData.stock}
              onChange={(e) => setFormData({ ...formData, stock: parseInt(e.target.value) })}
              fullWidth
              required
            />
            <TextField
              label="Kategori"
              value={formData.category}
              onChange={(e) => setFormData({ ...formData, category: e.target.value })}
              fullWidth
            />
            <TextField
              label="SKU"
              value={formData.sku}
              onChange={(e) => setFormData({ ...formData, sku: e.target.value })}
              fullWidth
              required
            />
            <TextField
              label="Görsel URL"
              value={formData.imageUrl}
              onChange={(e) => setFormData({ ...formData, imageUrl: e.target.value })}
              fullWidth
            />
            <FormControlLabel
              control={
                <Switch
                  checked={formData.isActive}
                  onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                />
              }
              label="Aktif"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>İptal</Button>
          <Button onClick={handleSave} variant="contained">
            Kaydet
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
