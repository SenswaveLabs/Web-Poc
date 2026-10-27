import { request, type BrowserContext, type Locator, type Page } from '@playwright/test';
import { spawn, type ChildProcess } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';

const here = path.dirname(new URL(import.meta.url).pathname);

/** Reads tests/e2e/demo/.env (not committed) into process.env without overriding values that are already set. */
function loadEnvFile() {
  const file = path.join(here, '.env');
  if (!fs.existsSync(file)) return;
  for (const line of fs.readFileSync(file, 'utf8').split(/\r?\n/)) {
    const match = line.match(/^\s*([A-Z0-9_]+)\s*=\s*(.*)\s*$/);
    if (match && process.env[match[1]] === undefined) process.env[match[1]] = match[2];
  }
}

loadEnvFile();

function required(name: string) {
  const value = process.env[name];
  if (!value) throw new Error(`${name} is not set. Copy .env.example to .env and fill it in.`);
  return value;
}

export const cfg = {
  baseUrl: required('DEMO_BASE_URL').replace(/\/$/, ''),
  apiUrl: required('DEMO_API_URL').replace(/\/$/, ''),
  user1: { email: required('DEMO_USER1_EMAIL'), password: required('DEMO_USER1_PASSWORD') },
  user2: { email: required('DEMO_USER2_EMAIL'), password: required('DEMO_USER2_PASSWORD') },
  mqtt: {
    name: process.env.DEMO_MQTT_NAME ?? 'HiveMQ Cloud',
    host: required('DEMO_MQTT_URL'),
    port: process.env.DEMO_MQTT_PORT ?? '8883',
    username: required('DEMO_MQTT_USERNAME'),
    password: required('DEMO_MQTT_PASSWORD'),
  },
  /** DEMO_RECORD=1 records the X display with ffmpeg (run inside the demo Docker image). */
  record: process.env.DEMO_RECORD === '1',
  headless: process.env.DEMO_HEADLESS === '1',
  /** Multiplies every pause. 0 gives a fast dry run. */
  pace: Number(process.env.DEMO_PACE ?? '0.8'),
  outDir: path.resolve(here, process.env.DEMO_OUT_DIR ?? 'out'),
  screen: { width: 1920, height: 1080 },
};

fs.mkdirSync(cfg.outDir, { recursive: true });

export const pause = (page: Page, ms: number) => page.waitForTimeout(Math.round(ms * cfg.pace));

// ---------------------------------------------------------------- API cleanup

type Json = Record<string, any>;

/** Removes homes (with their rooms and devices), shared homes and data sources of a demo account. The account itself stays. */
export async function cleanAccount(user: { email: string; password: string }) {
  const api = await request.newContext({ baseURL: cfg.apiUrl });
  try {
    const login = await api.post('/api/v1/auth/login', { data: user });
    if (!login.ok()) throw new Error(`Cleanup login failed for ${user.email}: ${login.status()}`);
    const { accessToken } = (await login.json()) as Json;
    const headers = { Authorization: `Bearer ${accessToken}` };

    const homes = await api.get('/api/v1/homes', { headers });
    const items: Json[] = homes.ok() ? ((await homes.json()) as Json).items ?? [] : [];
    for (const home of items) {
      const res = home.isOwner
        ? await api.delete(`/api/v1/homes/${home.id}`, { headers })
        : await api.delete(`/api/v1/homes/sharings/leave/${home.id}`, { headers });
      console.log(`cleanup ${user.email}: ${home.isOwner ? 'delete' : 'leave'} home "${home.name}" -> ${res.status()}`);
    }

    const brokers = await api.get('/api/v1/datasources/brokers', { headers });
    const brokerItems: Json[] = brokers.ok() ? ((await brokers.json()) as Json).items ?? [] : [];
    for (const broker of brokerItems) {
      // A broker with a running client cannot be deleted, so stop the client first.
      await api.delete(`/api/v1/datasources/brokers/${broker.id}/clients`, { headers });
      const res = await api.delete(`/api/v1/datasources/brokers/${broker.id}`, { headers });
      console.log(`cleanup ${user.email}: delete data source "${broker.name}" -> ${res.status()}`);
    }
  } finally {
    await api.dispose();
  }
}

// ---------------------------------------------------------------- Captions

/**
 * Caption bar drawn over every page of the context. The text lives in localStorage,
 * so a reload or a second tab shows the current caption too. Hidden from assistive technology.
 */
export async function installOverlay(context: BrowserContext) {
  await context.addInitScript(() => {
    const KEY = '__demo_caption';
    const PANEL = '__demo_panel';
    const render = () => {
      if (!document.body) return;
      const read = (key: string) => {
        try { return JSON.parse(localStorage.getItem(key) ?? 'null'); } catch { return null; }
      };
      const caption = read(KEY);
      let bar = document.getElementById('__demo_bar');
      if (!caption) { bar?.remove(); } else {
        if (!bar) {
          bar = document.createElement('div');
          bar.id = '__demo_bar';
          bar.setAttribute('aria-hidden', 'true');
          bar.style.cssText = 'position:fixed;left:50%;bottom:28px;transform:translateX(-50%);z-index:2147483647;pointer-events:none;'
            + 'display:flex;align-items:center;gap:14px;width:max-content;max-width:min(92vw,1200px);padding:12px 22px;border-radius:14px;'
            + 'background:rgba(10,10,10,.88);color:#fff;font:600 min(24px,4.4vw)/1.3 "Liberation Sans",Arial,sans-serif;box-shadow:0 6px 24px rgba(0,0,0,.45);';
          const code = document.createElement('span');
          code.id = '__demo_code';
          code.style.cssText = 'flex:none;padding:4px 10px;border-radius:8px;background:#ffc107;color:#000;font-weight:800;';
          const lines = document.createElement('span');
          lines.style.cssText = 'display:flex;flex-direction:column;gap:2px;';
          const title = document.createElement('span');
          title.id = '__demo_title';
          const text = document.createElement('span');
          text.id = '__demo_text';
          text.style.cssText = 'font-weight:500;font-size:.78em;opacity:.85;';
          lines.append(title, text);
          bar.append(code, lines);
          document.body.appendChild(bar);
        }
        (document.getElementById('__demo_code') as HTMLElement).textContent = caption.code;
        (document.getElementById('__demo_code') as HTMLElement).style.display = caption.code ? '' : 'none';
        (document.getElementById('__demo_title') as HTMLElement).textContent = caption.title ?? '';
        (document.getElementById('__demo_title') as HTMLElement).style.display = caption.title ? '' : 'none';
        (document.getElementById('__demo_text') as HTMLElement).textContent = caption.text;
      }

      const panel = read(PANEL);
      let box = document.getElementById('__demo_panel');
      if (!panel) { box?.remove(); } else {
        if (!box) {
          box = document.createElement('div');
          box.id = '__demo_panel';
          box.setAttribute('aria-hidden', 'true');
          box.style.cssText = 'position:fixed;right:24px;top:24px;z-index:2147483647;pointer-events:none;white-space:pre;max-width:46vw;overflow:hidden;'
            + 'padding:16px 20px;border-radius:12px;border:2px solid #ffc107;background:rgba(10,10,10,.92);color:#fff;'
            + 'font:500 18px/1.5 "Liberation Mono",ui-monospace,monospace;box-shadow:0 6px 24px rgba(0,0,0,.45);';
          document.body.appendChild(box);
        }
        box.textContent = panel;
      }
    };
    (window as any).__demoRender = render;
    window.addEventListener('storage', render);
    document.addEventListener('DOMContentLoaded', render);
    new MutationObserver(() => {
      if (!document.getElementById('__demo_bar') && localStorage.getItem(KEY)) render();
    }).observe(document, { childList: true, subtree: true });
  });
}

export interface Chapter { at: number; code: string; badge: string; title: string; text: string }

export class Narrator {
  readonly chapters: Chapter[] = [];
  private startedAt = Date.now();

  constructor(private context: BrowserContext) {}

  markStart() { this.startedAt = Date.now(); }

  private async write(key: string, value: unknown) {
    for (const page of this.context.pages()) {
      if (page.isClosed() || !page.url().startsWith(cfg.baseUrl)) continue;
      await page.evaluate(([k, v]) => {
        if (v === null) localStorage.removeItem(k as string);
        else localStorage.setItem(k as string, JSON.stringify(v));
        (window as any).__demoRender?.();
      }, [key, value] as const).catch(() => {});
    }
  }

  private shot = 0;
  private badge = '';
  private title = '';

  /**
   * Starts a feature from the system description. Its badge (role or requirement) and title stay on screen
   * for the following steps, which add the current action below them.
   */
  feature(badge: string, title: string) {
    this.badge = badge;
    this.title = title;
  }

  /** Shows a caption, remembers when it appeared and gives the viewer time to read it. */
  async step(page: Page, code: string, text: string, readMs = 2200) {
    if (process.env.DEMO_STEP_SHOTS === '1') {
      // Dry runs keep the state each step ended in, to review the flow without a video.
      fs.mkdirSync(path.join(cfg.outDir, 'steps'), { recursive: true });
      for (const [i, p] of this.context.pages().entries()) {
        await p.screenshot({ path: path.join(cfg.outDir, 'steps', `${String(++this.shot).padStart(3, '0')}-before-${code || 'intro'}-tab${i + 1}.png`) }).catch(() => {});
      }
    }
    this.chapters.push({ at: (Date.now() - this.startedAt) / 1000, code, badge: this.badge, title: this.title, text });
    console.log(`[${formatTime((Date.now() - this.startedAt) / 1000)}] ${code} ${this.badge} | ${this.title} | ${text}`);
    await this.write('__demo_caption', { code: this.badge, title: this.title, text });
    await pause(page, readMs);
  }

  async panel(page: Page, text: string | null) {
    await this.write('__demo_panel', text);
    await pause(page, 300);
  }

  async clear(page: Page) {
    await this.write('__demo_caption', null);
    await this.write('__demo_panel', null);
  }

  /** Writes the chapter list used for the checklist (minute of the recording for each step). */
  save() {
    const lines: string[] = [];
    let title = '';
    for (const c of this.chapters) {
      if (c.title !== title) {
        title = c.title;
        lines.push('', `${formatTime(c.at)}  [${c.badge}] ${c.title}`);
      }
      lines.push(`${formatTime(c.at)}    ${c.code.padEnd(5)} ${c.text}`);
    }
    fs.writeFileSync(path.join(cfg.outDir, 'chapters.txt'), lines.join('\n').trimStart() + '\n');
  }
}

export function formatTime(seconds: number) {
  const s = Math.max(0, Math.floor(seconds));
  return `${String(Math.floor(s / 60)).padStart(2, '0')}:${String(s % 60).padStart(2, '0')}`;
}

// ---------------------------------------------------------------- Screen recording

export class ScreenRecorder {
  private proc?: ChildProcess;
  readonly file = path.join(cfg.outDir, 'senswave-demo.mp4');

  start() {
    if (!cfg.record) return;
    const display = process.env.DISPLAY ?? ':99';
    this.proc = spawn('ffmpeg', [
      '-y', '-loglevel', 'error',
      '-f', 'x11grab', '-draw_mouse', '1', '-framerate', '30',
      '-video_size', `${cfg.screen.width}x${cfg.screen.height}`, '-i', `${display}.0`,
      '-c:v', 'libx264', '-preset', 'veryfast', '-crf', '24', '-pix_fmt', 'yuv420p', '-movflags', '+faststart',
      this.file,
    ], { stdio: ['pipe', 'inherit', 'inherit'] });
  }

  async stop() {
    const proc = this.proc;
    if (!proc) return;
    const exited = new Promise<void>((resolve) => proc.on('exit', () => resolve()));
    proc.stdin?.write('q');
    proc.stdin?.end();
    await Promise.race([exited, new Promise((r) => setTimeout(r, 15_000))]);
  }
}

// ---------------------------------------------------------------- UI helpers

/** Types like a person, so the viewer can follow. */
export async function typeInto(field: Locator, text: string) {
  await field.click();
  await field.press('ControlOrMeta+a');
  await field.press('Delete');
  await field.pressSequentially(text, { delay: cfg.pace === 0 ? 0 : 45 });
}

/** Accessible name of a MudBlazor field, which gets a trailing asterisk when the field is required. */
export const label = (text: string) => new RegExp(`^${text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}\\*?$`);

/** Opens a MudSelect by its label and picks an option. */
export async function choose(page: Page, scope: Locator, label: string, option: string) {
  await scope.locator('.mud-select').filter({ hasText: label }).first().click();
  const item = page.locator('.mud-popover-open .mud-list-item').filter({ hasText: option }).first();
  await item.waitFor();
  await pause(page, 500);
  await item.click();
}
