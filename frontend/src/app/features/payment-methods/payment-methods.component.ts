import { Component, OnInit, signal, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { PaymentMethodService } from '../../core/services/payment-method.service';
import { AddPaymentMethodRequest, PaymentMethodDto } from '../../core/models/api.models';

const PAYMENT_TYPES = ['CreditCard', 'DebitCard', 'BankTransfer', 'DigitalWallet'];

@Component({
  selector: 'app-payment-methods',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './payment-methods.component.html'
})
export class PaymentMethodsComponent implements OnInit {
  private readonly paymentMethodService = inject(PaymentMethodService);
  private readonly fb = inject(FormBuilder);

  readonly paymentMethods = signal<PaymentMethodDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly showAddForm = signal(false);
  readonly paymentTypes = PAYMENT_TYPES;

  readonly addForm = this.fb.nonNullable.group({
    type: ['CreditCard', Validators.required],
    provider: ['', [Validators.required, Validators.maxLength(100)]],
    token: ['', [Validators.required, Validators.maxLength(500)]],
    lastFour: ['', Validators.pattern(/^[0-9]{4}$/)],
    expiryMonth: ['', Validators.pattern(/^(0[1-9]|1[0-2])$/)],
    expiryYear: ['', Validators.pattern(/^[0-9]{4}$/)],
    isDefault: [false]
  });

  ngOnInit(): void {
    this.loadPaymentMethods();
  }

  loadPaymentMethods(): void {
    this.loading.set(true);
    this.paymentMethodService.getAll().subscribe({
      next: (methods: PaymentMethodDto[]) => {
        this.paymentMethods.set(methods);
        this.loading.set(false);
      },
      error: (err: { error?: { error?: string } }) => {
        this.error.set(err?.error?.error ?? 'Failed to load payment methods.');
        this.loading.set(false);
      }
    });
  }

  addPaymentMethod(): void {
    if (this.addForm.invalid) return;

    const raw = this.addForm.getRawValue();
    const request: AddPaymentMethodRequest = {
      type: raw.type,
      provider: raw.provider,
      token: raw.token,
      isDefault: raw.isDefault,
      lastFour: raw.lastFour || undefined,
      expiryMonth: raw.expiryMonth || undefined,
      expiryYear: raw.expiryYear || undefined
    };

    this.paymentMethodService.add(request).subscribe({
      next: () => {
        this.addForm.reset({ type: 'CreditCard', isDefault: false });
        this.showAddForm.set(false);
        this.loadPaymentMethods();
      },
      error: (err: { error?: { error?: string } }) =>
        this.error.set(err?.error?.error ?? 'Failed to add payment method.')
    });
  }

  setDefault(id: string): void {
    this.paymentMethodService.setDefault(id).subscribe({
      next: () => this.loadPaymentMethods(),
      error: (err: { error?: { error?: string } }) =>
        this.error.set(err?.error?.error ?? 'Failed to set default.')
    });
  }

  remove(id: string): void {
    this.paymentMethodService.deactivate(id).subscribe({
      next: () => this.loadPaymentMethods(),
      error: (err: { error?: { error?: string } }) =>
        this.error.set(err?.error?.error ?? 'Failed to remove payment method.')
    });
  }
}

