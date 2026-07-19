import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HubConnection, HubConnectionBuilder, IHttpConnectionOptions } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { AuthTokenService } from './auth-token.service';
import { environment } from '../../../environments/environment';
import {
  AuctionEndedEvent,
  AuctionStartedEvent,
  BidPlacedEvent,
  CountdownSync,
  Notification,
  OutbidAlertEvent
} from '../models/notification.model';

export interface ConnectionState {
  status: 'disconnected' | 'connecting' | 'connected' | 'reconnecting';
  error?: string;
}

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection: HubConnection | null = null;
  private readonly _connectionState = new BehaviorSubject<ConnectionState>({ status: 'disconnected' });
  private readonly _bidPlaced = new BehaviorSubject<BidPlacedEvent[]>([]);
  private readonly _outbid = new BehaviorSubject<OutbidAlertEvent | null>(null);
  private readonly _auctionStarted = new BehaviorSubject<AuctionStartedEvent | null>(null);
  private readonly _auctionEnded = new BehaviorSubject<AuctionEndedEvent | null>(null);
  private readonly _countdownSync = new BehaviorSubject<CountdownSync | null>(null);
  private readonly _notification = new BehaviorSubject<Notification | null>(null);

  private reconnectAttempts = 0;
  private readonly maxReconnectDelayMs = 30_000;
  private reconnectTimer: any;
  private joinedAuctions = new Set<string>();

  public readonly connectionState$: Observable<ConnectionState> = this._connectionState.asObservable();
  public readonly bidPlaced$: Observable<BidPlacedEvent[]> = this._bidPlaced.asObservable();
  public readonly outbid$: Observable<OutbidAlertEvent | null> = this._outbid.asObservable();
  public readonly auctionStarted$: Observable<AuctionStartedEvent | null> = this._auctionStarted.asObservable();
  public readonly auctionEnded$: Observable<AuctionEndedEvent | null> = this._auctionEnded.asObservable();
  public readonly countdownSync$: Observable<CountdownSync | null> = this._countdownSync.asObservable();
  public readonly notification$: Observable<Notification | null> = this._notification.asObservable();

  constructor(private readonly tokenService: AuthTokenService) {
    this.buildConnection();
  }

  private buildConnection(): void {
    const hubUrl = `${environment.apiUrl.replace('/api', '')}/hubs/auction`;
    const token = this.tokenService.getToken();

    const options: IHttpConnectionOptions = {
      accessTokenFactory: () => token ?? '',
      transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents,
      withCredentials: true
    };

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(hubUrl, options)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 15000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.registerEvents();
  }

  private registerEvents(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('BidPlaced', (event: BidPlacedEvent) => {
      const current = this._bidPlaced.value;
      this._bidPlaced.next([event, ...current].slice(0, 50));
    });

    this.hubConnection.on('AuctionStarted', (event: AuctionStartedEvent) => {
      this._auctionStarted.next(event);
    });

    this.hubConnection.on('AuctionEnded', (event: AuctionEndedEvent) => {
      this._auctionEnded.next(event);
    });

    this.hubConnection.on('OutbidAlert', (event: OutbidAlertEvent) => {
      this._outbid.next(event);
    });

    this.hubConnection.on('CountdownSync', (event: CountdownSync) => {
      this._countdownSync.next(event);
    });

    this.hubConnection.on('NotificationReceived', (event: Notification) => {
      this._notification.next(event);
    });

    this.hubConnection.onreconnecting((error) => {
      this._connectionState.next({ status: 'reconnecting', error: error?.message });
    });

    this.hubConnection.onreconnected(() => {
      this._connectionState.next({ status: 'connected' });
      this.rejoinAuctionGroups();
      this.reconnectAttempts = 0;
    });

    this.hubConnection.onclose((error) => {
      this._connectionState.next({ status: 'disconnected', error: error?.message });
      this.scheduleReconnect();
    });
  }

  async start(): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Disconnected) {
      return;
    }

    this._connectionState.next({ status: 'connecting' });

    try {
      await this.hubConnection.start();
      this._connectionState.next({ status: 'connected' });
      this.reconnectAttempts = 0;
      this.rejoinAuctionGroups();
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unknown error';
      this._connectionState.next({ status: 'disconnected', error: message });
      this.scheduleReconnect();
    }
  }

  async stop(): Promise<void> {
    this.clearReconnectTimer();
    if (!this.hubConnection) return;
    await this.hubConnection.stop();
    this.joinedAuctions.clear();
  }

  async joinAuctionGroup(auctionId: string): Promise<void> {
    if (!this.hubConnection) return;
    await this.hubConnection.invoke('JoinAuctionGroup', auctionId);
    this.joinedAuctions.add(auctionId);
  }

  async leaveAuctionGroup(auctionId: string): Promise<void> {
    if (!this.hubConnection) return;
    await this.hubConnection.invoke('LeaveAuctionGroup', auctionId);
    this.joinedAuctions.delete(auctionId);
  }

  private rejoinAuctionGroups(): void {
    this.joinedAuctions.forEach((auctionId) => {
      this.hubConnection?.invoke('JoinAuctionGroup', auctionId).catch(() => {
        this.joinedAuctions.delete(auctionId);
      });
    });
  }

  private scheduleReconnect(): void {
    if (this.reconnectTimer) return;

    const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), this.maxReconnectDelayMs);
    this.reconnectAttempts++;

    this.reconnectTimer = setTimeout(() => {
      this.reconnectTimer = null;
      this.buildConnection();
      this.start();
    }, delay);
  }

  private clearReconnectTimer(): void {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }
  }
}
