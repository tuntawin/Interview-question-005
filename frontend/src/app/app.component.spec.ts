import { ComponentFixture, TestBed } from "@angular/core/testing";
import {
  provideHttpClientTesting,
  HttpTestingController,
} from "@angular/common/http/testing";
import { provideHttpClient } from "@angular/common/http";
import { provideZonelessChangeDetection } from "@angular/core";
import {
  AppComponent,
  QueueGenerateResponse,
  QueueResetResponse,
} from "./app.component";

describe("AppComponent", () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it("should create the app component", () => {
    fixture.detectChanges();
    const req = httpMock.expectOne("/api/queue/current");
    expect(req.request.method).toBe("GET");
    req.flush({ queueCode: "00", currentIndex: -1 });

    expect(component).toBeTruthy();
  });

  it("should initialize with Screen 1 (IT 05-1) and fetch current queue", () => {
    fixture.detectChanges();
    const req = httpMock.expectOne("/api/queue/current");
    req.flush({ queueCode: "A3", currentIndex: 3 });

    expect(component.currentScreen()).toBe(1);
    expect(component.currentQueueCode()).toBe("A3");

    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector(".screen-title")?.textContent).toContain(
      "IT 05-1",
    );
    expect(compiled.querySelector(".btn-get-ticket")?.textContent).toContain(
      "รับบัตรคิว",
    );
  });

  it("should generate queue ticket and navigate to Screen 2 (IT 05-2)", () => {
    fixture.detectChanges();
    const currentReq = httpMock.expectOne("/api/queue/current");
    currentReq.flush({ queueCode: "00", currentIndex: -1 });

    const mockResponse: QueueGenerateResponse = {
      queueCode: "A0",
      currentIndex: 0,
      generatedAt: "2026-08-12T15:00:00.000Z",
    };

    component.generateQueue();
    expect(component.isLoading()).toBeTrue();

    const genReq = httpMock.expectOne("/api/queue/generate");
    expect(genReq.request.method).toBe("POST");
    genReq.flush(mockResponse);

    fixture.detectChanges();

    expect(component.currentScreen()).toBe(2);
    expect(component.queueCode()).toBe("A0");
    expect(component.currentQueueCode()).toBe("A0");
    expect(component.isLoading()).toBeFalse();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector(".screen-title")?.textContent).toContain(
      "IT 05-2",
    );
    expect(
      compiled.querySelector(".queue-code-display")?.textContent,
    ).toContain("A0");
    expect(compiled.querySelector(".timestamp-display")?.textContent).toContain(
      "วันที่ :",
    );
  });

  it("should handle error when generate queue fails", () => {
    fixture.detectChanges();
    const currentReq = httpMock.expectOne("/api/queue/current");
    currentReq.flush({ queueCode: "00", currentIndex: -1 });

    component.generateQueue();

    const genReq = httpMock.expectOne("/api/queue/generate");
    genReq.flush("Server error", {
      status: 500,
      statusText: "Internal Server Error",
    });

    fixture.detectChanges();

    expect(component.isLoading()).toBeFalse();
    expect(component.errorMessage()).toBe(
      "เกิดข้อผิดพลาด ไม่สามารถรับบัตรคิวได้",
    );
  });

  it("should navigate to Screen 3 (IT 05-3) on goToScreen3()", () => {
    fixture.detectChanges();
    const initReq = httpMock.expectOne("/api/queue/current");
    initReq.flush({ queueCode: "A5", currentIndex: 5 });

    component.goToScreen3();

    const screen3Req = httpMock.expectOne("/api/queue/current");
    screen3Req.flush({ queueCode: "A5", currentIndex: 5 });

    fixture.detectChanges();

    expect(component.currentScreen()).toBe(3);
    expect(component.currentQueueCode()).toBe("A5");

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector(".screen-title")?.textContent).toContain(
      "IT 05-3",
    );
    expect(compiled.querySelector(".btn-reset-top")?.textContent).toContain(
      "ล้างคิว",
    );
  });

  it("should reset queue and update queue code to 00 on resetQueue()", () => {
    fixture.detectChanges();
    const initReq = httpMock.expectOne("/api/queue/current");
    initReq.flush({ queueCode: "A5", currentIndex: 5 });

    component.goToScreen3();
    const screen3Req = httpMock.expectOne("/api/queue/current");
    screen3Req.flush({ queueCode: "A5", currentIndex: 5 });

    const mockResetResponse: QueueResetResponse = {
      message: "Queue has been reset successfully.",
      currentIndex: -1,
    };

    component.resetQueue();
    expect(component.isLoading()).toBeTrue();

    const resetReq = httpMock.expectOne("/api/queue/reset");
    expect(resetReq.request.method).toBe("POST");
    resetReq.flush(mockResetResponse);

    fixture.detectChanges();

    expect(component.queueCode()).toBe("00");
    expect(component.currentQueueCode()).toBe("00");
    expect(component.isLoading()).toBeFalse();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(
      compiled.querySelector(".queue-code-display")?.textContent,
    ).toContain("00");
  });

  it("should navigate back to Screen 1 on goToScreen1()", () => {
    component.currentScreen.set(2);
    component.goToScreen1();

    expect(component.currentScreen()).toBe(1);
    expect(component.errorMessage()).toBeNull();
  });
});
