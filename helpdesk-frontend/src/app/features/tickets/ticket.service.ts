import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { forkJoin, map, Observable } from 'rxjs';
import { apiConfig } from '../../core/api.config';

export interface Ticket {
  id: number;
  title: string;
  description: string;
  status: number;
  priority: number;
  requesterId: number;
  agentId: number | null;
  createdAt: string;
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: number;
}

export interface CreateCommentRequest {
  content: string;
  isInternal: boolean;
}

export interface UpdateTicketStatusRequest {
  status: number;
}

export interface AssignTicketRequest {
  agentId: number;
}

export interface Comment {
  id: number;
  ticketId: number;
  authorId: number;
  content: string;
  isInternal: boolean;
  createdAt: string;
}

export interface AuditTrailEntry {
  id: number;
  ticketId: number;
  actorId: number;
  action: string;
  description: string;
  createdAt: string;
}

export interface Attachment {
  id: number;
  ticketId: number;
  uploadedByUserId: number;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly http = inject(HttpClient);

  getTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${apiConfig.baseUrl}/Tickets`);
  }

  createTicket(payload: CreateTicketRequest): Observable<Ticket> {
    return this.http.post<Ticket>(`${apiConfig.baseUrl}/Tickets`, payload);
  }

  getTicketById(id: number): Observable<Ticket> {
    return this.http.get<Ticket>(`${apiConfig.baseUrl}/Tickets/${id}`);
  }

  getComments(ticketId: number): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${apiConfig.baseUrl}/tickets/${ticketId}/comments`);
  }

  getAuditTrail(ticketId: number): Observable<AuditTrailEntry[]> {
    return this.http.get<AuditTrailEntry[]>(`${apiConfig.baseUrl}/tickets/${ticketId}/audit-trail`);
  }

  getAttachments(ticketId: number): Observable<Attachment[]> {
    return this.http.get<Attachment[]>(`${apiConfig.baseUrl}/tickets/${ticketId}/attachments`);
  }

  createComment(ticketId: number, payload: CreateCommentRequest): Observable<Comment> {
    return this.http.post<Comment>(`${apiConfig.baseUrl}/tickets/${ticketId}/comments`, payload);
  }

  uploadAttachment(ticketId: number, file: File): Observable<Attachment> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<Attachment>(`${apiConfig.baseUrl}/tickets/${ticketId}/attachments`, formData);
  }

  updateTicketStatus(ticketId: number, payload: UpdateTicketStatusRequest): Observable<void> {
    return this.http.put<void>(`${apiConfig.baseUrl}/Tickets/${ticketId}/status`, payload);
  }

  assignTicket(ticketId: number, payload: AssignTicketRequest): Observable<void> {
    return this.http.put<void>(`${apiConfig.baseUrl}/Tickets/${ticketId}/assign`, payload);
  }

  getTicketDetails(ticketId: number): Observable<{
    ticket: Ticket;
    comments: Comment[];
    auditTrail: AuditTrailEntry[];
    attachments: Attachment[];
  }> {
    return forkJoin({
      ticket: this.getTicketById(ticketId),
      comments: this.getComments(ticketId),
      auditTrail: this.getAuditTrail(ticketId),
      attachments: this.getAttachments(ticketId),
    }).pipe(map((response) => response));
  }
}
