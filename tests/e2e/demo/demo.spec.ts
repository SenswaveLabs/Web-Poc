import { test, expect, chromium, type BrowserContext, type Page } from '@playwright/test';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { cfg, cleanAccount, installOverlay, Narrator, ScreenRecorder, pause, typeInto, choose, label } from './helpers';

/*
 * Demo recording of Senswave for the course project. Runs against the deployed app with real accounts,
 * so it deletes and recreates the demo data of both accounts. Start it through record.sh, not in CI.
 */

const HOME = 'Mój dom';
const ROOM = 'Salon';
const DEVICE = 'Lampa';
const SCREEN = 'Sterowanie';
const OP_BRIGHTNESS = 'Jasność';
const OP_POWER = 'Zasilanie';
const W_SLIDER = 'Jasność suwak';
const W_50 = 'Jasność 50%';
const W_25 = 'Jasność 25%';
const W_SWITCH = 'Włącznik';
const TOPIC = 'senswave-demo/lampa';

test.describe.configure({ mode: 'serial' });
test.setTimeout(30 * 60_000);

test('Senswave – nagranie demonstracyjne', async () => {
  // ------------------------------------------------------------ Preparation (not recorded)
  await cleanAccount(cfg.user1);
  await cleanAccount(cfg.user2);

  const profile = fs.mkdtempSync(path.join(os.tmpdir(), 'senswave-demo-'));
  // No "Save password?" bubble over the recording.
  fs.mkdirSync(path.join(profile, 'Default'), { recursive: true });
  fs.writeFileSync(path.join(profile, 'Default', 'Preferences'), JSON.stringify({
    credentials_enable_service: false,
    profile: { password_manager_enabled: false, password_manager_leak_detection: false },
  }));
  // The window size is in scaled pixels, so divide by the scale factor to fill the recorded screen exactly.
  const scale = 1.25;
  const context: BrowserContext = await chromium.launchPersistentContext(profile, {
    headless: cfg.headless,
    viewport: cfg.headless ? { width: 1536, height: 800 } : null,
    serviceWorkers: 'block',
    slowMo: cfg.pace === 0 ? 0 : 80,
    locale: 'en-US',
    args: [
      '--window-position=0,0',
      `--window-size=${Math.round(cfg.screen.width / scale)},${Math.round(cfg.screen.height / scale)}`,
      `--force-device-scale-factor=${scale}`,
      '--remote-debugging-port=9222',
      '--no-first-run',
      '--disable-features=Translate,PasswordManagerOnboarding,PasswordLeakDetection',
      '--password-store=basic',
    ],
  });
  context.setDefaultTimeout(30_000);
  await installOverlay(context);

  const say = new Narrator(context);
  const recorder = new ScreenRecorder();
  const admin = context.pages()[0] ?? (await context.newPage());
  let guest: Page | undefined;

  try {
    await admin.goto(cfg.baseUrl + '/auth/login');
    await expect(admin.getByRole('heading', { name: 'Sign in', level: 1 })).toBeVisible({ timeout: 60_000 });

    recorder.start();
    say.markStart();

    // ------------------------------------------------------------ 0. Sign in and validation
    say.feature('Senswave', 'System IoT / Smart Home: wirtualne odwzorowanie urządzeń domu na konfigurowalnym ekranie');
    await say.step(admin, '', 'Aplikacja wdrożona pod ' + new URL(cfg.baseUrl).host, 3500);

    say.feature('Każdy użytkownik', 'Dołączenie do systemu: logowanie z walidacją danych po stronie klienta i serwera');
    await say.step(admin, '0.1', 'Walidacja formularza logowania: puste pola');
    await admin.getByRole('button', { name: 'Sign in', exact: true }).click();
    await expect(admin.getByText('Email is required.')).toBeVisible();
    await expect(admin.getByText('Password is required.')).toBeVisible();
    await pause(admin, 1800);

    await say.step(admin, '0.1', 'Walidacja: niepoprawny format adresu e-mail');
    await typeInto(admin.getByLabel('Email'), 'user@');
    await admin.getByLabel('Password').click();
    await pause(admin, 1800);

    await say.step(admin, '0.1', 'Walidacja po stronie serwera: błędne hasło');
    await typeInto(admin.getByLabel('Email'), cfg.user1.email);
    await typeInto(admin.getByLabel('Password'), 'WrongPassword1!');
    await admin.getByRole('button', { name: 'Sign in', exact: true }).click();
    await expect(admin.getByRole('alert').filter({ hasText: /incorrect|invalid/i })).toBeVisible();
    await pause(admin, 2000);

    say.feature('Autoryzacja', 'Po zalogowaniu serwer wydaje token dostępu z ograniczonym czasem życia');
    await say.step(admin, '0.2', 'Logowanie jako administrator domu (' + cfg.user1.email + ')');
    await typeInto(admin.getByLabel('Password'), cfg.user1.password);
    const loginResponse = admin.waitForResponse((r) => r.url().endsWith('/api/v1/auth/login') && r.request().method() === 'POST');
    await admin.getByRole('button', { name: 'Sign in', exact: true }).click();
    const token = await (await loginResponse).json();
    await expect(admin.getByRole('navigation').getByText('Logout', { exact: true })).toBeVisible();
    await expect(admin.getByRole('heading', { name: 'Welcome to Senswave' })).toBeVisible();

    await say.step(admin, '0.3', 'Token Bearer z czasem życia z odpowiedzi serwera', 500);
    const expiresAt = new Date(Date.now() + token.expiresIn * 1000).toLocaleTimeString('pl-PL', { timeZone: 'Europe/Warsaw' });
    await say.panel(admin, [
      'POST /api/v1/auth/login  →  200 OK',
      '',
      `tokenType:    ${token.tokenType}`,
      `accessToken:  ${String(token.accessToken).slice(0, 18)}…`,
      `expiresIn:    ${token.expiresIn} s  (ważny do ${expiresAt})`,
      `refreshToken: ${String(token.refreshToken).slice(0, 12)}…`,
      '',
      'Bez „Remember me” token trzymany jest tylko',
      'w pamięci karty i sessionStorage.',
    ].join('\n'));
    await pause(admin, 6000);
    await say.panel(admin, null);

    // ------------------------------------------------------------ 1. Data source
    say.feature('Każdy użytkownik', 'Konfiguracja własnego źródła danych: publiczny serwer MQTT, do którego łączy się backend');
    await say.step(admin, '1.1', 'Źródło danych: dodaj własny broker MQTT (HiveMQ Cloud, TLS, MQTT v5)');
    await menu(admin, 'Data sources').click();
    await expect(admin.getByRole('heading', { name: 'Data sources', level: 1 })).toBeVisible();
    await admin.getByRole('button', { name: 'Data source', exact: true }).click();
    const brokerDialog = admin.getByRole('dialog');
    await expect(brokerDialog.getByRole('heading', { name: 'Create Broker' })).toBeVisible();
    await typeInto(brokerDialog.getByRole('textbox', { name: label('Name') }), cfg.mqtt.name);
    await typeInto(brokerDialog.getByRole('textbox', { name: label('Host') }), cfg.mqtt.host);
    await typeInto(brokerDialog.getByRole('spinbutton', { name: label('Port') }), cfg.mqtt.port);
    await typeInto(brokerDialog.getByRole('textbox', { name: label('Client Name') }), `senswave-demo-${Date.now().toString(36)}`);
    await brokerDialog.getByText('Use TLS', { exact: true }).click();
    await typeInto(brokerDialog.getByRole('textbox', { name: label('Username') }), cfg.mqtt.username);
    await typeInto(brokerDialog.getByRole('textbox', { name: label('Password') }), cfg.mqtt.password);
    await pause(admin, 1000);
    await brokerDialog.getByRole('button', { name: 'Add', exact: true }).click();
    await expect(brokerDialog).toBeHidden();
    await expect(admin.getByText(cfg.mqtt.name).first()).toBeVisible();
    await pause(admin, 1500);

    await say.step(admin, '1.2', 'Uruchom połączenie backendu z brokerem MQTT');
    if (!admin.url().includes('/datasources/details/')) {
      await admin.getByText(cfg.mqtt.name).first().click();
    }
    await expect(admin.getByRole('heading', { name: cfg.mqtt.name, level: 1 })).toBeVisible();
    await expect(admin.getByText('Not started', { exact: true })).toBeVisible();
    await pause(admin, 1200);
    await admin.getByRole('button', { name: 'Start client' }).click();
    const credentials = admin.getByRole('dialog');
    await typeInto(credentials.getByRole('textbox', { name: label('Username') }), cfg.mqtt.username);
    await typeInto(credentials.getByRole('textbox', { name: label('Password') }), cfg.mqtt.password);
    await pause(admin, 600);
    await credentials.getByRole('button', { name: 'Connect', exact: true }).click();
    await expect(admin.getByText('Connected', { exact: true })).toBeVisible({ timeout: 60_000 });
    await pause(admin, 2500);

    // ------------------------------------------------------------ 2. Home
    say.feature('Administrator domu', 'Konfiguracja domu: utworzenie domu i połączenie go ze źródłem danych');
    await say.step(admin, '2.1', 'Dom: stwórz dom „' + HOME + '”');
    await openHomesList(admin);
    await admin.getByRole('button', { name: /^(Create|Home)$/ }).first().click();
    const homeDialog = admin.getByRole('dialog');
    await typeInto(homeDialog.getByRole('textbox', { name: label('Name') }), HOME);
    await pause(admin, 600);
    await homeDialog.getByRole('button', { name: 'Add', exact: true }).click();
    await expect(homeDialog).toBeHidden();
    await expect(admin.getByText(HOME).first()).toBeVisible();
    await pause(admin, 1500);

    await say.step(admin, '2.2', 'Przejdź do domu i połącz go ze źródłem danych');
    await admin.getByText(HOME).first().click();
    await menu(admin, 'Home & sharing').click();
    await expect(admin.getByRole('heading', { name: HOME, level: 1 })).toBeVisible();
    await admin.getByRole('button', { name: 'Link data source' }).click();
    await admin.getByRole('dialog').getByText(cfg.mqtt.name).first().click();
    await expect(admin.getByText('Connected', { exact: true })).toBeVisible({ timeout: 30_000 });
    await pause(admin, 2000);

    say.feature('Współtwórca domu', 'Tworzenie pokoi w domu i przypisywanie do nich urządzeń');
    await say.step(admin, '2.3', 'Ekran domu: stwórz pokój „' + ROOM + '”');
    await menu(admin, 'Dashboard').click();
    await expect(admin.getByRole('heading', { name: 'Devices', level: 1 })).toBeVisible();
    await admin.getByRole('button', { name: 'Room', exact: true }).click();
    const roomDialog = admin.getByRole('dialog');
    await typeInto(roomDialog.getByRole('textbox', { name: label('Name') }), ROOM);
    await roomDialog.getByRole('button', { name: 'Add', exact: true }).click();
    await expect(roomDialog).toBeHidden();
    await expect(admin.getByRole('button', { name: new RegExp(`^${ROOM}`) }).first()).toBeVisible();
    await pause(admin, 1500);

    // ------------------------------------------------------------ 3. Device
    await say.step(admin, '3.1', 'Urządzenie: stwórz lampę w pokoju „' + ROOM + '”');
    await admin.getByRole('button', { name: 'Device', exact: true }).click();
    const deviceDialog = admin.getByRole('dialog');
    await expect(deviceDialog.getByRole('heading', { name: 'Create Device' })).toBeVisible();
    await typeInto(deviceDialog.getByRole('textbox', { name: label('Name') }), DEVICE);
    await choose(admin, deviceDialog, 'Icon', 'Lightbulb');
    await choose(admin, deviceDialog, 'Room', ROOM);
    await pause(admin, 800);
    await deviceDialog.getByRole('button', { name: 'Create', exact: true }).click();
    await expect(deviceDialog).toBeHidden();
    await openDeviceConfiguration(admin);
    await pause(admin, 1500);

    say.feature('Współtwórca domu', 'Konfiguracja cech urządzenia: system przyjmuje definicje operacji (jasność 1–100, zasilanie on/off)');
    await say.step(admin, '3.2', 'Dodaj ekran (dashboard) „' + SCREEN + '”');
    await admin.getByRole('tab', { name: /^Screens/ }).click();
    await admin.getByRole('button', { name: 'Screen', exact: true }).click();
    const screenDialog = admin.getByRole('dialog');
    await typeInto(screenDialog.getByRole('textbox', { name: label('Name') }), SCREEN);
    await typeInto(screenDialog.getByRole('spinbutton', { name: label('Rows') }), '3');
    await pause(admin, 600);
    await screenDialog.getByRole('button', { name: 'Add', exact: true }).click();
    await expect(screenDialog).toBeHidden();
    await expect(admin.getByRole('tab', { name: 'Screens (1)' })).toBeVisible();
    await pause(admin, 1500);

    await say.step(admin, '3.3', 'Operacja numeryczna „' + OP_BRIGHTNESS + '” z zakresem 1–100');
    await admin.getByRole('tab', { name: /^Operations/ }).click();
    await addOperation(admin, OP_BRIGHTNESS, `${TOPIC}/brightness`, 'Number Value', { min: '1', max: '100' });

    await say.step(admin, '3.4', 'Operacja logiczna (boolean) „' + OP_POWER + '” – włącz / wyłącz');
    await addOperation(admin, OP_POWER, `${TOPIC}/power`, 'Boolean Value');

    say.feature('Współtwórca domu', 'Konfiguracja wyglądu do interakcji: widgety powiązane z operacjami urządzenia');
    await say.step(admin, '3.5', 'Widget: suwak jasności');
    await addWidget(admin, OP_BRIGHTNESS, W_SLIDER, 'Slider', async (dialog) => {
      await typeInto(dialog.getByRole('spinbutton', { name: label('Step') }), '1');
    });

    await say.step(admin, '3.6', 'Widget: przycisk „jasność 50%”');
    await addWidget(admin, OP_BRIGHTNESS, W_50, 'Button', async (dialog) => {
      await typeInto(dialog.getByRole('spinbutton', { name: label('Value to send') }), '50');
    });

    await say.step(admin, '3.7', 'Widget: przycisk „jasność 25%”');
    await addWidget(admin, OP_BRIGHTNESS, W_25, 'Button', async (dialog) => {
      await typeInto(dialog.getByRole('spinbutton', { name: label('Value to send') }), '25');
    });

    await say.step(admin, '3.8', 'Widget: przełącznik on/off dla operacji „' + OP_POWER + '”');
    await addWidget(admin, OP_POWER, W_SWITCH, 'Switch');

    await say.step(admin, '3.9', 'Kafelek urządzenia: przełącznik on/off na ekranie domu');
    await admin.getByRole('tab', { name: 'General' }).click();
    const general = admin.getByRole('tabpanel', { name: 'General' });
    await choose(admin, general, 'Tile Type', 'Switch');
    await choose(admin, general, 'Tile Operation', OP_POWER);
    await pause(admin, 600);
    await admin.getByRole('button', { name: 'Save', exact: true }).click();
    await expect(admin.getByRole('button', { name: 'Save', exact: true })).toBeDisabled();
    await pause(admin, 1500);

    say.feature('Współtwórca domu', 'Dostosowanie wizualnego układu operacji urządzenia na ekranie');
    await say.step(admin, '3.10', 'Przejdź na dashboard urządzenia');
    await admin.getByRole('link', { name: 'Open controls' }).click();
    await expect(admin.getByRole('heading', { name: DEVICE, level: 1 })).toBeVisible();
    await pause(admin, 1000);

    await say.step(admin, '3.11', 'Ułóż stworzone widgety na ekranie');
    await placeWidget(admin, 1, 1, 4, 1, W_SLIDER);
    await placeWidget(admin, 2, 1, 2, 1, W_25);
    await placeWidget(admin, 2, 3, 2, 1, W_50);
    await placeWidget(admin, 3, 1, 4, 1, W_SWITCH);
    const done = admin.getByRole('button', { name: 'Done', exact: true });
    if (await done.isVisible().catch(() => false)) await done.click();
    await pause(admin, 2000);

    // ------------------------------------------------------------ 4-8. Use the lamp
    say.feature('Użytkownik domu', 'Otwieranie urządzenia i korzystanie z interfejsu graficznego dostępnego na ekranie');
    await say.step(admin, '4', 'Przejdź na ekran domu');
    await menu(admin, 'Dashboard').click();
    await expect(admin.getByRole('button', { name: `Open ${DEVICE}` })).toBeVisible();
    await pause(admin, 1200);

    await say.step(admin, '5', 'Włącz lampę z kafelka urządzenia');
    await admin.getByRole('checkbox', { name: `Turn ${DEVICE} on` }).click();
    await expect(admin.getByRole('checkbox', { name: `Turn ${DEVICE} off` })).toBeChecked();
    await pause(admin, 1500);

    await say.step(admin, '6', 'Otwórz lampę i ustaw jasność przyciskiem 25%');
    await openLamp(admin);
    await admin.getByRole('button', { name: W_25, exact: true }).click();
    await expect(admin.getByRole('slider', { name: W_SLIDER })).toHaveValue('25');
    await pause(admin, 1500);

    await say.step(admin, '7', 'Zmień jasność przyciskiem 50%');
    await admin.getByRole('button', { name: W_50, exact: true }).click();
    await expect(admin.getByRole('slider', { name: W_SLIDER })).toHaveValue('50');
    await pause(admin, 1500);

    await say.step(admin, '8', 'Ustaw jasność 99 suwakiem');
    await setSlider(admin, 99);
    await expect(admin.getByRole('slider', { name: W_SLIDER })).toHaveValue('99');
    await pause(admin, 2000);

    // ------------------------------------------------------------ 9-11. Share the home
    say.feature('Administrator domu', 'Zapraszanie ludzi do domu: rola Użytkownik domu („Use”) lub Współtwórca domu („Manage”)');
    await say.step(admin, '9', 'Zaproś drugiego użytkownika z rolą „Use” (tylko korzystanie); „Manage” dałoby prawa Współtwórcy');
    await menu(admin, 'Home & sharing').click();
    await admin.getByRole('button', { name: 'Invite', exact: true }).click();
    const inviteDialog = admin.getByRole('dialog');
    await typeInto(inviteDialog.getByRole('textbox', { name: label('Email') }), cfg.user2.email);
    await inviteDialog.getByText('Use', { exact: true }).click();
    await pause(admin, 800);
    await inviteDialog.getByRole('button', { name: 'Create invitation' }).click();
    const codeDialog = admin.getByRole('dialog').filter({ hasText: 'Send this code to' });
    await expect(codeDialog).toBeVisible();
    const code = (await codeDialog.locator('h5').first().innerText()).trim();
    expect(code).toMatch(/^[A-Z0-9]{8}$/);
    await pause(admin, 3500);
    await codeDialog.getByRole('button', { name: 'Done' }).click();

    // Admin waits on the lamp screen, so the live update is visible later.
    await menu(admin, 'Dashboard').click();
    await openLamp(admin);

    say.feature('Udostępnianie domu', 'Drugi użytkownik dołącza do udostępnionego domu kodem zaproszenia');
    await say.step(admin, '10', 'Nowa karta: logowanie jako drugi użytkownik (' + cfg.user2.email + ')', 1500);
    guest = await context.newPage();
    await guest.goto(cfg.baseUrl + '/auth/login');
    await guest.bringToFront();
    await expect(guest.getByRole('heading', { name: 'Sign in', level: 1 })).toBeVisible({ timeout: 60_000 });
    await guest.evaluate(() => (window as any).__demoRender?.());
    await typeInto(guest.getByLabel('Email'), cfg.user2.email);
    await typeInto(guest.getByLabel('Password'), cfg.user2.password);
    await guest.getByRole('button', { name: 'Sign in', exact: true }).click();
    await expect(guest.getByRole('navigation').getByText('Logout', { exact: true })).toBeVisible();
    await pause(guest, 1500);

    await say.step(guest, '11', 'Dołącz do domu kodem zaproszenia ' + code);
    await openHomesList(guest);
    await guest.getByRole('button', { name: /^(I have an invitation code|Join)$/ }).first().click();
    const joinDialog = guest.getByRole('dialog');
    await typeInto(joinDialog.getByRole('textbox', { name: label('Join Code') }), code);
    await pause(guest, 600);
    await joinDialog.getByRole('button', { name: 'Join', exact: true }).click();
    await expect(joinDialog).toBeHidden();
    await expect(guest.getByText(HOME).first()).toBeVisible();
    await pause(guest, 1500);

    // ------------------------------------------------------------ 12-13. Live update
    say.feature('Użytkownik domu', 'Członek rodziny steruje lampą, a zmiany widać na żywo u wszystkich członków domu');
    await say.step(guest, '12', 'Drugi użytkownik otwiera lampę i ją wyłącza');
    await guest.getByText(HOME).first().click();
    await menu(guest, 'Dashboard').click();
    await openLamp(guest);
    await pause(guest, 1000);
    await guest.getByRole('button', { name: new RegExp(`^${W_SWITCH}`) }).click();
    await expect(guest.getByRole('button', { name: new RegExp(`^${W_SWITCH}`) })).toContainText('Off');
    await pause(guest, 1500);

    await say.step(guest, '12', 'Drugi użytkownik włącza lampę i ustawia jasność 25%');
    await guest.getByRole('button', { name: new RegExp(`^${W_SWITCH}`) }).click();
    await expect(guest.getByRole('button', { name: new RegExp(`^${W_SWITCH}`) })).toContainText('On');
    await guest.getByRole('button', { name: W_25, exact: true }).click();
    await expect(guest.getByRole('slider', { name: W_SLIDER })).toHaveValue('25');
    await pause(guest, 1500);

    await say.step(admin, '13', 'Pierwsza karta (administrator): wartości zaktualizowały się na żywo (SignalR)', 500);
    await admin.bringToFront();
    await expect(admin.getByRole('slider', { name: W_SLIDER })).toHaveValue('25', { timeout: 15_000 });
    await expect(admin.getByRole('button', { name: new RegExp(`^${W_SWITCH}`) })).toContainText('On');
    await pause(admin, 3500);

    // ------------------------------------------------------------ 14-15. Authorization
    say.feature('ACL (uprawnienia per dom)', 'Użytkownik domu nie może zmieniać urządzeń: serwer odrzuca zmianę nazwy');
    await say.step(guest, '14', 'Użytkownik z rolą „Use” próbuje zmienić nazwę urządzenia', 500);
    await guest.bringToFront();
    await pause(guest, 1500);
    await tryRenameAsGuest(guest, say);

    say.feature('Administrator domu', 'Usuwanie ludzi z domu: dostęp do domu i urządzeń znika od razu');
    await say.step(admin, '15', 'Administrator usuwa drugiego użytkownika z domu', 500);
    await admin.bringToFront();
    await menu(admin, 'Home & sharing').click();
    await expect(admin.getByText(cfg.user2.email)).toBeVisible();
    await pause(admin, 1200);
    await admin.getByRole('button', { name: `Remove ${cfg.user2.email}` }).click();
    const confirm = admin.getByRole('dialog');
    if (await confirm.isVisible({ timeout: 2000 }).catch(() => false)) {
      await pause(admin, 800);
      await confirm.getByRole('button', { name: /^(Remove|Delete|Yes|Confirm)/ }).first().click();
    }
    await expect(admin.getByText(cfg.user2.email)).toBeHidden();
    await pause(admin, 1500);

    await say.step(guest, '15', 'Drugi użytkownik traci dostęp do domu i lampy', 500);
    await guest.bringToFront();
    await pause(guest, 1000);
    await menu(guest, 'Dashboard').click();
    await expect(guest.getByRole('button', { name: `Open ${DEVICE}` })).toBeHidden();
    await pause(guest, 3000);
    await say.step(guest, '15', 'Lista domów drugiego użytkownika: „' + HOME + '” zniknęła');
    await guest.getByRole('button', { name: /^Home: .*Switch home$/ }).first().click();
    await pause(guest, 1000);
    await guest.getByRole('link', { name: 'All homes' }).click();
    // Cleanup left the guest with no homes, so the empty state proves the shared home is gone.
    await expect(guest.getByRole('heading', { name: 'Welcome to Senswave' })).toBeVisible();
    await pause(guest, 3000);

    // ------------------------------------------------------------ 16. Accessibility and mobile
    say.feature('Wymaganie niefunkcjonalne', 'Wygląd do wyboru: jasny, ciemny i kontrastowy (dla osób słabo widzących)');
    await say.step(admin, '16', 'Motyw wysokiego kontrastu', 500);
    await admin.bringToFront();
    await menu(admin, 'Settings').click();
    await admin.getByRole('group', { name: 'Theme' }).getByText('High Contrast').click();
    await pause(admin, 1500);
    await menu(admin, 'Dashboard').click();
    await pause(admin, 2000);
    await openLamp(admin);
    await pause(admin, 2500);

    await say.step(admin, '16', 'Motywy jasny i ciemny');
    await menu(admin, 'Settings').click();
    await admin.getByRole('group', { name: 'Theme' }).getByText('Light', { exact: true }).click();
    await pause(admin, 2000);
    await admin.getByRole('group', { name: 'Theme' }).getByText('Dark', { exact: true }).click();
    await pause(admin, 2000);

    say.feature('Wymaganie niefunkcjonalne', 'Responsywny i intuicyjny interfejs: widok mobilny');
    await say.step(admin, '17', 'Widok mobilny: szerokość 360 px');
    await admin.setViewportSize({ width: 360, height: 740 });
    await pause(admin, 1500);
    await openMobileMenuLink(admin, 'Dashboard');
    await mobileTour(admin);

    // ------------------------------------------------------------ 18. Lighthouse
    say.feature('Lighthouse', 'Audyt dostępności Lighthouse strony z listą urządzeń');
    await say.step(admin, '18', 'Audyt uruchomiony na stronie głównej z listą urządzeń', 500);
    await relogWithRememberMe(admin, say);
    await say.clear(admin);
    const report = await runLighthouse(cfg.baseUrl + '/devices');
    const reportPage = await context.newPage();
    await reportPage.goto('file://' + report.htmlPath);
    await reportPage.bringToFront();
    await showReportCaption(reportPage, `Audyt dostępności Lighthouse: Accessibility ${report.scores.accessibility}`);
    await pause(reportPage, 6000);
    // End on the list of audits.
    await reportPage.locator('#accessibility').scrollIntoViewIfNeeded().catch(() => {});
    await reportPage.evaluate(() => document.getElementById('accessibility')?.scrollIntoView({ block: 'start' }));
    await pause(reportPage, 6000);

    // ------------------------------------------------------------ 19. Print view
    say.feature('Wydruk', 'Widok wydruku strony głównej: zostają dane, znikają menu i elementy sterujące');
    await admin.bringToFront();
    await say.step(admin, '19', 'Strona główna z listą urządzeń w zwykłym widoku', 500);
    await pause(admin, 2500);
    await say.step(admin, '19', 'Ten sam ekran w widoku wydruku (A4, CSS @media print)', 500);
    // A4 at 96 dpi is 794 px wide, so the layout matches the printed page.
    await admin.setViewportSize({ width: 794, height: 760 });
    await admin.emulateMedia({ media: 'print' });
    await expect(admin.getByRole('button', { name: `Open ${DEVICE}` })).toBeVisible();
    await pause(admin, 6000);
    await admin.emulateMedia({ media: 'screen' });
    await admin.setViewportSize({ width: 1536, height: 760 }).catch(() => {});
  } catch (error) {
    // Keep what every tab showed, the run is long and hard to repeat.
    for (const [i, page] of context.pages().entries()) {
      await page.screenshot({ path: path.join(cfg.outDir, `failure-tab${i + 1}.png`) }).catch(() => {});
      const tree = await page.locator('body').ariaSnapshot().catch(() => '');
      fs.writeFileSync(path.join(cfg.outDir, `failure-tab${i + 1}.txt`), `${page.url()}\n${tree}`);
    }
    throw error;
  } finally {
    await recorder.stop();
    say.save();
    await context.close();
  }
});

// ---------------------------------------------------------------- Steps

async function openHomesList(page: Page) {
  const heading = page.getByRole('heading', { name: /^(Welcome to Senswave|My homes)$/ });
  if (!(await heading.isVisible().catch(() => false))) {
    // Without a home the menu links straight to My homes, otherwise the home page breadcrumb does.
    const myHomes = menu(page, 'My homes');
    if (await myHomes.isVisible().catch(() => false)) {
      await myHomes.click();
    } else {
      await menu(page, 'Home & sharing').click();
      await page.getByRole('link', { name: 'My homes' }).first().click();
    }
  }
  await expect(heading).toBeVisible({ timeout: 30_000 });
}

/** Link in the side menu (breadcrumbs reuse the same names). */
function menu(page: Page, name: string) {
  return page.getByRole('complementary').getByRole('link', { name, exact: true });
}

/** Opens the lamp screen from the device list. Retries once if the screen failed to load. */
async function openLamp(page: Page) {
  await page.getByRole('button', { name: `Open ${DEVICE}` }).click();
  const heading = page.getByRole('heading', { name: DEVICE, level: 1 });
  const failed = page.getByRole('button', { name: 'Retry' });
  await expect(heading.or(failed)).toBeVisible();
  if (await failed.isVisible().catch(() => false)) {
    await pause(page, 800);
    await failed.click();
  }
  await expect(heading).toBeVisible();
}

async function openDeviceConfiguration(page: Page) {
  const heading = page.getByRole('heading', { name: `Configure ${DEVICE}`, level: 1 });
  if (await heading.isVisible({ timeout: 4000 }).catch(() => false)) return;
  await page.getByRole('button', { name: `${DEVICE} options` }).click();
  await page.getByText('Configure', { exact: true }).click();
  await expect(heading).toBeVisible();
}

/** The configure page returns to the General tab after every change, so reopen Operations. */
async function showOperationsTab(page: Page) {
  const tab = page.getByRole('tab', { name: /^Operations/ });
  await tab.waitFor();
  if ((await tab.getAttribute('aria-selected')) !== 'true') {
    await pause(page, 500);
    await tab.click();
  }
}

async function addOperation(page: Page, name: string, topic: string, type: string, range?: { min: string; max: string }) {
  await page.getByRole('button', { name: 'Operation', exact: true }).click();
  const dialog = page.getByRole('dialog');
  await expect(dialog.getByRole('heading', { name: 'Add New Operation' })).toBeVisible();
  await typeInto(dialog.getByRole('textbox', { name: label('Name') }), name);
  await typeInto(dialog.getByRole('textbox', { name: label('Topic') }), topic);
  await choose(page, dialog, 'Operation Type', type);
  if (range) {
    await dialog.getByText('Enable Range Check').click();
    await typeInto(dialog.getByRole('textbox', { name: label('Min Value') }).or(dialog.getByRole('spinbutton', { name: label('Min Value') })), range.min);
    await typeInto(dialog.getByRole('textbox', { name: label('Max Value') }).or(dialog.getByRole('spinbutton', { name: label('Max Value') })), range.max);
  }
  await pause(page, 800);
  await dialog.getByRole('button', { name: 'Create Operation' }).click();
  await expect(dialog).toBeHidden();
  await showOperationsTab(page);
  await expect(page.getByRole('button', { name: `${name} options` })).toBeVisible();
  await pause(page, 1200);
}

async function addWidget(page: Page, operation: string, name: string, type: string, configure?: (dialog: ReturnType<Page['getByRole']>) => Promise<void>) {
  await showOperationsTab(page);
  const card = page.getByRole('tabpanel').locator('.mud-paper').filter({ has: page.getByRole('button', { name: `${operation} options` }) }).last();
  await card.getByRole('button', { name: 'Widget', exact: true }).click();
  const dialog = page.getByRole('dialog');
  await expect(dialog.getByRole('heading', { name: 'Add Widget' })).toBeVisible();
  await expect(dialog.getByRole('textbox', { name: label('Widget Name') })).toBeVisible();
  await typeInto(dialog.getByRole('textbox', { name: label('Widget Name') }), name);
  await choose(page, dialog, 'Widget Type', type);
  if (configure) await configure(dialog);
  await pause(page, 800);
  await dialog.getByRole('button', { name: 'Create Widget' }).click();
  await expect(dialog).toBeHidden();
  await showOperationsTab(page);
  await expect(page.getByText(name, { exact: true }).first()).toBeVisible();
  await pause(page, 1200);
}

/** Picks an empty cell (1-based), stretches the selection and attaches a widget. */
async function placeWidget(page: Page, row: number, col: number, width: number, height: number, widget: string) {
  // Placing a widget ends edit mode and reloads the screen, which can swallow an early click.
  const cell = page.locator(`button[style*="grid-row: ${row}; grid-column: ${col};"]`);
  for (let attempt = 0; attempt < 5 && !(await cell.isVisible().catch(() => false)); attempt++) {
    await page.waitForLoadState('networkidle').catch(() => {});
    const edit = page.getByRole('button', { name: 'Edit layout' });
    if (await edit.isVisible().catch(() => false)) await edit.click();
    await cell.waitFor({ timeout: 3000 }).catch(() => {});
  }
  await pause(page, 500);
  await cell.click();
  for (let i = 1; i < width; i++) await page.getByRole('button', { name: 'Wider' }).click();
  for (let i = 1; i < height; i++) await page.getByRole('button', { name: 'Taller' }).click();
  await pause(page, 600);
  await page.getByRole('button', { name: 'Place widget' }).click();
  const dialog = page.getByRole('dialog');
  await expect(dialog.getByRole('heading', { name: 'Select Widget' })).toBeVisible();
  await choose(page, dialog, 'Choose Widget', widget);
  await pause(page, 500);
  await dialog.getByRole('button', { name: 'Attach Widget' }).click();
  await expect(dialog).toBeHidden();
  await pause(page, 1000);
}


async function setSlider(page: Page, target: number) {
  const slider = page.getByRole('slider', { name: W_SLIDER });
  const box = await slider.boundingBox();
  if (box) {
    // One click on the track sends one value. Every value goes through the server, so wait for it to settle.
    const min = Number(await slider.getAttribute('min'));
    const max = Number(await slider.getAttribute('max'));
    const x = box.x + (box.width * (target - min)) / (max - min);
    await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2, { steps: 10 });
    await page.mouse.click(x, box.y + box.height / 2);
    await page.waitForTimeout(1500);
  }
  for (let i = 0; i < 8; i++) {
    const value = Number(await slider.inputValue());
    if (value === target) break;
    await slider.focus();
    await slider.press(value < target ? 'ArrowRight' : 'ArrowLeft');
    await page.waitForTimeout(1000);
  }
}

async function tryRenameAsGuest(page: Page, say: Narrator) {
  // The tile menu or the Configure button is the way an owner renames a device.
  const configure = page.getByRole('button', { name: 'Configure' });
  if (await configure.isVisible().catch(() => false)) {
    await configure.click();
  } else {
    await menu(page, 'Dashboard').click();
    await page.getByRole('button', { name: `${DEVICE} options` }).click();
    await pause(page, 1200);
    const menuConfigure = page.getByText('Configure', { exact: true });
    if (await menuConfigure.isVisible().catch(() => false)) await menuConfigure.click();
  }
  await pause(page, 2000);

  const name = page.getByRole('textbox', { name: label('Name') }).first();
  await name.waitFor({ timeout: 15_000 }).catch(() => {});
  await page.waitForLoadState('networkidle').catch(() => {});
  if (await name.isVisible().catch(() => false) && await name.isEditable().catch(() => false)) {
    await typeInto(name, `${DEVICE} gościa`);
    const save = page.getByRole('button', { name: 'Save', exact: true });
    const response = page.waitForResponse((r) => /\/api\/v1\/devices\/[^/]+$/.test(r.url()) && r.request().method() === 'PATCH', { timeout: 15_000 }).catch(() => null);
    await save.click();
    const res = await response;
    const body = res ? await res.text().catch(() => '') : '';
    console.log(`guest rename: ${res ? res.status() : 'no PATCH request'} ${body}`);
    const codes = (() => {
      try { return (JSON.parse(body).errors ?? []).map((e: { code: string }) => e.code); } catch { return []; }
    })();
    await pause(page, 1500);
    await say.panel(page, [
      `PATCH /api/v1/devices/{id}  →  ${res ? `${res.status()} ${res.statusText()}` : 'brak odpowiedzi'}`,
      ...codes.map((c: string) => `errors: ${c}`),
      '',
      'Serwer odrzuca zmianę: rola „Use” (Display)',
      'może tylko korzystać z urządzeń.',
    ].join('\n'));
    expect(res?.status(), 'server must reject the rename').toBeGreaterThanOrEqual(400);
  } else {
    console.log('guest rename: name field not editable');
    await say.panel(page, 'Interfejs nie pozwala zmienić nazwy:\nrola „Use” (Display) nie ma dostępu do konfiguracji.');
  }
  await pause(page, 5000);
  await say.panel(page, null);
}

async function mobileTour(page: Page) {
  await expect(page.getByRole('button', { name: `Open ${DEVICE}` })).toBeVisible();
  await pause(page, 2500);
  await openLamp(page);
  await pause(page, 3000);
  await page.mouse.wheel(0, 400);
  await pause(page, 1500);
  await openMobileMenuLink(page, 'Home');
  await pause(page, 2500);
  await page.setViewportSize({ width: 1536, height: 760 }).catch(() => {});
}

/** On a phone the side menu is replaced by the bottom navigation. */
async function openMobileMenuLink(page: Page, name: string) {
  await page.getByRole('navigation', { name: 'Main' }).getByRole('link', { name, exact: true }).click();
}

async function relogWithRememberMe(page: Page, say: Narrator) {
  // Lighthouse reloads the page. Only a "Remember me" session survives a reload, so sign in again with it.
  await page.getByRole('navigation').getByText('Logout', { exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Sign in', level: 1 })).toBeVisible();
  await typeInto(page.getByLabel('Email'), cfg.user1.email);
  await typeInto(page.getByLabel('Password'), cfg.user1.password);
  await page.getByText('Remember me').click();
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(page.getByRole('navigation').getByText('Logout', { exact: true })).toBeVisible();
  await menu(page, 'Dashboard').click();
  await expect(page.getByRole('button', { name: `Open ${DEVICE}` })).toBeVisible();
  await pause(page, 1000);
}

async function runLighthouse(url: string) {
  const { default: lighthouse } = await import('lighthouse');
  const { default: desktopConfig } = await import('lighthouse/core/config/desktop-config.js');
  const result = await lighthouse(url, {
    port: 9222,
    output: 'html',
    logLevel: 'error',
    disableStorageReset: true,
    onlyCategories: ['accessibility'],
  }, desktopConfig);
  if (!result) throw new Error('Lighthouse returned no result');
  const htmlPath = path.join(cfg.outDir, 'lighthouse.html');
  fs.writeFileSync(htmlPath, result.report as string);
  const scores = Object.fromEntries(
    Object.entries(result.lhr.categories).map(([id, c]) => [id, Math.round((c.score ?? 0) * 100)]),
  ) as Record<string, number>;
  fs.writeFileSync(path.join(cfg.outDir, 'lighthouse-scores.json'), JSON.stringify(scores, null, 2));
  return { htmlPath, scores };
}

async function showReportCaption(page: Page, text: string) {
  await page.evaluate((t) => {
    const bar = document.createElement('div');
    bar.setAttribute('aria-hidden', 'true');
    bar.textContent = t;
    bar.style.cssText = 'position:fixed;left:50%;bottom:28px;transform:translateX(-50%);z-index:2147483647;pointer-events:none;'
      + 'padding:12px 22px;border-radius:14px;background:rgba(10,10,10,.88);color:#fff;font:600 24px/1.3 "Liberation Sans",Arial,sans-serif;';
    document.body.appendChild(bar);
  }, text);
}
