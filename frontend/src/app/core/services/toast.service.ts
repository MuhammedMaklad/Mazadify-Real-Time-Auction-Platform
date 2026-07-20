import { Injectable } from '@angular/core';

export interface ToastMessage {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private messages: ToastMessage[] = [];
  private listeners: ((messages: ToastMessage[]) => void)[] = [];

  private emit(): void {
    this.listeners.forEach((listener) => listener([...this.messages]));
  }

  subscribe(listener: (messages: ToastMessage[]) => void): () => void {
    this.listeners.push(listener);
    listener([...this.messages]);
    return () => {
      this.listeners = this.listeners.filter((l) => l !== listener);
    };
  }

  show(
    title: string,
    message: string,
    type: ToastMessage['type'] = 'info',
    durationMs = 5000
  ): void {
    const toast: ToastMessage = {
      id: crypto.randomUUID(),
      title,
      message,
      type
    };
    this.messages.push(toast);
    this.emit();

    setTimeout(() => this.dismiss(toast.id), durationMs);
  }

  showSuccess(message: string, title = 'Success', durationMs = 5000): void {
    this.show(title, message, 'success', durationMs);
  }

  showError(message: string, title = 'Error', durationMs = 5000): void {
    this.show(title, message, 'error', durationMs);
  }

  dismiss(id: string): void {
    this.messages = this.messages.filter((m) => m.id !== id);
    this.emit();
  }
}
