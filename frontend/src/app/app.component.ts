import { Component, signal, inject, ChangeDetectionStrategy } from '@angular/core';

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

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [],
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  private readonly http = inject(HttpClient);

  // API base URL endpoint
  private readonly apiUrl = '/api/queue';

  // State managed via Angular Signals
  readonly currentScreen = signal<1 | 2 | 3>(1);
  readonly queueCode = signal<string>('');
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  /**
   * Action for Screen 1: Generate next queue ticket code (POST /api/queue/generate)
   */
  generateQueue(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.http.post<QueueGenerateResponse>(`${this.apiUrl}/generate`, {}).subscribe({
      next: (response) => {
        this.queueCode.set(response.queueCode);
        this.currentScreen.set(2);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to generate queue:', err);
        this.errorMessage.set('An error occurred. Unable to generate queue ticket.');
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Action for Screen 1: Reset queue sequence (POST /api/queue/reset)
   */
  resetQueue(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.http.post<QueueResetResponse>(`${this.apiUrl}/reset`, {}).subscribe({
      next: () => {
        this.queueCode.set('');
        this.currentScreen.set(3);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to reset queue:', err);
        this.errorMessage.set('An error occurred. Unable to reset queue.');
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Action for Screen 2 & 3: Navigate back to main ticket screen (Screen 1)
   */
  goToScreen1(): void {
    this.queueCode.set('');
    this.errorMessage.set(null);
    this.currentScreen.set(1);
  }
}
