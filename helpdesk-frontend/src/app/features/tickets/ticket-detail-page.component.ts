import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { NotificationService } from '../notifications/notification.service';
import {
  Attachment,
  AssignTicketRequest,
  AuditTrailEntry,
  Comment,
  CreateCommentRequest,
  Ticket,
  TicketService,
  UpdateTicketStatusRequest,
} from './ticket.service';

@Component({
  selector: 'app-ticket-detail-page',
  imports: [CommonModule, RouterLink, DatePipe, ReactiveFormsModule],
  templateUrl: './ticket-detail-page.component.html',
  styleUrl: './ticket-detail-page.component.css',
})
export class TicketDetailPageComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly ticketService = inject(TicketService);

  protected readonly ticket = signal<Ticket | null>(null);
  protected readonly comments = signal<Comment[]>([]);
  protected readonly auditTrail = signal<AuditTrailEntry[]>([]);
  protected readonly attachments = signal<Attachment[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal('');
  protected readonly commentErrorMessage = signal('');
  protected readonly attachmentErrorMessage = signal('');
  protected readonly actionErrorMessage = signal('');
  protected readonly isSubmittingComment = signal(false);
  protected readonly isUploadingAttachment = signal(false);
  protected readonly isSubmittingStatus = signal(false);
  protected readonly isSubmittingAssignment = signal(false);
  protected readonly selectedFileName = signal('');
  protected readonly email = this.authService.getEmail();
  protected readonly role = this.authService.getRole();
  protected readonly unreadCount = this.notificationService.unreadCount;
  protected readonly canCreateInternalComment = this.role === 'Admin' || this.role === 'Agent';
  protected readonly canUpdateStatus = this.role === 'Admin' || this.role === 'Agent';
  protected readonly canAssignTicket = this.role === 'Admin';

  protected readonly commentForm = this.formBuilder.nonNullable.group({
    content: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(1000)]],
    isInternal: [false],
  });

  protected readonly statusForm = this.formBuilder.nonNullable.group({
    status: [2, [Validators.required]],
  });

  protected readonly assignmentForm = this.formBuilder.nonNullable.group({
    agentId: [0, [Validators.required, Validators.min(1)]],
  });

  constructor() {
    this.notificationService.loadUnreadCount().subscribe({
      error: () => undefined,
    });
    this.loadTicketDetails();
  }

  protected reload(): void {
    this.loadTicketDetails();
  }

  protected logout(): void {
    this.authService.logout();
    void this.router.navigateByUrl('/login');
  }

  protected getStatusLabel(status: number): string {
    return ['New', 'Assigned', 'In Progress', 'Resolved', 'Closed'][status] ?? 'Unknown';
  }

  protected getPriorityLabel(priority: number): string {
    return ['Low', 'Medium', 'High'][priority] ?? 'Unknown';
  }

  protected submitStatusUpdate(): void {
    if (this.statusForm.invalid) {
      this.statusForm.markAllAsTouched();
      return;
    }

    const ticketId = this.getTicketId();
    if (!ticketId) {
      this.actionErrorMessage.set('Ticket id is invalid.');
      return;
    }

    this.actionErrorMessage.set('');
    this.isSubmittingStatus.set(true);

    const payload: UpdateTicketStatusRequest = this.statusForm.getRawValue();

    this.ticketService
      .updateTicketStatus(ticketId, payload)
      .pipe(finalize(() => this.isSubmittingStatus.set(false)))
      .subscribe({
        next: () => this.loadTicketDetails(),
        error: (error) => {
          const message =
            this.extractValidationMessage(error?.error?.errors) ??
            error?.error?.detail ??
            error?.error?.message ??
            error?.message ??
            'We could not update the ticket status right now.';
          this.actionErrorMessage.set(message);
        },
      });
  }

  protected submitAssignment(): void {
    if (this.assignmentForm.invalid) {
      this.assignmentForm.markAllAsTouched();
      return;
    }

    const ticketId = this.getTicketId();
    if (!ticketId) {
      this.actionErrorMessage.set('Ticket id is invalid.');
      return;
    }

    this.actionErrorMessage.set('');
    this.isSubmittingAssignment.set(true);

    const payload: AssignTicketRequest = this.assignmentForm.getRawValue();

    this.ticketService
      .assignTicket(ticketId, payload)
      .pipe(finalize(() => this.isSubmittingAssignment.set(false)))
      .subscribe({
        next: () => this.loadTicketDetails(),
        error: (error) => {
          const message =
            this.extractValidationMessage(error?.error?.errors) ??
            error?.error?.detail ??
            error?.error?.message ??
            error?.message ??
            'We could not assign the ticket right now.';
          this.actionErrorMessage.set(message);
        },
      });
  }

  protected submitComment(): void {
    if (this.commentForm.invalid) {
      this.commentForm.markAllAsTouched();
      return;
    }

    const ticketId = this.getTicketId();
    if (!ticketId) {
      this.commentErrorMessage.set('Ticket id is invalid.');
      return;
    }

    this.commentErrorMessage.set('');
    this.isSubmittingComment.set(true);

    const rawValue = this.commentForm.getRawValue();
    const payload: CreateCommentRequest = {
      content: rawValue.content,
      isInternal: this.canCreateInternalComment ? rawValue.isInternal : false,
    };

    this.ticketService
      .createComment(ticketId, payload)
      .pipe(finalize(() => this.isSubmittingComment.set(false)))
      .subscribe({
        next: () => {
          this.commentForm.reset({
            content: '',
            isInternal: false,
          });
          this.loadTicketDetails();
        },
        error: (error) => {
          const message =
            this.extractValidationMessage(error?.error?.errors) ??
            error?.error?.message ??
            error?.message ??
            'We could not add the comment right now.';
          this.commentErrorMessage.set(message);
        },
      });
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    this.selectedFileName.set(file?.name ?? '');
  }

  protected uploadAttachment(fileInput: HTMLInputElement): void {
    const ticketId = this.getTicketId();
    const file = fileInput.files?.[0];

    if (!ticketId) {
      this.attachmentErrorMessage.set('Ticket id is invalid.');
      return;
    }

    if (!file) {
      this.attachmentErrorMessage.set('Please choose a file first.');
      return;
    }

    this.attachmentErrorMessage.set('');
    this.isUploadingAttachment.set(true);

    this.ticketService
      .uploadAttachment(ticketId, file)
      .pipe(finalize(() => this.isUploadingAttachment.set(false)))
      .subscribe({
        next: () => {
          fileInput.value = '';
          this.selectedFileName.set('');
          this.loadTicketDetails();
        },
        error: (error) => {
          const message =
            this.extractValidationMessage(error?.error?.errors) ??
            error?.error?.message ??
            error?.message ??
            'We could not upload the attachment right now.';
          this.attachmentErrorMessage.set(message);
        },
      });
  }

  private syncActionForms(ticket: Ticket): void {
    this.statusForm.setValue({
      status: this.getNextStatusValue(ticket.status),
    });

    this.assignmentForm.setValue({
      agentId: ticket.agentId ?? 0,
    });
  }

  private getNextStatusValue(currentStatus: number): number {
    if (currentStatus >= 1 && currentStatus < 4) {
      return currentStatus + 1;
    }

    if (currentStatus === 0) {
      return 1;
    }

    return currentStatus;
  }

  private getTicketId(): number | null {
    const ticketId = Number(this.route.snapshot.paramMap.get('id'));
    return ticketId || null;
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

  private loadTicketDetails(): void {
    const ticketId = this.getTicketId();

    if (!ticketId) {
      this.errorMessage.set('Ticket id is invalid.');
      this.isLoading.set(false);
      return;
    }

    this.errorMessage.set('');
    this.isLoading.set(true);

    this.ticketService
      .getTicketDetails(ticketId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: ({ ticket, comments, auditTrail, attachments }) => {
          this.ticket.set(ticket);
          this.comments.set(comments);
          this.auditTrail.set(auditTrail);
          this.attachments.set(attachments);
          this.actionErrorMessage.set('');
          this.syncActionForms(ticket);
        },
        error: (error) => {
          const message =
            error?.error?.message ??
            error?.message ??
            'We could not load the ticket details right now.';
          this.errorMessage.set(message);
        },
      });
  }
}
