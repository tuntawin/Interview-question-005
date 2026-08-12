import { Component, signal, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface QueueGenerateResponse {
  queueCode: string;
  currentIndex: number;
  generatedAt: string;
}

export interface QueueResetResponse {
  message: string;
  currentIndex: number;
}

export interface QueueCurrentResponse {
  queueCode: string;
  currentIndex: number;
  lastActive?: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager
})
export class AppComponent implements OnInit {
  private readonly http = inject(HttpClient);

  // API base endpoint
  private readonly apiUrl = '/api/queue';

  // Screen Signals (1: IT 05-1, 2: IT 05-2, 3: IT 05-3)
  readonly currentScreen = signal<1 | 2 | 3>(1);
  readonly queueCode = signal<string>('');
  readonly currentQueueCode = signal<string>('00');
  readonly formattedDate = signal<string>('');
  readonly formattedTime = signal<string>('');
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchCurrentQueue();
  }

  /**
   * Fetches the current active queue status from backend
   */
  fetchCurrentQueue(): void {
    this.http.get<QueueCurrentResponse>(`${this.apiUrl}/current`).subscribe({
      next: (res) => {
        this.currentQueueCode.set(res.queueCode || '00');
      },
      error: (err) => {
        console.error('Failed to fetch current queue status:', err);
      }
    });
  }

  /**
   * Action for Screen 1: Generate next queue ticket (POST /api/queue/generate)
   */
  generateQueue(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.http.post<QueueGenerateResponse>(`${this.apiUrl}/generate`, {}).subscribe({
      next: (response) => {
        this.queueCode.set(response.queueCode);
        this.currentQueueCode.set(response.queueCode);

        // Format Date and Time in Thai format: วันที่ : DD/MM/YYYY เวลา HH:mm น.
        const d = response.generatedAt ? new Date(response.generatedAt) : new Date();
        const day = String(d.getDate()).padStart(2, '0');
        const month = String(d.getMonth() + 1).padStart(2, '0');
        const year = d.getFullYear();
        const hours = String(d.getHours()).padStart(2, '0');
        const minutes = String(d.getMinutes()).padStart(2, '0');

        this.formattedDate.set(`${day}/${month}/${year}`);
        this.formattedTime.set(`${hours}:${minutes}`);

        this.currentScreen.set(2);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to generate queue:', err);
        this.errorMessage.set('เกิดข้อผิดพลาด ไม่สามารถรับบัตรคิวได้');
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Navigate to Screen 3 (IT 05-3)
   */
  goToScreen3(): void {
    this.fetchCurrentQueue();
    this.currentScreen.set(3);
  }

  /**
   * Action for Screen 3: Reset queue sequence (POST /api/queue/reset)
   */
  resetQueue(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.http.post<QueueResetResponse>(`${this.apiUrl}/reset`, {}).subscribe({
      next: () => {
        this.queueCode.set('00');
        this.currentQueueCode.set('00');
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to reset queue:', err);
        this.errorMessage.set('เกิดข้อผิดพลาด ไม่สามารถล้างคิวได้');
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Action for Screen 2 & 3: Return to Main Ticket Screen (Screen 1 / IT 05-1)
   */
  goToScreen1(): void {
    this.errorMessage.set(null);
    this.currentScreen.set(1);
  }
}
