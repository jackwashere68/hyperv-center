import {
  Component,
  OnInit,
  OnDestroy,
  ViewChild,
  ElementRef,
  signal,
  computed,
  inject,
  HostListener,
  AfterViewInit,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import Guacamole from 'guacamole-common-js';

@Component({
  selector: 'app-vm-console',
  templateUrl: './vm-console.component.html',
  imports: [
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
  ],
  host: {
    class: 'flex flex-col h-full',
  },
})
export class VmConsoleComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('displayContainer', { static: true }) displayContainer!: ElementRef<HTMLDivElement>;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly vmId = signal('');
  readonly vmName = signal('Virtual Machine');
  readonly connectionState = signal(0); // 0=idle, 1=connecting, 2=waiting, 3=connected, 4=disconnecting, 5=disconnected
  readonly errorMessage = signal<string | null>(null);
  readonly isFullscreen = signal(false);
  readonly debugMode = signal(false);

  private client: Guacamole.Client | null = null;
  private tunnel: Guacamole.WebSocketTunnel | null = null;
  private keyboard: Guacamole.Keyboard | null = null;
  private mouse: Guacamole.Mouse | null = null;
  private resizeObserver: ResizeObserver | null = null;
  private debugOverlay: HTMLDivElement | null = null;
  private debugStats = { imgCount: 0, blobCount: 0, endCount: 0, syncCount: 0, blobBytes: 0, sizeInfo: '', lastImgInfo: '' };
  private keyDownCount = 0;
  private keyUpCount = 0;
  private rawKeyCount = 0;
  private rawKeyDebugHandler: ((e: KeyboardEvent) => void) | null = null;

  readonly connectionStatusClass = computed(() => {
    const state = this.connectionState();
    if (state === 3) return 'bg-success animate-pulse-dot';
    if (state >= 1 && state <= 2) return 'bg-warning animate-pulse-dot';
    if (state === 5 && this.errorMessage()) return 'bg-error';
    return 'bg-text-muted';
  });

  readonly connectionStatusText = computed(() => {
    switch (this.connectionState()) {
      case 0: return 'Idle';
      case 1: return 'Connecting...';
      case 2: return 'Waiting...';
      case 3: return 'Connected';
      case 4: return 'Disconnecting...';
      case 5: return this.errorMessage() ? 'Error' : 'Disconnected';
      default: return 'Unknown';
    }
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.vmId.set(id);

    const name = this.route.snapshot.queryParamMap.get('name');
    if (name) this.vmName.set(name);
  }

  ngAfterViewInit(): void {
    this.connect();
  }

  ngOnDestroy(): void {
    this.cleanup();
  }

  connect(): void {
    this.cleanup();
    this.errorMessage.set(null);
    this.connectionState.set(1);

    // CRITICAL FIX: Wrap createImageBitmap to add error recovery.
    // guacamole-common-js v1.5 calls createImageBitmap(blob).then(cb) with NO .catch().
    // If the promise rejects (corrupted image data), the display task is never unblocked,
    // permanently stalling the frame queue — nothing renders after that point.
    if (window.createImageBitmap && !(window as any).__guacBitmapPatched) {
      const orig: typeof window.createImageBitmap = window.createImageBitmap.bind(window);
      window.createImageBitmap = ((source: ImageBitmapSource, ...rest: any[]) => {
        const promise = rest.length > 0
          ? orig(source, ...rest as [number, number, number, number])
          : orig(source);
        return promise.catch((err: any) => {
          console.warn('[guac] createImageBitmap failed, returning fallback:', err);
          // Return a 1x1 transparent bitmap so the task unblocks
          return orig(new ImageData(1, 1));
        });
      }) as typeof window.createImageBitmap;
      (window as any).__guacBitmapPatched = true;
    }

    // Build WebSocket URL
    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    const wsUrl = `${protocol}//${window.location.host}/api/virtualmachines/${this.vmId()}/console`;

    this.tunnel = new Guacamole.WebSocketTunnel(wsUrl);
    this.client = new Guacamole.Client(this.tunnel);

    // Intercept instructions to gather debug stats (always counted, only displayed when debug is on)
    this.debugStats = { imgCount: 0, blobCount: 0, endCount: 0, syncCount: 0, blobBytes: 0, sizeInfo: '', lastImgInfo: '' };
    const clientHandler = this.tunnel.oninstruction;
    this.tunnel.oninstruction = (opcode: string, args: string[]) => {
      if (opcode === 'img') {
        this.debugStats.imgCount++;
        this.debugStats.lastImgInfo = `img#${this.debugStats.imgCount} s=${args[0]} L=${args[2]} ${args[3]} ${args[4]},${args[5]}`;
      } else if (opcode === 'blob') {
        this.debugStats.blobCount++;
        this.debugStats.blobBytes += (args[1]?.length ?? 0);
      } else if (opcode === 'end') {
        this.debugStats.endCount++;
      } else if (opcode === 'size') {
        this.debugStats.sizeInfo = `size: L${args[0]} ${args[1]}x${args[2]}`;
      } else if (opcode === 'sync') {
        this.debugStats.syncCount++;
        this.updateDebugOverlay();
      }
      clientHandler?.call(this.tunnel, opcode, args);
    };

    // Re-fit display whenever the remote display resizes (e.g. size instruction arrives)
    const display = this.client.getDisplay();
    (display as any).onresize = () => {
      if (this.connectionState() === 3) {
        this.fitDisplay();
      }
    };

    // State change handler
    this.client.onstatechange = (state: number) => {
      this.connectionState.set(state);

      if (state === 3) {
        // Connected - set up input
        this.setupInput();
        this.fitDisplay();
      }
    };

    // Error handler
    this.client.onerror = (status: Guacamole.Status) => {
      const msg = status.message || `Error code: ${status.code}`;
      this.errorMessage.set(msg);
      this.connectionState.set(5);
    };

    this.tunnel.onerror = (status: Guacamole.Status) => {
      const msg = status.message || `Tunnel error: ${status.code}`;
      this.errorMessage.set(msg);
      this.connectionState.set(5);
    };

    // Name handler - update VM name from server
    this.client.onname = (name: string) => {
      if (name) this.vmName.set(name);
    };

    // Clipboard handler
    this.client.onclipboard = (stream: Guacamole.InputStream, mimetype: string) => {
      if (mimetype === 'text/plain') {
        const reader = new Guacamole.StringReader(stream);
        let clipboardText = '';
        reader.ontext = (text: string) => { clipboardText += text; };
        reader.onend = () => {
          navigator.clipboard.writeText(clipboardText).catch(() => {
            // Clipboard access denied - silently ignore
          });
        };
      }
    };

    // Add display element to DOM
    const displayElement = this.client.getDisplay().getElement();
    this.displayContainer.nativeElement.appendChild(displayElement);

    // Set up display resize observer
    this.resizeObserver = new ResizeObserver(() => {
      if (this.connectionState() === 3) {
        this.fitDisplay();
      }
    });
    this.resizeObserver.observe(this.displayContainer.nativeElement);

    // Connect
    this.client.connect('');
  }

  disconnect(): void {
    this.client?.disconnect();
    this.connectionState.set(5);
  }

  sendCtrlAltDel(): void {
    if (!this.client) return;
    // Key codes for Ctrl+Alt+Del
    const CTRL = 0xFFE3;
    const ALT = 0xFFE9;
    const DEL = 0xFFFF;

    this.client.sendKeyEvent(1 as any, CTRL);
    this.client.sendKeyEvent(1 as any, ALT);
    this.client.sendKeyEvent(1 as any, DEL);
    this.client.sendKeyEvent(0 as any, DEL);
    this.client.sendKeyEvent(0 as any, ALT);
    this.client.sendKeyEvent(0 as any, CTRL);
  }

  toggleFullscreen(): void {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen().then(() => {
        this.isFullscreen.set(true);
      });
    } else {
      document.exitFullscreen().then(() => {
        this.isFullscreen.set(false);
      });
    }
  }

  @HostListener('document:fullscreenchange')
  onFullscreenChange(): void {
    this.isFullscreen.set(!!document.fullscreenElement);
    // Re-fit display after fullscreen change
    setTimeout(() => this.fitDisplay(), 100);
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    if (this.connectionState() === 3) {
      this.fitDisplay();
    }
  }

  goBack(): void {
    this.router.navigate(['/virtual-machines']);
  }

  toggleDebug(): void {
    const on = !this.debugMode();
    this.debugMode.set(on);
    if (on) {
      const el = document.createElement('div');
      el.style.cssText = 'position:fixed;top:0;right:0;z-index:9999;background:rgba(0,0,0,0.85);color:#0f0;font:11px monospace;padding:6px 10px;pointer-events:none;white-space:pre';
      document.body.appendChild(el);
      this.debugOverlay = el;
      this.updateDebugOverlay();
    } else {
      this.debugOverlay?.remove();
      this.debugOverlay = null;
    }
  }

  private updateDebugOverlay(): void {
    if (!this.debugOverlay) return;
    const s = this.debugStats;
    const d = this.client?.getDisplay();
    const dw = d?.getWidth() ?? 0, dh = d?.getHeight() ?? 0;
    const el = d?.getElement();
    const canvases = el?.querySelectorAll('canvas') ?? [];
    let pixelInfo = '';
    if (canvases.length > 0) {
      const c = canvases[0] as HTMLCanvasElement;
      const ctx = c.getContext('2d');
      if (ctx && c.width > 0 && c.height > 0) {
        const p = ctx.getImageData(c.width / 2, c.height / 2, 1, 1).data;
        pixelInfo = `px(mid): ${p[0]},${p[1]},${p[2]},${p[3]}`;
      }
    }
    this.debugOverlay.textContent = [
      `img:${s.imgCount} blob:${s.blobCount} end:${s.endCount} sync:${s.syncCount}`,
      `blobKB:${Math.round(s.blobBytes / 1024)} display:${dw}x${dh} canv:${canvases.length}`,
      s.sizeInfo,
      pixelInfo,
      s.lastImgInfo,
      `keys: dn=${this.keyDownCount} up=${this.keyUpCount} raw=${this.rawKeyCount}`,
    ].filter(Boolean).join('\n');
  }

  private setupInput(): void {
    if (!this.client) return;

    const display = this.client.getDisplay();
    const displayElement = display.getElement();

    // Raw DOM keyboard listener for debug diagnostics
    this.rawKeyCount = 0;
    this.rawKeyDebugHandler = (e: KeyboardEvent) => {
      this.rawKeyCount++;
      this.updateDebugOverlay();
    };
    document.addEventListener('keydown', this.rawKeyDebugHandler, true);

    // Keyboard — attach to document to capture all key events
    this.keyDownCount = 0;
    this.keyUpCount = 0;
    this.keyboard = new Guacamole.Keyboard(document);
    this.keyboard.onkeydown = (keysym: number) => {
      this.keyDownCount++;
      // CRITICAL: must pass 1/0 integers, NOT true/false booleans.
      // guacd parses with atoi() — atoi("true") = 0, so booleans break keyboard.
      this.client?.sendKeyEvent(1 as any, keysym);
      this.updateDebugOverlay();
      return false; // Prevent default browser behavior
    };
    this.keyboard.onkeyup = (keysym: number) => {
      this.keyUpCount++;
      this.client?.sendKeyEvent(0 as any, keysym);
      return false;
    };

    // Mouse
    this.mouse = new Guacamole.Mouse(displayElement);

    // Re-focus display on click (ensures keyboard capture after toolbar interaction)
    displayElement.addEventListener('click', () => displayElement.focus());

    const sendMouse = (mouseState: Guacamole.Mouse.State) => {
      // Guard against zero-size display element (causes NaN coordinates)
      if (!displayElement.offsetWidth || !displayElement.offsetHeight) return;

      // Scale mouse coordinates to match remote display
      const scale = display.getWidth() / displayElement.offsetWidth;
      const scaledState: Guacamole.Mouse.State = {
        x: Math.round(mouseState.x * scale),
        y: Math.round(mouseState.y * scale),
        left: mouseState.left,
        middle: mouseState.middle,
        right: mouseState.right,
        up: mouseState.up,
        down: mouseState.down,
      };
      this.client?.sendMouseState(scaledState);
    };

    this.mouse.onmousedown = sendMouse;
    this.mouse.onmouseup = sendMouse;
    this.mouse.onmousemove = sendMouse;

    // Focus the display element for keyboard input
    displayElement.setAttribute('tabindex', '0');
    displayElement.focus();
  }

  private fitDisplay(): void {
    if (!this.client) return;

    const display = this.client.getDisplay();
    const container = this.displayContainer.nativeElement;
    const displayElement = display.getElement();

    const containerWidth = container.offsetWidth;
    const containerHeight = container.offsetHeight;
    const displayWidth = display.getWidth();
    const displayHeight = display.getHeight();

    if (displayWidth === 0 || displayHeight === 0) return;
    if (containerWidth === 0 || containerHeight === 0) return;

    // Calculate scale to fit display in container
    const scaleX = containerWidth / displayWidth;
    const scaleY = containerHeight / displayHeight;
    const scale = Math.min(scaleX, scaleY, 1); // Don't scale up beyond 1:1

    display.scale(scale);

    // Center the display element manually since transform-origin is 0,0
    // The CSS transform doesn't affect layout, so we offset with margins
    const scaledWidth = displayWidth * scale;
    const scaledHeight = displayHeight * scale;
    const marginLeft = Math.max(0, (containerWidth - scaledWidth) / 2);
    const marginTop = Math.max(0, (containerHeight - scaledHeight) / 2);
    displayElement.style.marginLeft = marginLeft + 'px';
    displayElement.style.marginTop = marginTop + 'px';
  }

  private cleanup(): void {
    if (this.rawKeyDebugHandler) {
      document.removeEventListener('keydown', this.rawKeyDebugHandler, true);
      this.rawKeyDebugHandler = null;
    }
    if (this.keyboard) {
      // CRITICAL: Clear handlers BEFORE reset so leaked event listeners
      // (which Guacamole.Keyboard never removes) exit early on the
      // `if (!guac_keyboard.onkeydown) return` check and don't mark
      // events with EVENT_MARKER, which would block any new Keyboard instance.
      this.keyboard.onkeydown = null as any;
      this.keyboard.onkeyup = null as any;
      this.keyboard.reset();
    }
    this.keyboard = null;
    this.mouse = null;
    this.resizeObserver?.disconnect();
    this.resizeObserver = null;
    this.debugOverlay?.remove();
    this.debugOverlay = null;

    if (this.client) {
      try { this.client.disconnect(); } catch { /* ignore */ }
      const displayElement = this.client.getDisplay().getElement();
      displayElement.remove();
      this.client = null;
    }

    this.tunnel = null;
  }
}
