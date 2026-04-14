import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { CreateTicketRequest, TicketService } from './ticket.service';

@Component({
  selector: 'app-create-ticket-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './create-ticket-page.component.html',
  styleUrl: './create-ticket-page.component.css',
})
export class CreateTicketPageComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly ticketService = inject(TicketService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal('');
  protected readonly role = this.authService.getRole();
  protected readonly email = this.authService.getEmail();

  protected readonly ticketForm = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
    description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]],
    priority: [1, [Validators.required]],
  });

  protected submit(): void {
    if (this.ticketForm.invalid) {
      this.ticketForm.markAllAsTouched();
      return;
    }

    this.errorMessage.set('');
    this.isSubmitting.set(true);

    const payload = this.ticketForm.getRawValue() as CreateTicketRequest;

    this.ticketService
      .createTicket(payload)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          void this.router.navigateByUrl('/tickets');
        },
        error: (error) => {
          const message =
            this.extractValidationMessage(error?.error?.errors) ??
            error?.error?.message ??
            error?.message ??
            'We could not create the ticket right now.';
          this.errorMessage.set(message);
        },
      });
  }

  private extractValidationMessage(
    errors: Record<string, string[]> | undefined
  ): string | null {
    if (!errors) {
      return null;
    }

    const messages = Object.entries(errors)
      .flatMap(([field, fieldErrors]) =>
        fieldErrors.map((fieldError) => `${field}: ${fieldError}`)
      );

    return messages.length > 0 ? messages.join(' ') : null;
  }
}
