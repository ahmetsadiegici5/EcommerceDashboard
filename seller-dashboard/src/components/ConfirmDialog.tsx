import { Dialog, DialogTitle, DialogContent, DialogActions, Button, Typography } from '@mui/material';
import type { ReactNode } from 'react';

interface ConfirmDialogProps {
  open: boolean;
  title?: string;
  message: ReactNode;
  onConfirm: () => void;
  onCancel: () => void;
  confirmText?: string;
  cancelText?: string;
}

export default function ConfirmDialog({
  open,
  title = 'Onay',
  message,
  onConfirm,
  onCancel,
  confirmText = 'Evet',
  cancelText = 'İptal',
}: ConfirmDialogProps) {
  return (
    <Dialog open={open} onClose={onCancel} maxWidth="xs" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <Typography>{message}</Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>{cancelText}</Button>
        <Button onClick={onConfirm} variant="contained" color="error">
          {confirmText}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
