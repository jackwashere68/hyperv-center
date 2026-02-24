declare module 'guacamole-common-js' {
  namespace Guacamole {
    class Client {
      constructor(tunnel: Tunnel);

      connect(data?: string): void;
      disconnect(): void;

      getDisplay(): Display;

      sendKeyEvent(pressed: boolean, keysym: number): void;
      sendMouseState(mouseState: Mouse.State): void;
      sendSize(width: number, height: number): void;

      onerror: ((status: Status) => void) | null;
      onstatechange: ((state: number) => void) | null;
      onclipboard: ((stream: InputStream, mimetype: string) => void) | null;
      onname: ((name: string) => void) | null;

      static State: {
        IDLE: 0;
        CONNECTING: 1;
        WAITING: 2;
        CONNECTED: 3;
        DISCONNECTING: 4;
        DISCONNECTED: 5;
      };
    }

    class Display {
      getElement(): HTMLElement;
      getWidth(): number;
      getHeight(): number;
      getDefaultLayer(): Display.VisibleLayer;
      scale(scale: number): void;

      oncursor: ((canvas: HTMLCanvasElement, x: number, y: number) => void) | null;
      onresize: ((width: number, height: number) => void) | null;
    }

    namespace Display {
      class VisibleLayer {
        width: number;
        height: number;
      }
    }

    interface Tunnel {
      connect(data?: string): void;
      disconnect(): void;
      sendMessage(...elements: unknown[]): void;

      onerror: ((status: Status) => void) | null;
      onstatechange: ((state: number) => void) | null;
      oninstruction: ((opcode: string, args: string[]) => void) | null;

      state: number;
    }

    class WebSocketTunnel implements Tunnel {
      constructor(tunnelURL: string);

      connect(data?: string): void;
      disconnect(): void;
      sendMessage(...elements: unknown[]): void;

      onerror: ((status: Status) => void) | null;
      onstatechange: ((state: number) => void) | null;
      oninstruction: ((opcode: string, args: string[]) => void) | null;

      state: number;

      static State: {
        CLOSED: 0;
        OPEN: 1;
        UNSTABLE: 2;
      };
    }

    class Mouse {
      constructor(element: HTMLElement);

      onmousedown: ((mouseState: Mouse.State) => void) | null;
      onmouseup: ((mouseState: Mouse.State) => void) | null;
      onmousemove: ((mouseState: Mouse.State) => void) | null;

      static State: {
        new (
          x: number,
          y: number,
          left: boolean,
          middle: boolean,
          right: boolean,
          up: boolean,
          down: boolean,
        ): Mouse.State;
      };
    }

    namespace Mouse {
      interface State {
        x: number;
        y: number;
        left: boolean;
        middle: boolean;
        right: boolean;
        up: boolean;
        down: boolean;
      }

      class Touchpad {
        constructor(element: HTMLElement);

        onmousedown: ((mouseState: Mouse.State) => void) | null;
        onmouseup: ((mouseState: Mouse.State) => void) | null;
        onmousemove: ((mouseState: Mouse.State) => void) | null;
      }

      class Touchscreen {
        constructor(element: HTMLElement);

        onmousedown: ((mouseState: Mouse.State) => void) | null;
        onmouseup: ((mouseState: Mouse.State) => void) | null;
        onmousemove: ((mouseState: Mouse.State) => void) | null;
      }
    }

    class Keyboard {
      constructor(element: HTMLElement | Document);

      onkeydown: ((keysym: number) => boolean | void) | null;
      onkeyup: ((keysym: number) => void) | null;

      press(keysym: number): void;
      release(keysym: number): void;
      reset(): void;
    }

    class InputStream {
      onblob: ((data: string) => void) | null;
      onend: (() => void) | null;
    }

    class Status {
      code: number;
      message: string;
      isError(): boolean;

      static Code: {
        SUCCESS: number;
        UNSUPPORTED: number;
        SERVER_ERROR: number;
        UPSTREAM_TIMEOUT: number;
        UPSTREAM_ERROR: number;
        RESOURCE_NOT_FOUND: number;
        RESOURCE_CONFLICT: number;
        SESSION_CLOSED: number;
        SESSION_TIMEOUT: number;
        CLIENT_BAD_REQUEST: number;
        CLIENT_UNAUTHORIZED: number;
        CLIENT_FORBIDDEN: number;
        CLIENT_TIMEOUT: number;
        CLIENT_OVERRUN: number;
        CLIENT_BAD_TYPE: number;
        CLIENT_TOO_MANY: number;
      };
    }

    class StringReader {
      constructor(stream: InputStream);
      ontext: ((text: string) => void) | null;
      onend: (() => void) | null;
    }
  }

  export default Guacamole;
}
