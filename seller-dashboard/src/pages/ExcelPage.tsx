import { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  Alert,
  LinearProgress,
  Card,
  CardContent,
} from '@mui/material';
import {
  CloudUpload as UploadIcon,
  Download as DownloadIcon,
  Description as FileIcon,
} from '@mui/icons-material';
import { useDropzone } from 'react-dropzone';
import { productService } from '../services/api';
import { useToast } from '../contexts/ToastContext';

export default function ExcelPage() {
  const [uploading, setUploading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const { showToast } = useToast();

  const onDrop = async (acceptedFiles: File[]) => {
    if (acceptedFiles.length === 0) return;

    const file = acceptedFiles[0];
    setUploading(true);
    setMessage(null);

    try {
      await productService.importFromExcel(file);
      const successMsg = 'Excel dosyası başarıyla yüklendi ve ürünler eklendi!';
      setMessage({
        type: 'success',
        text: successMsg,
      });
      showToast(successMsg, 'success');
    } catch (error) {
      const errorMsg = 'Dosya yüklenirken bir hata oluştu. Lütfen dosyanızı kontrol edin.';
      setMessage({
        type: 'error',
        text: errorMsg,
      });
      showToast(errorMsg, 'error');
      console.error('Excel yükleme hatası:', error);
    } finally {
      setUploading(false);
    }
  };

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': ['.xlsx'],
      'application/vnd.ms-excel': ['.xls'],
    },
    multiple: false,
  });

  const handleDownloadTemplate = async () => {
    try {
      const blob = await productService.downloadTemplate();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'urun-sablonu.xlsx';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      showToast('Şablon başarıyla indirildi', 'success');
    } catch (error) {
      console.error('Şablon indirme hatası:', error);
      setMessage({
        type: 'error',
        text: 'Şablon indirilirken bir hata oluştu.',
      });
      showToast('Şablon indirilirken bir hata oluştu', 'error');
    }
  };

  const handleExportProducts = async () => {
    try {
      const blob = await productService.exportToExcel();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `urunler-${new Date().toISOString().split('T')[0]}.xlsx`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      const successMsg = 'Ürünler başarıyla Excel dosyasına aktarıldı!';
      setMessage({
        type: 'success',
        text: successMsg,
      });
      showToast(successMsg, 'success');
    } catch (error) {
      console.error('Export hatası:', error);
      setMessage({
        type: 'error',
        text: 'Ürünler aktarılırken bir hata oluştu.',
      });
      showToast('Ürünler aktarılırken bir hata oluştu', 'error');
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Excel İle Toplu İşlemler
      </Typography>

      {message && (
        <Alert severity={message.type} sx={{ mb: 3 }} onClose={() => setMessage(null)}>
          {message.text}
        </Alert>
      )}

      <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
        {/* Excel Şablonu İndir */}
        <Box sx={{ flex: '1 1 300px', minWidth: 300 }}>
          <Card>
            <CardContent>
              <Box display="flex" flexDirection="column" alignItems="center" textAlign="center">
                <FileIcon sx={{ fontSize: 60, color: 'primary.main', mb: 2 }} />
                <Typography variant="h6" gutterBottom>
                  Excel Şablonu
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Ürün bilgilerinizi doğru formatta girebilmeniz için örnek Excel şablonunu indirin.
                </Typography>
                <Button
                  variant="contained"
                  startIcon={<DownloadIcon />}
                  onClick={handleDownloadTemplate}
                  fullWidth
                >
                  Şablon İndir
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Box>

        {/* Excel Yükle */}
        <Box sx={{ flex: '1 1 300px', minWidth: 300 }}>
          <Card>
            <CardContent>
              <Box display="flex" flexDirection="column" alignItems="center" textAlign="center">
                <UploadIcon sx={{ fontSize: 60, color: 'success.main', mb: 2 }} />
                <Typography variant="h6" gutterBottom>
                  Toplu Ürün Ekle
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Hazırladığınız Excel dosyasını yükleyerek toplu olarak ürün ekleyin.
                </Typography>
                <Paper
                  {...getRootProps()}
                  sx={{
                    p: 3,
                    border: '2px dashed',
                    borderColor: isDragActive ? 'success.main' : 'grey.300',
                    backgroundColor: isDragActive ? 'action.hover' : 'background.paper',
                    cursor: 'pointer',
                    width: '100%',
                    '&:hover': {
                      borderColor: 'success.main',
                      backgroundColor: 'action.hover',
                    },
                  }}
                >
                  <input {...getInputProps()} />
                  <Box textAlign="center">
                    <UploadIcon sx={{ fontSize: 40, color: 'text.secondary', mb: 1 }} />
                    <Typography variant="body2">
                      {isDragActive
                        ? 'Dosyayı buraya bırakın...'
                        : 'Excel dosyasını sürükleyip bırakın veya tıklayın'}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      (.xlsx, .xls)
                    </Typography>
                  </Box>
                </Paper>
                {uploading && <LinearProgress sx={{ width: '100%', mt: 2 }} />}
              </Box>
            </CardContent>
          </Card>
        </Box>

        {/* Ürünleri Dışa Aktar */}
        <Box sx={{ flex: '1 1 300px', minWidth: 300 }}>
          <Card>
            <CardContent>
              <Box display="flex" flexDirection="column" alignItems="center" textAlign="center">
                <DownloadIcon sx={{ fontSize: 60, color: 'info.main', mb: 2 }} />
                <Typography variant="h6" gutterBottom>
                  Ürünleri Dışa Aktar
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Mevcut ürünlerinizi Excel dosyası olarak indirin ve düzenleyin.
                </Typography>
                <Button
                  variant="contained"
                  color="info"
                  startIcon={<DownloadIcon />}
                  onClick={handleExportProducts}
                  fullWidth
                >
                  Excel'e Aktar
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Box>
      </Box>

      <Box mt={4}>
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            📋 Kullanım Talimatları
          </Typography>
          <Typography variant="body2" component="div">
            <ol>
              <li>
                <strong>Excel Şablonu İndir:</strong> Önce "Şablon İndir" butonuna tıklayarak örnek
                Excel dosyasını indirin.
              </li>
              <li>
                <strong>Ürün Bilgilerini Girin:</strong> İndirdiğiniz dosyayı açın ve ürün
                bilgilerinizi ilgili sütunlara girin.
                <ul>
                  <li>Ürün Adı, Açıklama, Fiyat, Stok, Kategori, SKU zorunludur</li>
                  <li>Görsel URL ve Aktif durumu opsiyoneldir</li>
                </ul>
              </li>
              <li>
                <strong>Dosyayı Yükleyin:</strong> Hazırladığınız Excel dosyasını "Toplu Ürün Ekle"
                alanına sürükleyip bırakın veya tıklayarak seçin.
              </li>
              <li>
                <strong>Dışa Aktarma:</strong> Mevcut ürünlerinizi düzenlemek için "Excel'e Aktar"
                butonunu kullanarak tüm ürünleri indirebilirsiniz.
              </li>
            </ol>
          </Typography>
        </Paper>
      </Box>
    </Box>
  );
}
